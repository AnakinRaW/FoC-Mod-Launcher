using System;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncherApp.Threading;

namespace FocLauncherApp.WaitDialog
{
    internal class WaitDialog
    {
        private static WaitDialog _sharedInstance;
        private static bool _isInstanceAcquired;
        private IWaitDialogCallback _cancellationCallback;
        private bool _isDialogActive;
        private DialogShowArguments _dialogArguments;
        private CancellationTokenSource _queueCancellationTokenSource;
        private DialogInitializationArguments _initializationArguments;
        private static int _currentInstanceId;

        private readonly CancelHandler _cancelHandler;

        public bool IsCancelled { get; private set; }

        public WaitDialog()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _cancelHandler = new CancelHandler(this);
        }

        public static void ReleaseInstance(WaitDialog instance)
        {

        }

        public static WaitDialog AcquireInstance()
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(AcquireInstance));
            if (_isInstanceAcquired)
                return null;
            if (_sharedInstance == null)
                _sharedInstance = new WaitDialog();
            _isInstanceAcquired = true;
            ++_currentInstanceId;
            return _sharedInstance;
        }

        public void CloseDialog()
        {

        }

        public void Show(TimeSpan delayToShowDialog, DialogShowArguments args, IWaitDialogCallback callback = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(Show));

            IsCancelled = false;
            _cancellationCallback = callback;
            ShowDialogInternalAsync(delayToShowDialog, args).Forget();
        }

        public void UpdateDialog(DialogUpdateArguments dialogUpdateArguments)
        {

        }

        private async Task ShowDialogInternalAsync(TimeSpan delayToShowDialog, DialogShowArguments showArguments)
        {
            var instanceId = _currentInstanceId;
            _dialogArguments = showArguments;
            await Task.Delay(delayToShowDialog).ConfigureAwait(false);
            if (instanceId != _currentInstanceId || !_isInstanceAcquired)
                return;
            _isDialogActive = true;



            await Task.Run(() =>
            {
                var d = Application.Current.Dispatcher;
                d.Invoke(() => new WaitWindowDialog().ShowDialog());
            });
        }

        private void OnDialogCancellation()
        {
            IsCancelled = true;
            Task.Run(() => _cancellationCallback?.OnCanceled()).Forget();
        }

        private class CancelHandler : ICancelHandler
        {
            private readonly WaitDialog _owner;

            public CancelHandler(WaitDialog owner)
            {
                _owner = owner;
            }

            public void OnCancel()
            {
                _owner?.OnDialogCancellation();
            }
        }
    }

    public interface ICancelHandler
    {
        void OnCancel();
    }

    public class DialogInitializationArguments
    {
        public string AppName { get; set; }

        public IntPtr AppMainWindowHandle { get; set; }

        public int AppProcessId { get; set; }
    }
}