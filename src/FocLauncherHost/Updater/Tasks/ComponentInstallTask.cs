using System;
using System.Threading;
using FocLauncherHost.Updater.Component;
using FocLauncherHost.Updater.FileSystem;

namespace FocLauncherHost.Updater.Tasks
{
    internal class ComponentInstallTask : SynchronizedUpdaterTask
    {
        internal static readonly long AdditionalSizeBuffer = 20000000;

        internal ComponentAction Action { get; }

        internal InstallResult Result { get; set; }

        internal bool? RestartRequired { get; private set; }

        public ComponentInstallTask(IComponent component, ComponentAction action)
        {
            Component = component;
            Action = action;
        }

        public override string ToString()
        {
            return $"{Action}ing \"{Component.Name}\"";
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            if (Action == ComponentAction.Keep)
                return;
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

                    }
                    else if (Action == ComponentAction.Delete)
                    {
                        Result = installer.Remove(Component ,token);
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
                    //SetRestartManagerRebootState(RebootType.FinalReboot);
                }

                if (Result.IsFailure())
                    throw new UpdaterException($"Component '{Component.Name}' failed to {Component.RequiredAction}");
                if (Result == InstallResult.Cancel)
                    throw new OperationCanceledException();
            }
            finally
            {
                // remove progress event
            }
        }

        private static void ValidateEnoughDiskSpaceAvailable(IComponent component)
        {
            if (component.RequiredAction == ComponentAction.Keep)
                return;
            foreach (var diskData in new DiskSpaceCalculator(component, AdditionalSizeBuffer).CalculatedDiskSizes)
            {
                if (!diskData.Value.HasEnoughDiskSpace)
                    throw new OutOfDiskspaceException(
                        $"There is not enough space to install “{component.Name}”. {diskData.Key} is required on drive {diskData.Value.RequestedSize + AdditionalSizeBuffer} " +
                        $"but you only have {diskData.Value.AvailableDiskSpace} available.");
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
                    throw new OperationCanceledException();
                }
            }

        }
    }
}