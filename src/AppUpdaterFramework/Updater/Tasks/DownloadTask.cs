using System;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Configuration;
using AnakinRaW.AppUpdaterFramework.Elevation;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Storage;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;
using AnakinRaW.AppUpdaterFramework.Utilities;
using AnakinRaW.CommonUtilities.DownloadManager;
using AnakinRaW.CommonUtilities.DownloadManager.Verification;
using AnakinRaW.CommonUtilities.DownloadManager.Verification.HashVerification;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Updater.Tasks;

internal class DownloadTask : SynchronizedTask, IProgressTask
{
    private readonly IUpdateConfiguration _updateConfiguration;
    private readonly IFileSystem _fileSystem;
    private readonly DownloadRepository _downloadRepository;

    public ProgressType Type => ProgressType.Download;
    public ITaskProgressReporter ProgressReporter { get; }

    public IFileInfo DownloadPath { get; private set; } = null!;

    public long Size { get; }

    public Uri Uri { get; }

    public IInstallableComponent Component { get; }

    IProductComponent IComponentTask.Component => Component;

    public DownloadTask(
        IInstallableComponent installable, 
        ITaskProgressReporter progressReporter, 
        IUpdateConfiguration updateConfiguration,
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Requires.NotNull(installable, nameof(installable));
        Requires.NotNull(progressReporter, nameof(progressReporter));
        Requires.NotNull(updateConfiguration, nameof(updateConfiguration));

        Component = installable;
        ProgressReporter = progressReporter;
        Size = installable.DownloadSize;
        Uri = installable.OriginInfo!.Url;

        _updateConfiguration = updateConfiguration;
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _downloadRepository = serviceProvider.GetRequiredService<DownloadRepository>();
    }

    public override string ToString()
    {
        return $"Downloading component '{Component.GetUniqueId()}' form \"{Uri}\"";
    }

    protected override void SynchronizedInvoke(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;
        try
        {
            ReportProgress(0.0);
            
            Exception? lastException = null;
            if (!token.IsCancellationRequested)
                DownloadAction(token, out lastException);

            token.ThrowIfCancellationRequested();

            if (lastException != null)
            {
                var action = lastException is VerificationFailedException ? "validate download" : "download";
                Logger?.LogError(lastException, $"Failed to {action} from '{Uri}'. {lastException.Message}");
                throw lastException;
            }
        }
        finally
        {
            ReportProgress(1.0);
        }
    }

    private void ReportProgress(double progress)
    {
        ProgressReporter.Report(this, progress);
    }

    private void DownloadAction(CancellationToken token, out Exception? lastException)
    {
        lastException = null;
        var downloadManager = Services.GetRequiredService<IDownloadManager>();

        try
        {
            var downloadPath = _downloadRepository.AddComponent(Component);
            DownloadPath = downloadPath;


            for (var i = 0; i < _updateConfiguration.DownloadRetryCount; i++)
            {
                if (token.IsCancellationRequested)
                    break;

                try
                {
                    DownloadAndVerifyAsync(downloadManager, DownloadPath, token).Wait(CancellationToken.None);
                    DownloadPath.Refresh();
                    if (!DownloadPath.Exists)
                    {
                        var message = "File not found after being successfully downloaded and verified: " +
                                      DownloadPath + ", package: " + Component.GetDisplayName();
                        Logger?.LogWarning(message);
                        throw new FileNotFoundException(message, DownloadPath.FullName);
                    }

                    lastException = null;

                    break;
                }
                catch (OperationCanceledException ex)
                {
                    lastException = ex;
                    Logger?.LogWarning($"Download of '{Uri}' was cancelled.");
                    break;
                }
                catch (Exception e) when (e.IsExceptionType<UnauthorizedAccessException>())
                {
                    ExceptionDispatchInfo.Capture(e.InnerException!).Throw();
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException && ex.IsExceptionType<OperationCanceledException>())
                    {
                        lastException = ex;
                        Logger?.LogWarning($"Download of {Uri} was cancelled.");
                        break;
                    }
                    var wrappedException = ex.TryGetWrappedException();
                    if (wrappedException != null)
                        ex = wrappedException;
                    lastException = ex;
                    Logger?.LogError(ex, $"Failed to download \"{Uri}\" on try {i}: {ex.Message}");
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            lastException = ex;
            Logger?.LogError(ex, $"Failed to create download path '{DownloadPath}' due to missing permission: {ex.Message}");
            var elevationManager = Services.GetRequiredService<IElevationManager>();
            elevationManager.SetElevationRequest();
        }
    }

    private async Task DownloadAndVerifyAsync(IDownloadManager downloadManager, IFileInfo destination, CancellationToken token)
    {
        var integrityInformation = Component.OriginInfo!.IntegrityInformation;
        var hashContext = new HashVerificationContext(integrityInformation.Hash, HashType.None);

        try
        {
            using var file = destination.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            await downloadManager.DownloadAsync(Uri, file, OnProgress, hashContext, token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                Logger?.LogTrace(
                    $"Deleting potentially partially downloaded file '{destination}' generated as a result of operation cancellation.");
                destination.Delete();
            }
            catch (Exception e)
            {
                Logger?.LogTrace($"Could not delete partially downloaded file '{destination}' due to exception: {e}");
            }

            throw;
        }
    }

    private void OnProgress(ProgressUpdateStatus status)
    {
        var progress = (double) status.BytesRead / Size;
        ReportProgress(progress);
    }
}