using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FocLauncherHost.Updater.Component;
using FocLauncherHost.Updater.Configuration;
using FocLauncherHost.Updater.Download;
using FocLauncherHost.Updater.FileSystem;

namespace FocLauncherHost.Updater.Tasks
{
    internal sealed class ComponentDownloadTask : SynchronizedUpdaterTask
    {
        public const string NewFileExtension = ".new";
        internal static readonly long AdditionalSizeBuffer = 20000000;

        private readonly ProgressUpdateCallback _progress;

        public Uri FailedDownloadUri { get; set; }

        public Uri Uri { get; }

        public string DownloadPath { get; private set; }

        public ComponentDownloadTask(IComponent component)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
            if (component.OriginInfo?.Origin == null)
                throw new ArgumentNullException(nameof(OriginInfo));
            Uri = component.OriginInfo.Origin;
        }

        public override string ToString()
        {
            return $"Downloading component '{Component.Name}' form \"{Uri}\"";
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            var directoryName = Path.GetDirectoryName(Component.Destination);
            if (string.IsNullOrEmpty(directoryName))
                throw new InvalidOperationException("Unable to determine a download directory");
            Directory.CreateDirectory(directoryName);

            if (UpdateConfiguration.Instance.BackupPolicy != BackupPolicy.NotRequired && UpdateConfiguration.Instance.DownloadOnlyMode)
                BackupComponent();


            Exception lastException = null;
            if (!token.IsCancellationRequested)
                DownloadAction(token, out lastException);

            token.ThrowIfCancellationRequested();

            if (lastException != null)
            {
                var action = lastException is ValidationFailedException ? "validate download" : "download";
                Logger.Error(lastException, $"Failed to {action} from '{Uri}'. {lastException.Message}");
                throw lastException;
            }
        }

        private void BackupComponent()
        {
            try
            {
                BackupManager.Instance.CreateBackup(Component);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Creating backup of {Component.Name} failed.");
                if (UpdateConfiguration.Instance.BackupPolicy == BackupPolicy.Required)
                {
                    Logger.Trace("Cancelling update operation due to BackupPolicy");
                    throw;
                }
            }
        }

        private void DownloadAction(CancellationToken token, out Exception? lastException)
        {
            lastException = null;
            var downloadManager = DownloadManager.Instance;

            for (var i = 0; i <= UpdateConfiguration.Instance.DownloadRetryCount; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                try
                {
                    var downloadPath = CalculateDownloadPath();
                    DownloadPath = downloadPath;
                    ComponentDownloadPathStorage.Instance.Add(Component, DownloadPath);

                    DownloadAndVerifyAsync(downloadManager, DownloadPath, token).Wait();
                    if (!File.Exists(DownloadPath))
                    {
                        var message = "File not found after being successfully downloaded and verified: " +
                                      DownloadPath + ", package: " + Component.Name;
                        Logger.Warn(message);
                        throw new FileNotFoundException(message, DownloadPath);
                    }

                    lastException = null;

                    if (UpdateConfiguration.Instance.DownloadOnlyMode)
                    {
                        Component.CurrentState = CurrentState.Installed;
                        ComponentDownloadPathStorage.Instance.Remove(Component);
                    }
                    else
                        Component.CurrentState = CurrentState.Downloaded;

                    break;
                }
                catch (OperationCanceledException ex)
                {
                    lastException = ex;
                    Logger.Warn($"Download of {Uri} was cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException && ex.IsExceptionType<OperationCanceledException>())
                    {
                        lastException = ex;
                        Logger.Warn($"Download of {Uri} was cancelled.");
                        break;
                    }
                    var wrappedException = ex.TryGetWrappedException();
                    if (wrappedException != null)
                        ex = wrappedException;
                    if (ex is UnauthorizedAccessException unauthorizedAccessException)
                    {
                        lastException = ex;//new ElevationRequireException(ex);
                        Logger.Error(ex, $"Failed to download \"{Uri}\" to {DownloadPath}: {ex.Message}");
                        Elevator.Instance.RequestElevation(unauthorizedAccessException);
                        break;
                    }
                    lastException = ex;
                    Logger.Error(ex, $"Failed to download \"{Uri}\" on try {i}: {ex.Message}");
                }
            }
        }

        private string CalculateDownloadPath()
        {
            if (UpdateConfiguration.Instance.DownloadOnlyMode)
            {
                var destination = Component.GetFilePath(); 
                return destination;
            }

            var randomFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var backupFileName = $"{Component.Name}.{randomFileName}{NewFileExtension}";
            return Path.Combine(Component.Destination, backupFileName);
        }

        private async Task DownloadAndVerifyAsync(IDownloadManager downloadManager, string destination, CancellationToken token)
        {
            try
            {
                using var file = new FileStream(destination, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                await downloadManager.DownloadAsync(Uri, file, status => _progress?.Invoke(status), token, Component, true);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    Logger.Trace($"Deleting potentially partially downloaded file '{destination}' generated as a result of operation cancellation.");
                    File.Delete(destination);
                }
                catch (Exception e)
                {
                    Logger.Trace($"Could not delete partially downloaded file '{destination}' due to exception: {e}");
                }
                throw;
            }
        }

        // TODO: This has to be a precheck, because of parallel download tasks
        private static void ValidateEnoughDiskSpaceAvailable(IComponent component)
        {
            var option = DiskSpaceCalculator.CalculationOption.Download;
            if (UpdateConfiguration.Instance.DownloadOnlyMode)
            {
                option |= DiskSpaceCalculator.CalculationOption.Install;
                if (UpdateConfiguration.Instance.BackupPolicy != BackupPolicy.Disable)
                    option |= DiskSpaceCalculator.CalculationOption.Backup;
            }

            DiskSpaceCalculator.ThrowIfNotEnoughDiskSpaceAvailable(component, AdditionalSizeBuffer, option);
        }
    }
}