using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;

namespace FocLauncher.WaitDialog
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

        internal void Update(DialogUpdateArguments args)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Close));
            if (!_isDialogAcquired || _dataSource == null)
                return;
            if (!string.IsNullOrEmpty(args.WaitMessage))
                _dataSource.WaitMessage = args.WaitMessage;
            _dataSource.ProgressMessage = args.ProgressMessage;
            _dataSource.IsCancellable = args.IsCancellable;
            _dataSource.ShowMarqueeProgress = args.ShowMarqueeProgress;
            if (!_dataSource.IsProgressVisible || _dataSource.ShowMarqueeProgress)
                return;
            _dataSource.CurrentStep = args.CurrentStepCount;
            _dataSource.TotalSteps = args.TotalStepCount;
        }

        private void InitializeTask(DialogInitializationArguments args)
        {
            _hostAppName = args.AppName;
            _dataSource = new WaitDialogDataSource();
            _window = new WaitWindowDialog(args.AppMainWindowHandle, args.AppProcessId, null)
            {
                DataContext = _dataSource
            };
            _window.Cancelled += OnDialogCancelled;
            UpdateDialogStyle(args);
            StartApplication();
        }

        private void StartApplication()
        {
            var t = AppDomain.CurrentDomain.FriendlyName;
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

        private void UpdateDialogStyle(DialogInitializationArguments args)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UpdateDialogStyle));
            if (_dataSource == null)
                return;
            _dataSource.ForegroundColorBrush = new SolidColorBrush(args.TextColor.ToColorFromRgba());
            _dataSource.BackgroundColorBrush = new SolidColorBrush(args.BackgroundColor.ToColorFromRgba());
            _dataSource.BorderColorBrush = new SolidColorBrush(args.BorderColor.ToColorFromRgba());
            _dataSource.CaptionBackgroundColorBrush = new SolidColorBrush(args.CaptionBackgroundColor.ToColorFromRgba());
            _dataSource.CaptionForegroundColorBrush = new SolidColorBrush(args.CaptionTextColor.ToColorFromRgba());
            _dataSource.CancelText = args.CancelText;
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