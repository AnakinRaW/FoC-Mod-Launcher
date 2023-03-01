﻿using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;
using System.Threading;
using System;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Updater.Backup;
using AnakinRaW.AppUpaterFramework.Updater.Progress;
using AnakinRaW.AppUpaterFramework.Updater.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Validation;
using AnakinRaW.AppUpaterFramework.Updater.Installer;
using AnakinRaW.AppUpaterFramework.Utilities;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

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
        installer.Progress += OnInstallerProgress;

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

            if (Result.IsFailure())
                throw new ComponentFailedException(new[] { Component });
            if (Result == InstallResult.Cancel)
                throw new OperationCanceledException();

        }
        catch (OutOfDiskspaceException)
        {
            Result = InstallResult.Failure;
            throw;
        }
        finally
        {
            installer.Progress -= OnInstallerProgress;
        }
    }

    private void BackupComponent()
    {
        if (_currentComponent is null)
            return;
        
        var backupManager = Services.GetRequiredService<IBackupManager>();
        
        try
        {
            backupManager.BackupComponent(_currentComponent);
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