using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using FocLauncher.Theming;
using FocLauncher.Threading;
using Microsoft.VisualStudio.Threading;

namespace FocLauncher.WaitDialog
{
    internal sealed class WaitDialog : MarshalByRefObject
    {
        private readonly IServiceProvider _serviceProvider;
        private static WaitDialog _sharedInstance;
        private static bool _isInstanceAcquired;
        private IWaitDialogCallback _cancellationCallback;
        private bool _isDialogActive;
        private DialogShowArguments _dialogArguments;
        private DialogInitializationArguments _initializationArguments;
        private static int _currentInstanceId;
        private readonly ICancelHandler _cancelHandler;
        private bool _disposed;
        private WaitDialogWindowInternalService _provider;
        private AppDomain _appDomain;

        public bool IsCancelled { get; private set; }

        public WaitDialog(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Application.Current.Exit += OnApplicationExit;
            _cancelHandler = new CancelHandler(this);

            if (!(_serviceProvider.GetService(typeof(IThemeManager)) is IThemeManager themeManager))
                return;
            themeManager.ThemeChanged += OnThemeChanged;
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
                _sharedInstance = new WaitDialog(LauncherServiceProvider.Instance);
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

        public void UpdateDialog(DialogUpdateArguments updateArguments)
        {
            if (_disposed)
                return;
            if (!_isDialogActive)
                _dialogArguments?.Merge(updateArguments);
            _provider.Update(updateArguments);
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

                AppDomain.Unload(_appDomain);
            }
            _disposed = true;
        }

        private async Task ShowInternalAsync(TimeSpan delayToShowDialog, DialogShowArguments showArguments)
        {
            var localInstanceId = _currentInstanceId;
            _dialogArguments = showArguments;
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
            Dispose(true);
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
                _initializationArguments.CancelText = "Cancel";
                InitializeFontAndColorInformation();
            }
            if (_provider != null)
                return;
            CreateAndStartAppDomain();
        }

        private void InitializeFontAndColorInformation()
        {
            if (_serviceProvider.GetService(typeof(IThemeManager)) is IThemeManager themeManager)
            {
                _initializationArguments.BackgroundColor = themeManager.GetThemedColorRgba(EnvironmentColors.WaitWindowBackground);
                _initializationArguments.TextColor = themeManager.GetThemedColorRgba(EnvironmentColors.WaitWindowText);
                _initializationArguments.CaptionBackgroundColor = themeManager.GetThemedColorRgba(EnvironmentColors.WaitWindowCaptionBackground);
                _initializationArguments.CaptionTextColor = themeManager.GetThemedColorRgba(EnvironmentColors.WaitWindowCaptionText);
                _initializationArguments.BorderColor = themeManager.GetThemedColorRgba(EnvironmentColors.WaitWindowBorder);
            }
            else
            {
                _initializationArguments.BackgroundColor = 4143380223U;
                _initializationArguments.TextColor = 505290495U;
                _initializationArguments.CaptionBackgroundColor = 4025479935U;
                _initializationArguments.CaptionTextColor = 505290495U;
                _initializationArguments.BorderColor = 8047871U;
            }
        }

        private void CreateAndStartAppDomain()
        {
            var info = new AppDomainSetup {ApplicationBase = LauncherConstants.ApplicationBasePath};
            _appDomain = AppDomain.CreateDomain("LauncherWaitDialog", null, info);

            // Since we may execute this code from an embedded assembly typeof().Assembly.Location might return "". 
            // Thus we need to tell the AppDomain where to look at on disk.
            var location = Path.Combine(LauncherConstants.ApplicationBasePath, GetType().Assembly.GetName().Name) + ".dll";
            if (!File.Exists(location))
                throw new FileNotFoundException("Required assembly not found!", location);

            var provider = (WaitDialogWindowInternalService) _appDomain.CreateInstanceFromAndUnwrap(location, typeof(WaitDialogWindowInternalService).FullName);
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

        private void OnThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            if (_initializationArguments != null)
            {
                InitializeFontAndColorInformation();
                _provider.UpdateDialogStyle(_initializationArguments);
            }
        }

        [Serializable]
        internal class CancelHandler : ICancelHandler
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