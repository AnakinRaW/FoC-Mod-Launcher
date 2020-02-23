using System.Collections.Generic;
using System.Diagnostics;
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
            UpdateMessageBox.ProcessKillResult result;
            do
            {
                token.ThrowIfCancellationRequested();
                var launcherId = Process.GetCurrentProcess().Id;
                var processes = WithoutProcess(processManager.GetProcesses(), launcherId).
                    Where(x => x.ApplicationStatus == ApplicationStatus.Running).ToList();
                if (!processes.Any())
                    return true;
                var dialog = new UpdateMessageBox(processes);
                dialog.ShowDialog();
                result = dialog.Result;
            } while (result == UpdateMessageBox.ProcessKillResult.Retry);

            return result == UpdateMessageBox.ProcessKillResult.Kill;
        }

        private static IEnumerable<ILockingProcessInfo> WithoutProcess(IEnumerable<ILockingProcessInfo> processes,
            int processId)
        {
            return processes.Where(x => !x.Id.Equals(processId));
        }
    }
}