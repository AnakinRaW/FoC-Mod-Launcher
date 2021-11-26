using System;
using System.Threading;
using FocLauncher.WaitDialog;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;

namespace FocLauncher.Threading
{
    public static class JoinableTaskFactoryExtensions
    {
        private static readonly TimeSpan DefaultWaitDialogDelay = TimeSpan.FromSeconds(2.0);

        public static void Run(this JoinableTaskFactory joinableTaskFactory, string waitCaption,
            Func<IProgress<WaitDialogProgressData>, CancellationToken, Task> asyncMethod,
            TimeSpan? delayToShowDialog = null)
        {
            Requires.NotNull(joinableTaskFactory, nameof(joinableTaskFactory));
            Requires.NotNullOrEmpty(waitCaption, nameof(waitCaption));
            Requires.NotNull(asyncMethod, nameof(asyncMethod));
            ThreadHelper.ThrowIfNotOnUIThread(nameof(Run));
            var waitDialogFactory = WaitDialogFactory.Instance;
            Assumes.Present(waitDialogFactory);
            var initialProgress = new WaitDialogProgressData(null, null, null, true);
            using var twd = waitDialogFactory.StartWaitDialog(waitCaption, initialProgress,
                delayToShowDialog ?? DefaultWaitDialogDelay);
            joinableTaskFactory.Run(() => asyncMethod(twd.Progress, twd.UserCancellationToken));
        }

        public static void Run(this JoinableTaskFactory joinableTaskFactory, string waitCaption, Func<IProgress<WaitDialogProgressData>, Task> asyncMethod, TimeSpan? delayToShowDialog = null)
        {
            Requires.NotNull(joinableTaskFactory, nameof(joinableTaskFactory));
            Requires.NotNullOrEmpty(waitCaption, nameof(waitCaption));
            Requires.NotNull(asyncMethod, nameof(asyncMethod));
            ThreadHelper.ThrowIfNotOnUIThread(nameof(Run));
            var waitDialogFactory = WaitDialogFactory.Instance;
            Assumes.Present(waitDialogFactory);
            using var twd = waitDialogFactory.StartWaitDialog(waitCaption, null, delayToShowDialog ?? DefaultWaitDialogDelay);
            joinableTaskFactory.Run(() => asyncMethod(twd.Progress));
        }

        public static void Run(this JoinableTaskFactory joinableTaskFactory, string waitCaption, string waitMessage,
            Func<IProgress<WaitDialogProgressData>, CancellationToken, Task> asyncMethod,
            TimeSpan? delayToShowDialog = null)
        {
            Requires.NotNull(joinableTaskFactory, nameof(joinableTaskFactory));
            Requires.NotNullOrEmpty(waitMessage, nameof(waitMessage));
            Requires.NotNull(asyncMethod, nameof(asyncMethod));
            ThreadHelper.ThrowIfNotOnUIThread(nameof(Run));
            var waitDialogFactory = WaitDialogFactory.Instance;
            Assumes.Present(waitDialogFactory);
            var initialProgress = new WaitDialogProgressData(waitMessage, null, null, true);
            using var twd = waitDialogFactory.StartWaitDialog(waitCaption, initialProgress,
                delayToShowDialog ?? DefaultWaitDialogDelay);
            joinableTaskFactory.Run(() => asyncMethod(twd.Progress, twd.UserCancellationToken));
        }

        public static void Run(this JoinableTaskFactory joinableTaskFactory, string waitCaption, string waitMessage,
            Func<IProgress<WaitDialogProgressData>, Task> asyncMethod, TimeSpan? delayToShowDialog = null)
        {
            Requires.NotNull(joinableTaskFactory, nameof(joinableTaskFactory));
            Requires.NotNullOrEmpty(waitMessage, nameof(waitMessage));
            Requires.NotNull(asyncMethod, nameof(asyncMethod));
            ThreadHelper.ThrowIfNotOnUIThread(nameof(Run));
            var waitDialogFactory = WaitDialogFactory.Instance;
            Assumes.Present(waitDialogFactory);
            var initialProgress = new WaitDialogProgressData(waitMessage);
            using var twd = waitDialogFactory.StartWaitDialog(waitCaption, initialProgress,
                delayToShowDialog ?? DefaultWaitDialogDelay);
            joinableTaskFactory.Run(() => asyncMethod(twd.Progress));
        }
    }
}
