using System;
using System.Threading;
using System.Windows;

namespace FocLauncherApp.WaitDialog
{
    internal class WaitDialogWindowInternalService : MarshalByRefObject, IDisposable
    {
        private Application _application;
        private bool _disposed;
        private WaitWindowDialog _window;
        private WaitDialogDataSource _dataSource;
        private string _hostAppName;
        private bool _isDialogAcquired;
        private Action _onCancelAction;

        ~WaitDialogWindowInternalService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Initialize(DialogInitializationArguments args, ICancelHandler cancelHandler)
        {
            _onCancelAction = () => OnDialogCancelled(cancelHandler);
            var thread = new Thread(() => InitializeTask(args));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        internal void Show(DialogShowArguments args)
        {
            try
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ShowDialogTask));
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

                _application.Dispatcher?.Invoke(() => ShowDialogTask(args));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        internal void Close()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Close));
            try
            {
                if (_application == null)
                    return;
                if (!_isDialogAcquired)
                    return;
                _application?.Dispatcher?.Invoke(() => { _window?.HideDialog(); });
                _isDialogAcquired = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void InitializeTask(DialogInitializationArguments args)
        {
            _hostAppName = args.AppName;
            _dataSource = new WaitDialogDataSource();
            _window = new WaitWindowDialog(args.AppMainWindowHandle, args.AppProcessId)
            {
                DataContext = _dataSource
            };
            _window.Cancelled += OnDialogCancelled;
            StartApplication();
        }

        private void StartApplication()
        {
            if (_application != null)
                return;
            _application = new Application();
            _application.Run();
        }

        private void ShowDialogTask(DialogShowArguments args)
        {
            _window.TryShowDialog(args.AppMainWindowHandle, args.ActiveWindowHandle, args.RootWindowCaption);
        }

        private void OnDialogCancelled(object sender, EventArgs e)
        {
            _application?.Dispatcher?.Invoke(() => _onCancelAction?.Invoke());
        }
        
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _application?.Dispatcher?.Invoke(() =>
                {
                    _window?.Close();
                    _application?.Shutdown();
                });
                _window = null;
                _application = null;
                _isDialogAcquired = false;
            }
            _disposed = true;
        }

        private static void OnDialogCancelled(ICancelHandler cancelHandler)
        {
            cancelHandler?.OnCancel();
        }
    }
}