using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using FocLauncherApp.Threading;

namespace FocLauncherApp.WaitDialog
{
    internal sealed class WaitDialogServiceWrapper : DisposableObject, IWaitDialog
    {
        private readonly object _syncObject = new object();
        private WaitDialog _instance;
        private bool _isUiSuppressed;
        private bool _isDialogStarted;

        public void EndWaitDialog(out bool canceled)
        {
            canceled = CloseDialogHelper();
        }

        public void StartWaitDialog(string caption, string waitMessage, string progressText, bool isCancelable, int delayToShowDialog,
            bool showProgress, CancellationTokenSource cts)
        {
            StartWaitDialogHelper(caption, waitMessage, progressText, delayToShowDialog, isCancelable,
                showProgress, 0, 0, new CancellationCallback(cts));
        }

        protected override void DisposeNativeResources()
        {
            if (ThreadHelper.CheckAccess())
                HideDialog();
            else
                ReleaseDialogInstance();
            base.DisposeNativeResources();
        }

        private void HideDialog()
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(HideDialog));
            if (!_isUiSuppressed)
                _instance?.CloseDialog();
        }

        private bool CloseDialogHelper()
        {
            if (!ThreadHelper.CheckAccess())
            {
                var flag = false;
                ThreadHelper.Generic.Invoke(() => flag = CloseDialogHelper());
                return false;
            }
            ThreadHelper.ThrowIfNotOnUIThread(nameof(CloseDialogHelper));
            lock (_syncObject)
            {
                if (!_isDialogStarted)
                    Marshal.ThrowExceptionForHR(-2147418113);
                var flag = false;
                if (_instance != null)
                    flag = _instance.IsCancelled;
                HideDialog();
                ReleaseDialogInstance();
                _isDialogStarted = false;
                return flag;
            }
        }

        private void ReleaseDialogInstance()
        {
            lock (_syncObject)
            {
                if (_instance == null)
                    return;
                WaitDialog.ReleaseInstance(_instance);
                _instance = null;
            }
        }

        private void StartWaitDialogHelper(string caption, string waitMessage, string progressText,
            int delayToShowDialog, bool isCancellable, bool isProgressVisible,
            int currentStepCount = 0, int totalStepCount = -1, IWaitDialogCallback callback = null)
        {
            if (!ThreadHelper.CheckAccess())
            {
                ThreadHelper.Generic.Invoke(() =>
                    {
                        StartWaitDialogHelper(caption, waitMessage, progressText, delayToShowDialog, 
                            isCancellable, isProgressVisible, currentStepCount, totalStepCount, callback);
                    });
            }
            else
            {
                ThreadHelper.ThrowIfNotOnUIThread(nameof(StartWaitDialogHelper));
                lock (_syncObject)
                {
                    if (_instance == null)
                        _instance = WaitDialog.AcquireInstance();
                    if (_instance == null)
                        _isDialogStarted = true;
                    else
                    {
                        _isUiSuppressed = IsUiSuppressed();
                        if (!_isUiSuppressed)
                        {
                            try
                            {
                                var args = new DialogShowArguments(caption, waitMessage, progressText, isCancellable,
                                    isProgressVisible, currentStepCount, totalStepCount)
                                {
                                    AppMainWindowHandle = GetPrimaryWindowHandle()
                                };
                                _instance.Show(TimeSpan.FromSeconds(delayToShowDialog), args, callback);
                                _isDialogStarted = true;
                            }
                            catch
                            {
                                WaitDialog.ReleaseInstance(_instance);
                                _instance = null;
                                throw;
                            }
                        }
                        else
                            _isDialogStarted = true;
                    }
                }
            }
        }

        private static IntPtr GetPrimaryWindowHandle()
        {
            return Application.Current == null ? IntPtr.Zero : new WindowInteropHelper(Application.Current.MainWindow).Handle;
        }

        private bool IsUiSuppressed()
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(IsUiSuppressed));
            return false;
        }

        private class CancellationCallback : IWaitDialogCallback
        {
            private readonly CancellationTokenSource _cancellationSource;

            internal CancellationCallback(CancellationTokenSource cancellationSource)
            {
                _cancellationSource = cancellationSource;
            }

            public void OnCanceled()
            {
                _cancellationSource.Cancel();
            }
        }
    }
}