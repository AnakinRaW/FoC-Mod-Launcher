using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using FocLauncherApp.Threading;

namespace FocLauncherApp.WaitDialog
{
    internal sealed class WaitDialog : MarshalByRefObject
    {
        private static WaitDialog _sharedInstance;
        private static bool _isInstanceAcquired;
        private IWaitDialogCallback _cancellationCallback;
        private bool _isDialogActive;
        private DialogInitializationArguments _initializationArguments;
        private static int _currentInstanceId;
        private readonly CancelHandler _cancelHandler;
        private bool _disposed;
        private WaitDialogWindowInternalService _provider;

        public bool IsCancelled { get; private set; }

        public WaitDialog()
        {
            Application.Current.Exit += OnApplicationExit;
            _cancelHandler = new CancelHandler(this);
        }
        
        ~WaitDialog()
        {
            Dispose(false);
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

        public void Show(TimeSpan delayToShowDialog, DialogShowArguments showArguments, IWaitDialogCallback callback = null)
        {
            if (_disposed)
                return;
            if (showArguments == null)
                throw new ArgumentNullException(nameof(showArguments));
            ThreadHelper.ThrowIfNotOnUIThread(nameof(Show));
            EnsureInitialized();

            var activeWindow = NativeMethods.NativeMethods.GetActiveWindow();
            var windowText = NativeMethods.NativeMethods.GetWindowText(GetRootOwnerWindow(activeWindow));
            showArguments.SetActiveWindowArgs(windowText, activeWindow);
            IsCancelled = false;
            _cancellationCallback = callback;
            ShowInternalAsync(delayToShowDialog, showArguments).Forget();
        }

        public void CloseDialog()
        {
            if (_disposed)
                return;
            if (_isDialogActive)
                _provider.Close();
            _isDialogActive = false;
            _cancellationCallback = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _provider?.Dispose();
                _provider = null;

                if (Application.Current?.MainWindow != null)
                    Application.Current.MainWindow.Closed -= OnApplicationExit;
            }
            _disposed = true;
        }

        private async Task ShowInternalAsync(TimeSpan delayToShowDialog, DialogShowArguments showArguments)
        {
            var localInstanceId = _currentInstanceId;
            await Task.Delay(delayToShowDialog).ConfigureAwait(false);
            if (localInstanceId != _currentInstanceId || !_isInstanceAcquired)
                return;
            _isDialogActive = true;
            _provider.Show(showArguments);
        }

        private void OnDialogCancellation()
        {
            IsCancelled = true;
            Task.Run(() => _cancellationCallback?.OnCanceled()).Forget();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            ReleaseInstance(this);
            _provider?.Dispose();
            _provider = null;
        }

        private void EnsureInitialized()
        {
            ThreadHelper.ThrowIfNotOnUIThread(nameof(EnsureInitialized));
            if (_initializationArguments == null)
            {
                _initializationArguments = new DialogInitializationArguments();
                _initializationArguments.AppMainWindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                _initializationArguments.AppProcessId = Process.GetCurrentProcess().Id;
                _initializationArguments.AppName = "FoC Launcher";
                InitializeFontAndColorInformation();
            }
            if (_provider != null)
                return;
            CreateAndStartAppDomain();
        }

        private void InitializeFontAndColorInformation()
        {
            //_initializationArguments.BackgroundColor =
            //    _themeManager.GetThemedColorRgba(EnvironmentColors.ToolWindowBackground);
            //_initializationArguments.TextColor = _themeManager.GetThemedColorRgba(EnvironmentColors.ToolWindowText);
            //_initializationArguments.CaptionBackgroundColor =
            //    _themeManager.GetThemedColorRgba(EnvironmentColors.MainWindowTitleBarBackground);
            //_initializationArguments.CaptionTextColor =
            //    _themeManager.GetThemedColorRgba(EnvironmentColors.MainWindowTitleBarForeground);
            //_initializationArguments.BorderColor =
            //    _themeManager.GetThemedColorRgba(EnvironmentColors.MainWindowActiveShadowAndBorderColor);
        }

        private void CreateAndStartAppDomain()
        {
            var currentDomainSetup = AppDomain.CurrentDomain.SetupInformation;
            var info = new AppDomainSetup
            {
                ApplicationBase = currentDomainSetup.ApplicationBase,
                ConfigurationFile = currentDomainSetup.ConfigurationFile ?? string.Empty,
                LoaderOptimization = LoaderOptimization.MultiDomain
            };

            var appDomain = AppDomain.CreateDomain("InternalWaitDialog", null, info);

            var provider = (WaitDialogWindowInternalService)appDomain.CreateInstanceFromAndUnwrap(
                typeof(WaitDialogWindowInternalService).Assembly.Location, typeof(WaitDialogWindowInternalService).FullName);
            _provider = provider;
            if (_provider == null)
                throw new InvalidOperationException("Could not create WaitDialogWindowInternal");
            _provider.Initialize(_initializationArguments, _cancelHandler);
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

        private class CancelHandler : MarshalByRefObject, ICancelHandler
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
}