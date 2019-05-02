using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
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
        private DialogInitializationArguments _initializationArguments;
        private static int _currentInstanceId;
        private readonly CancelHandler _cancelHandler;
        private readonly Lazy<DialogService> _service;



        public bool IsCancelled { get; private set; }

        public WaitDialog()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _cancelHandler = new CancelHandler(this);
            _service = new Lazy<DialogService>(GetService);
        }

        private DialogService GetService()
        {
            var service = new DialogService(_cancelHandler);
            var dispatcher = Application.Current.Dispatcher;
            dispatcher.Invoke(() => service.Initialize(_initializationArguments));
            return service;
        }

        public static void ReleaseInstance(WaitDialog instance)
        {
            if (_sharedInstance == null || _sharedInstance != instance)
                return;
            _sharedInstance.CloseDialog();
            _isInstanceAcquired = false;
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
            if (_isDialogActive)
                Task.Run(() => _service.Value.CloseDialog());
            _isDialogActive = false;
            _cancellationCallback = null;
        }

        public void Show(TimeSpan delayToShowDialog, DialogShowArguments args, IWaitDialogCallback callback = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(Show));
            EnsureInitialized();
            var activeWindow = NativeMethods.NativeMethods.GetActiveWindow();
            var text = NativeMethods.NativeMethods.GetWindowText(GetRootOwnerWindow(activeWindow));
            args.SetActiveWindowArgs(text, activeWindow);
            IsCancelled = false;
            _cancellationCallback = callback;

            ShowDialogInternalAsync(delayToShowDialog, args).Forget();
        }

        private void EnsureInitialized()
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(EnsureInitialized));
            if (_initializationArguments == null)
            {
                _initializationArguments = new DialogInitializationArguments
                {
                    AppMainWindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle,
                    AppProcessId = Process.GetCurrentProcess().Id,
                    AppName = "FoC Launcher"
                };
            }
        }

        public void UpdateDialog(DialogUpdateArguments dialogUpdateArguments)
        {
            if (!_isDialogActive)
                _dialogArguments?.Merge(dialogUpdateArguments);
            else
                Task.Run(() => _service.Value.UpdateDialog(dialogUpdateArguments));
        }

        private async Task ShowDialogInternalAsync(TimeSpan delayToShowDialog, DialogShowArguments showArguments)
        {
            var instanceId = _currentInstanceId;
            _dialogArguments = showArguments;
            await Task.Delay(delayToShowDialog).ConfigureAwait(false);
            if (instanceId != _currentInstanceId || !_isInstanceAcquired)
                return;
            _isDialogActive = true;
            await Task.Run(() => _service.Value.ShowDialog(showArguments));
        }

        private void OnDialogCancellation()
        {
            IsCancelled = true;
            Task.Run(() => _cancellationCallback?.OnCanceled()).Forget();
        }

        private static IntPtr GetRootOwnerWindow(IntPtr handle)
        {
            while (true)
            {
                var window = NativeMethods.NativeMethods.GetWindow(handle, 4);
                if (!(window == IntPtr.Zero))
                    handle = window;
                else
                    break;
            }
            return handle;
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

        private class DialogService : DisposableObject
        {
            private Dispatcher _dispatcher;
            private readonly Action _onCancelAction;

            private string _hostAppName;
            private WaitDialogDataSource _dataSource;
            private WaitWindowDialog _window;
            private bool _isDialogAcquired;

            public DialogService(ICancelHandler cancelHandler)
            {
                _onCancelAction = cancelHandler.OnCancel;
            }

            public void Initialize(DialogInitializationArguments args)
            {
                if (_dispatcher != null)
                    return;
                _dispatcher = BackgroundDispatcher.GetBackgroundDispatcher("WaitDialog");
                _dispatcher.Invoke(() =>
                {
                    _hostAppName = args.AppName;
                    _dataSource = new WaitDialogDataSource();
                    _window = new WaitWindowDialog(args.AppMainWindowHandle, args.AppProcessId);
                    _window.Cancelled += OnDialogCancelled;
                    _window.DataContext = _dataSource;
                });
            }

            public void ShowDialog(DialogShowArguments args)
            {
                _dispatcher.Invoke(() =>
                {
                    if (_dataSource == null)
                        return;
                    _isDialogAcquired = true;
                    _dataSource.Caption = string.IsNullOrEmpty(args.Caption) ? _hostAppName : args.Caption;
                    _dataSource.WaitMessage = string.IsNullOrEmpty(args.WaitMessage) ? _hostAppName : args.WaitMessage;
                    _dataSource.ProgressMessage = args.ProgressMessage;
                    _dataSource.IsCancellable = args.IsCancellable;
                    _dataSource.IsProgressVisible = args.IsProgressVisible;
                    _dataSource.ShowMarqueeProgress = args.ShowMarqueeProgress;
                    if (_dataSource.IsProgressVisible && !_dataSource.ShowMarqueeProgress)
                    {
                        _dataSource.CurrentStep = args.CurrentStepCount;
                        _dataSource.TotalSteps = args.TotalStepCount;
                    }
                    _window.TryShowDialog(args.AppMainWindowHandle, args.ActiveWindowHandle, args.RootWindowCaption);
                });
            }

            public void CloseDialog()
            {
                ThrowIfDisposed();
                _dispatcher.Invoke(() =>
                {
                    if (!_isDialogAcquired)
                        return;
                    _window.HideDialog();
                    _isDialogAcquired = false;
                });
            }

            protected override void DisposeManagedResources()
            {
                _dispatcher?.InvokeShutdown();
                _dispatcher = null;
                _isDialogAcquired = false;
                _window = null;
                base.DisposeManagedResources();
            }

            private void OnDialogCancelled(object sender, EventArgs e)
            {
                var action = _onCancelAction;
                action?.Invoke();
            }

            public void UpdateDialog(DialogUpdateArguments args)
            {
                ThrowIfDisposed();
                _dispatcher.Invoke(() =>
                {
                    if (!_isDialogAcquired || _dataSource == null)
                        return;
                    if (!string.IsNullOrEmpty(args.WaitMessage))
                        _dataSource.WaitMessage = args.WaitMessage;
                    _dataSource.ProgressMessage = args.ProgressMessage;
                    _dataSource.IsCancellable = args.IsCancellable;
                    if (!_dataSource.IsProgressVisible || _dataSource.ShowMarqueeProgress)
                        return;
                    _dataSource.CurrentStep = args.CurrentStepCount;
                    _dataSource.TotalSteps = args.TotalStepCount;
                });
            }
        }
    }

    internal class WaitDialogDataSource : INotifyPropertyChanged
    {
        private string _caption;
        private string _waitMessage;
        private string _progressMessage;
        private bool _isProgressVisible;
        private bool _showMarqueeProgress;
        private bool _isCancellable;
        private int _currentStep;
        private int _totalSteps;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Caption
        {
            get => _caption;
            set
            {
                if (value == _caption) return;
                _caption = value;
                OnPropertyChanged();
            }
        }

        public string WaitMessage
        {
            get => _waitMessage;
            set
            {
                if (value == _waitMessage) return;
                _waitMessage = value;
                OnPropertyChanged();
            }
        }

        public string ProgressMessage
        {
            get => _progressMessage;
            set
            {
                if (value == _progressMessage) return;
                _progressMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set
            {
                if (value == _isProgressVisible) return;
                _isProgressVisible = value;
                OnPropertyChanged();
            }
        }

        public bool ShowMarqueeProgress
        {
            get => _showMarqueeProgress;
            set
            {
                if (value == _showMarqueeProgress) return;
                _showMarqueeProgress = value;
                OnPropertyChanged();
            }
        }

        public bool IsCancellable
        {
            get => _isCancellable;
            set
            {
                if (value == _isCancellable) return;
                _isCancellable = value;
                OnPropertyChanged();
            }
        }

        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (value == _currentStep) return;
                _currentStep = value;
                OnPropertyChanged();
            }
        }

        public int TotalSteps
        {
            get => _totalSteps;
            set
            {
                if (value == _totalSteps) return;
                _totalSteps = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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