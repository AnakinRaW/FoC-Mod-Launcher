using System;
using System.IO;
using System.Threading;
using FocLauncherHost.Updater.Component;
using FocLauncherHost.Updater.Configuration;
using FocLauncherHost.Updater.FileSystem;

namespace FocLauncherHost.Updater.Tasks
{
    internal class ComponentInstallTask : SynchronizedUpdaterTask
    {
        private readonly ComponentDownloadTask _download;
        internal static readonly long AdditionalSizeBuffer = 20000000;
        private readonly bool _isPresent;

        internal ComponentAction Action { get; }

        internal InstallResult Result { get; set; }

        internal bool? RestartRequired { get; private set; }

        public virtual TimeSpan DownloadWaitTime { get; internal set; } = new TimeSpan(0L);

        public ComponentInstallTask(IComponent component, ComponentAction action, ComponentDownloadTask download, bool isPresent = false) :
            this(component, action, isPresent)
        {
            _download = download;
        }

        public ComponentInstallTask(IComponent component, ComponentAction action, bool isPresent = false)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
            Action = action;
            _isPresent = isPresent;
        }

        public override string ToString()
        {
            return $"{Action}ing \"{Component.Name}\"";
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            if (Action == ComponentAction.Keep)
            {
                Result = InstallResult.Success;
                return;
            }

            var now = DateTime.Now;
            _download?.Wait();
            DownloadWaitTime += DateTime.Now - now;
            if (_download?.Error != null)
            {
                Logger.Warn($"Skipping {Action} of '{Component.Name}' since downloading it failed.");
                return;
            }

            var installer = FileInstaller.Instance;
            try
            {
                try
                {
                    ValidateEnoughDiskSpaceAvailable(Component);

                    if (UpdateConfiguration.Instance.BackupPolicy != BackupPolicy.Disable)
                        BackupComponent();

                    if (Action == ComponentAction.Update)
                    {
                        string localPath;
                        if (_download != null)
                            localPath = _download.DownloadPath;
                        else if (Component.CurrentState == CurrentState.Downloaded && ComponentDownloadPathStorage.Instance.TryGetValue(Component, out var downloadedFile))
                            localPath = downloadedFile;
                        else
                            throw new FileNotFoundException("Unable to find the downloaded file.");

                        Result = installer.Install(Component, token, localPath, _isPresent);
                    }
                    else if (Action == ComponentAction.Delete)
                    {
                        Result = installer.Remove(Component, token, _isPresent);
                    }

                }
                catch (OutOfDiskspaceException)
                {
                    Result = InstallResult.Failure;
                    throw;
                }

                if (Result == InstallResult.SuccessRestartRequired)
                {
                    RestartRequired = true;
                }

                if (Result.IsFailure())
                    throw new ComponentFailedException(new[] { Component });
                if (Result == InstallResult.Cancel)
                    throw new OperationCanceledException();
            }
            finally
            {
            }
        }

        private static void ValidateEnoughDiskSpaceAvailable(IComponent component)
        {
            if (component.RequiredAction == ComponentAction.Keep)
                return;
            var option = DiskSpaceCalculator.CalculationOption.All;
            if (component.CurrentState == CurrentState.Downloaded)
                option &= ~DiskSpaceCalculator.CalculationOption.Download;
            if (UpdateConfiguration.Instance.BackupPolicy == BackupPolicy.Disable)
                option &= ~DiskSpaceCalculator.CalculationOption.Backup;
            DiskSpaceCalculator.ThrowIfNotEnoughDiskSpaceAvailable(component, AdditionalSizeBuffer, option);
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
    }
}