using System;
using System.Threading.Tasks;
using FocLauncherApp.Threading;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace FocLauncherApp.WaitDialog
{
    internal static class WaitDialogHelper
    {
        private static readonly TimeSpan DefaultWaitDialogDelay = TimeSpan.FromSeconds(2.0);


        public static void Run(this JoinableTaskFactory joinableTaskFactory, string waitCaption, string waitMessage, string progressText, Func<Task> asyncMethod, int waitSeconds)
        {
            //Validate.IsNotNull(joinableTaskFactory, nameof(joinableTaskFactory));
            //Validate.IsNotNullAndNotEmpty(waitCaption, nameof(waitCaption));
            //Validate.IsNotNull(asyncMethod, nameof(asyncMethod));
            ThreadHelper.ThrowIfNotOnUIThread(nameof(Run));

            var twd = WaitDialogFactory.CreateInstance();
            Assumes.Present(twd);
            twd.StartWaitDialog(waitCaption, waitMessage, progressText, waitSeconds, false, true);
            try
            {
                joinableTaskFactory.Run(asyncMethod);
            }
            finally
            {
                twd?.EndWaitDialog(out _);
            }
        }
    }
}
