using System.Diagnostics;
using System.Linq;
using System.Threading;
using FocLauncherHost.Dialogs;
using TaskBasedUpdater.Restart;

namespace FocLauncherHost
{
    internal sealed class LauncherRestartManager
    {
        internal static bool ShowElevateDialog()
        {
            var window = new RestartElevatedWindow();
            window.ShowDialog();
            return window.ElevateResult;
        }

        internal static bool ShowProcessKillDialog(ILockingProcessManager processManager, CancellationToken token)
        {
            return ShowKillDialogCore(processManager, true, false, token);
        }

        internal static bool ShowSelfKillDialog(ILockingProcessManager processManager, CancellationToken token)
        {
            return ShowKillDialogCore(processManager, false, true, token);
        }

        internal static bool ShowRestoreDialog()
        {
            var dialog = new RestoreDialog();
            dialog.ShowDialog();
            return dialog.Restore;
        }

        private static bool ShowKillDialogCore(ILockingProcessManager processManager, bool retry, bool showSelf, CancellationToken token)
        {
            var currentProcessId = Process.GetCurrentProcess().Id;
            UpdateMessageBox.ProcessKillResult result;
            do
            {
                token.ThrowIfCancellationRequested();
                var processes = processManager.GetProcesses().Where(x => x.ApplicationStatus == ApplicationStatus.Running).ToList();
                if (!showSelf) 
                    processes = processes.Where(x => x.Id != currentProcessId).ToList();
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