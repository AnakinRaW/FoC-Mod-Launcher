using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using FocLauncher.NativeMethods;
using FocLauncher.Threading;
using FocLauncher.Utilities;

namespace FocLauncher.WaitDialog
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

        public void HasCanceled(out bool canceled)
        {
            var instance = _instance;
            canceled = instance != null && instance.IsCancelled;
        }

        public bool StartWaitDialog(string caption, string waitMessage, string progressText, int delayToShowDialog,
            bool isCancelable, bool showMarqueeProgress)
        {
            var isDialogStarted = _isDialogStarted;
            StartWaitDialogHelper(caption, waitMessage, progressText, delayToShowDialog, isCancelable,
                showMarqueeProgress);
            return !isDialogStarted && _isDialogStarted;
        }

        public void StartWaitDialogWithCallback(string caption, string waitMessage, string progressText,
            bool isCancelable, int delayToShowDialog, bool showProgress, int totalSteps, int currentStep,
            IWaitDialogCallback callback)
        {
            StartWaitDialogHelper(caption, waitMessage, progressText, delayToShowDialog, isCancelable,
                showProgress, currentStep, totalSteps, callback);
        }

        public void StartWaitDialogWithPercentageProgress(string caption, string waitMessage, string progressText,
            int delayToShowDialog, bool isCancelable, int totalSteps, int currentStep)
        {
            StartWaitDialogHelper(caption, waitMessage, progressText, delayToShowDialog, isCancelable,
                isCancelable, currentStep, totalSteps);
        }

        public void UpdateProgress(string waitMessage, string progressText, int currentStep, int totalSteps,
            bool disableCancel, out bool canceled)
        {
            var flag = UpdateDialogHelper(waitMessage, progressText, !disableCancel, currentStep, totalSteps);
            if (UnsafeHelpers.IsOptionalOutParamSet(out canceled))
                return;

            canceled = flag;
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
            if (Application.Current == null)
                return IntPtr.Zero;
            return new WindowInteropHelper(Application.Current.MainWindow).Handle;
        }

        private bool IsUiSuppressed()
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(IsUiSuppressed));
            return false;
        }

        private bool UpdateDialogHelper(string waitMessage, string progressText, bool isCancellable,
            int currentStepCount = 0, int totalStepCount = 0)
        {
            lock (_syncObject)
            {
                if (_instance == null)
                    return false;
                if (_instance != null && _instance.IsCancelled)
                    return true;

                if (!_isUiSuppressed)
                    _instance.UpdateDialog(new DialogUpdateArguments(waitMessage, progressText, isCancellable,
                        currentStepCount, totalStepCount));
                return false;
            }
        }

    }
}