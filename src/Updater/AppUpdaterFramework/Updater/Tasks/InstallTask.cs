using System;
using System.Threading;
using AnakinRaW.AppUpdaterFramework.Configuration;
using AnakinRaW.AppUpdaterFramework.Installer;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Product.Detectors;
using AnakinRaW.AppUpdaterFramework.Restart;
using AnakinRaW.AppUpdaterFramework.Storage;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;
using AnakinRaW.AppUpdaterFramework.Utilities;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Updater.Tasks;

internal class InstallTask : RunnerTask, IProgressTask
{ 
    private readonly IUpdateConfiguration _updateConfiguration;
    private readonly ProductVariables _productVariables;
    private readonly UpdateAction _action;
    private readonly IInstallableComponent? _currentComponent;
    private readonly DownloadTask? _download;
    private readonly IInstallerFactory _installerFactory;

    public IInstallableComponent Component { get; }

    IProductComponent IComponentTask.Component => Component;

    internal InstallResult Result { get; private set; } = InstallResult.Success;

    public ProgressType Type => ProgressType.Install;
    public ITaskProgressReporter ProgressReporter { get; }

    public long Size => Component.InstallationSize.Total;

    public InstallTask(
        IInstallableComponent installable, 
        ITaskProgressReporter progressReporter, 
        IUpdateConfiguration updateConfiguration,
        ProductVariables productVariables,
        IServiceProvider serviceProvider) :
        this(installable, UpdateAction.Delete, progressReporter, updateConfiguration, productVariables, serviceProvider)
    {
    }

    public InstallTask(
        IInstallableComponent installable,
        IInstallableComponent? currentComponent, 
        DownloadTask download, 
        ITaskProgressReporter progressReporter, 
        IUpdateConfiguration updateConfiguration,
        ProductVariables productVariables,
        IServiceProvider serviceProvider) : 
        this(installable, UpdateAction.Update, progressReporter, updateConfiguration, productVariables, serviceProvider)
    {
        Requires.NotNull(download, nameof(download));
        _currentComponent = currentComponent;
        _download = download;
    }

    private InstallTask(
        IInstallableComponent installable, 
        UpdateAction updateAction, 
        ITaskProgressReporter progressReporter, 
        IUpdateConfiguration updateConfiguration,
        ProductVariables productVariables,
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Requires.NotNull(installable, nameof(installable));
        Requires.NotNull(progressReporter, nameof(progressReporter));
        Requires.NotNull(updateConfiguration, nameof(updateConfiguration));

        Component = installable;
        ProgressReporter = progressReporter;

        _action = updateAction;
        _updateConfiguration = updateConfiguration;
        _productVariables = productVariables;
        _installerFactory = serviceProvider.GetRequiredService<IInstallerFactory>();
    }

    public override string ToString()
    {
        return $"{_action}ing \"{Component.GetUniqueId()}\"";
    }

    protected override void RunCore(CancellationToken token)
    {
        _download?.Wait();
        if (_download?.Error != null)
        {
            Logger?.LogWarning($"Skipping {_action} of '{Component.GetUniqueId()}' since downloading it failed.");
            return;
        }

        var installer = _installerFactory.CreateInstaller(Component);
        installer.Progress += OnInstallerProgress!;

        try
        {
            if (_action == UpdateAction.Update)
                ValidateEnoughDiskSpaceAvailable();

            if (_updateConfiguration.BackupPolicy != BackupPolicy.Disable)
                BackupComponent();

            switch (_action)
            {
                case UpdateAction.Update:
                {
                    Assumes.NotNull(_download);
                    var localPath = _download.DownloadPath;
                    Result = installer.Install(Component, localPath, _productVariables, token);
                    break;
                }
                case UpdateAction.Delete:
                    Result = installer.Remove(Component, _productVariables, token);
                    break;
            }

            if (Result == InstallResult.SuccessRestartRequired)
            {
                var restartManager = Services.GetRequiredService<IRestartManager>();
                restartManager.SetRestart(RestartType.ApplicationRestart);
                Logger?.LogWarning($"Component '{Component.GetDisplayName()}' get scheduled for installation after a restart.");
                
                var pendingComponentStore = Services.GetRequiredService<IWritablePendingComponentStore>();
                pendingComponentStore.AddComponent(new PendingComponent
                {
                    Component = Component, 
                    Action = _action
                });
            }

            if (Result == InstallResult.FailureElevationRequired)
            {
                Logger?.LogWarning($"Component '{Component.GetDisplayName()}' was not installed because required permissions are missing.");
                var restartManager = Services.GetRequiredService<IRestartManager>();
                restartManager.SetRestart(RestartType.ApplicationElevation);
            }

            if (Result.IsFailure())
                throw new ComponentFailedException(new[] { this });
            if (Result == InstallResult.Cancel)
                throw new OperationCanceledException();

            if (_updateConfiguration.ValidateInstallation)
                Result = ValidateInstall();

        }
        catch (OutOfDiskspaceException e)
        {
            Logger?.LogError(e, e.Message);
            Result = InstallResult.Failure;
            throw;
        }
        finally
        {
            installer.Progress -= OnInstallerProgress!;
        }
    }

    private InstallResult ValidateInstall()
    {
        var detectorFactory = Services.GetService<IComponentDetectorFactory>() ?? ComponentDetectorFactory.Default;
        var isInstalled = detectorFactory.GetDetector(Component.Type, Services).GetCurrentInstalledState(Component, _productVariables);

        switch (_action)
        {
            case UpdateAction.Update when isInstalled:
            case UpdateAction.Delete when !isInstalled:
                return InstallResult.Success;
            default:
                Logger?.LogWarning($"Validation of installed component '{Component.GetDisplayName()}' failed.");
                return InstallResult.Failure;
        }
    }

    private void BackupComponent()
    {
        if (_action == UpdateAction.Keep)
            return;

        var componentToBackup = _action switch
        {
            UpdateAction.Update => _currentComponent ?? Component,
            UpdateAction.Delete => Component,
            _ => throw new ArgumentOutOfRangeException()
        };

        var backupManager = Services.GetRequiredService<IBackupManager>();
        
        try
        {
            backupManager.BackupComponent(componentToBackup);
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, $"Creating backup of {Component.Id} failed.");
            if (_updateConfiguration.BackupPolicy == BackupPolicy.Required)
            {
                Logger?.LogError("Stopping install due to BackupPolicy");
                throw;
            }
        }
    }

    private void ValidateEnoughDiskSpaceAvailable()
    {
        if (_action == UpdateAction.Keep)
            return;
        var options = DiskSpaceCalculator.CalculationOptions.All;

        var installPath = Component is IPhysicalInstallable physicalInstallable ? physicalInstallable.InstallPath : null;
        if (!string.IsNullOrEmpty(installPath))
            installPath = VariableResolver.Default.ResolveVariables(installPath!, _productVariables.ToDictionary());

        // We already downloaded it, no need to calculate again
        if (_download is not null) 
            options &= ~DiskSpaceCalculator.CalculationOptions.Download;

        if (_updateConfiguration.BackupPolicy == BackupPolicy.Disable)
            options &= ~DiskSpaceCalculator.CalculationOptions.Backup;

        var diskSpaceCalculator = Services.GetRequiredService<IDiskSpaceCalculator>();

        diskSpaceCalculator.ThrowIfNotEnoughDiskSpaceAvailable(Component, _currentComponent, installPath, options);
    }

    private void OnInstallerProgress(object sender, ProgressEventArgs e)
    {
        ProgressReporter.Report(this, e.Progress);
    }
}