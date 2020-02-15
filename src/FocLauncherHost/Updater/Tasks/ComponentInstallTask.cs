using System;
using System.Threading;
using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater.Tasks
{
    internal class ComponentInstallTask : SynchronizedUpdaterTask
    {
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
            var installer = new FileInstaller(Component);
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

        private void ValidateEnoughDiskSpaceAvailable(IComponent component)
        {
            if (component.RequiredAction == ComponentAction.Keep)
                return;
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