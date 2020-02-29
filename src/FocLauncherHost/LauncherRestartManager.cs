using System.Linq;
using System.Threading;
using FocLauncherHost.Controls;
using FocLauncherHost.Updater.Restart;

namespace FocLauncherHost
{
    internal sealed class LauncherRestartManager
    {
        internal static bool ShowProcessKillDialog(ILockingProcessManager processManager, CancellationToken token)
        {
            return ShowKillDialogCore(processManager, true, token);
        }

        internal static bool ShowSelfKillDialog(ILockingProcessManager processManager, CancellationToken token)
        {
            return ShowKillDialogCore(processManager, false, token);
        }

        private static bool ShowKillDialogCore(ILockingProcessManager processManager, bool retry, CancellationToken token)
        {
            UpdateMessageBox.ProcessKillResult result;
            do
            {
                token.ThrowIfCancellationRequested();
                var processes = processManager.GetProcesses().Where(x => x.ApplicationStatus == ApplicationStatus.Running).ToList();
                if (!processes.Any())
                    return true;
                var dialog = new UpdateMessageBox(processes, retry);
                dialog.ShowDialog();
                result = dialog.Result;
            } while (result == UpdateMessageBox.ProcessKillResult.Retry);

            return result == UpdateMessageBox.ProcessKillResult.Kill;
        }
    }
}