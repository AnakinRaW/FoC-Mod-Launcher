using System;
using System.Threading;
using Microsoft.Internal.VisualStudio.Shell;

namespace FocLauncher.WaitDialog
{
    public static class WaitDialogHelper
    {
        /// <summary>
        /// Creates the instance of an <see cref="IWaitDialog"/>.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns>The instance of the <see cref="IWaitDialog"/></returns>
        public static IWaitDialog CreateInstance(this WaitDialogFactory factory)
        {
            Validate.IsNotNull(factory, nameof(factory));
            factory.CreateInstance(out var dialog);
            return dialog;
        }

        /// <summary>
        /// Ends the wait dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <returns>Returns <see langword="true"/> if the task was canceled by the user</returns>
        public static bool EndWaitDialog(this IWaitDialog dialog)
        {
            Validate.IsNotNull(dialog, nameof(dialog));
            dialog.EndWaitDialog(out var canceled);
            return canceled;
        }

        public static Session StartWaitDialog(this WaitDialogFactory factory, string waitCatption,
            WaitDialogProgressData initialProgress = null, TimeSpan delayToShow = default)
        {
            var instance = factory.CreateInstance();
            var cancellationSource = new CancellationTokenSource();
            var progress = new ProgressAdapter(instance, cancellationSource);
            var callback = new CancellationCallback(cancellationSource);
            instance.StartWaitDialogWithCallback(waitCatption, initialProgress?.WaitMessage,
                initialProgress?.ProgressText, initialProgress != null && initialProgress.IsCancelable, (int)delayToShow.TotalSeconds, true, initialProgress?.TotalSteps ?? 0,
                initialProgress?.CurrentStep ?? 0, callback);
            return new Session(instance, progress, cancellationSource.Token, callback);
        }

        public static Session CreateSession(IWaitDialog waitDialog)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var progress = (IProgress<WaitDialogProgressData>)new ProgressAdapter(waitDialog, cancellationTokenSource);
            var cancellationCallback = new CancellationCallback(cancellationTokenSource);
            return new Session(waitDialog, progress, cancellationTokenSource.Token, cancellationCallback);
        }

        public class Session : IDisposable
        {
            private readonly IWaitDialog _dialog;
            private bool _disposed;

            /// <summary>
            /// The callback instance.
            /// </summary>
            public IWaitDialogCallback Callback { get; }

            /// <summary>
            /// The progress report.
            /// </summary>
            public IProgress<WaitDialogProgressData> Progress { get; }

            /// <summary>
            /// The cancellation token.
            /// </summary>
            public CancellationToken UserCancellationToken { get; }

            internal Session(IWaitDialog dialog, IProgress<WaitDialogProgressData> progress, CancellationToken token,
                IWaitDialogCallback callback)
            {
                _dialog = dialog;
                Validate.IsNotNull(progress, nameof(progress));
                Progress = progress;
                UserCancellationToken = token;
                Callback = callback;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;
                _disposed = true;
                _dialog.EndWaitDialog();
            }
        }

        private class CancellationCallback : IWaitDialogCallback
        {
            private readonly CancellationTokenSource _cancellationSource;

            internal CancellationCallback(CancellationTokenSource cancellationSource)
            {
                Validate.IsNotNull(cancellationSource, nameof(cancellationSource));
                _cancellationSource = cancellationSource;
            }

            public void OnCanceled()
            {
                _cancellationSource.Cancel();
            }
        }

        private class ProgressAdapter : IProgress<WaitDialogProgressData>
        {
            private readonly CancellationTokenSource _cancellationTokenSource;
            private readonly IWaitDialog _dialog;

            internal ProgressAdapter(IWaitDialog dialog, CancellationTokenSource cancellationTokenSource)
            {
                Validate.IsNotNull(dialog, nameof(dialog));
                _dialog = dialog;
                _cancellationTokenSource = cancellationTokenSource;
            }

            public void Report(WaitDialogProgressData value)
            {
                if (value == null)
                    return;
                try
                {
                    _dialog.UpdateProgress(value.WaitMessage, value.ProgressText, value.CurrentStep, value.TotalSteps, !value.IsCancelable, out var pfCanceled);

                    if (!pfCanceled || _cancellationTokenSource == null)
                        return;
                    _cancellationTokenSource.Cancel();
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
