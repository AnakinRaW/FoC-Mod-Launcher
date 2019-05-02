using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using FocLauncherApp.NativeMethods;
using FocLauncherApp.Utilities;

namespace FocLauncherApp.WaitDialog
{
    public partial class WaitWindowDialog
    {
        private static readonly TimeSpan RetryTimerDelay = TimeSpan.FromMilliseconds(250.0);

        private IntPtr _hostMainWindowHandle;
        private readonly int _hostProcessId;
        private readonly DispatcherTimer _dispatcherTimer;
        private IntPtr _hostActiveWindowHandle;
        private string _hostRootWindowCaption;
        private IntPtr _dialogWindowHandle;
        private WindowInteropHelper _interopHelper;

        public event EventHandler Cancelled;

        public WaitWindowDialog(IntPtr hostMainWindowHandle, int hostProcessId)
        {
            InitializeComponent();
            _hostMainWindowHandle = hostMainWindowHandle;
            _hostProcessId = hostProcessId;
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            Closing += OnClosing;
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _interopHelper = new WindowInteropHelper(this);
            _dialogWindowHandle = _interopHelper.Handle;
            NativeMethods.NativeMethods.SetWindowLong(_dialogWindowHandle, (int)Gwl.Style, NativeMethods.NativeMethods.GetWindowLong(_dialogWindowHandle, (int)Gwl.Style) & -524289 | int.MinValue);
            NativeMethods.NativeMethods.SetProp(_dialogWindowHandle, "UIA_WindowPatternEnabled", new IntPtr(1));
            NativeMethods.NativeMethods.SetProp(_dialogWindowHandle, "UIA_WindowVisibilityOverridden", new IntPtr(1));
        }

        public void TryShowDialog(IntPtr hostMainWindowHandle, IntPtr hostActiveWindowHandle, string rootWindowCaption)
        {
            _hostActiveWindowHandle = hostActiveWindowHandle;
            _hostMainWindowHandle = hostMainWindowHandle;
            _hostRootWindowCaption = rootWindowCaption;

            if (CanShowDialog())
                PositionAndShowDialog();

            if (_hostMainWindowHandle != IntPtr.Zero)
            {
                _dispatcherTimer.Interval = RetryTimerDelay;
                _dispatcherTimer.Start();
            }
            else
                _dispatcherTimer.Stop();

        }

        public void HideDialog()
        {
            _dispatcherTimer.Stop();
            Hide();
        }

        protected virtual void OnCancelled()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        private void PositionAndShowDialog()
        {
            Topmost = true;
            if (_hostActiveWindowHandle == IntPtr.Zero || !NativeMethods.NativeMethods.GetWindowRect(_hostActiveWindowHandle, out var lpRect) || (lpRect.Width == 0 || lpRect.Height == 0))
                NativeMethods.NativeMethods.GetWindowRect(NativeMethods.NativeMethods.GetDesktopWindow(), out lpRect);
            var logicalUnits = new Rect(lpRect.Left, lpRect.Top, lpRect.Width, lpRect.Height).DeviceToLogicalUnits();
            var num = double.IsNaN(Height) ? MinHeight : Height;
            Top = logicalUnits.Top + (logicalUnits.Height - num) / 2.0;
            Left = logicalUnits.Left + (logicalUnits.Width - Width) / 2.0;
            try
            {
                Show();
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException && e.HResult == -2146233079)
                    return;
                throw;
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!IsVisible)
                TryShowDialog(_hostMainWindowHandle, _hostActiveWindowHandle, _hostRootWindowCaption);
            else if (CanShowDialog())
            {
                NativeMethods.NativeMethods.SetWindowPos(_dialogWindowHandle, new IntPtr(-1), 0, 0, 0, 0, 19);
            }
            else
            {
                var window = NativeMethods.NativeMethods.GetWindow(_hostActiveWindowHandle, 3);
                if (NativeMethods.NativeMethods.SetWindowPos(_dialogWindowHandle, window != IntPtr.Zero ? window : new IntPtr(1), 0, 0, 0, 0,
                    19))
                    return;
                NativeMethods.NativeMethods.SetWindowPos(_dialogWindowHandle, new IntPtr(1), 0, 0, 0, 0, 19);
            }
        }

        private bool CanShowDialog()
        {
            if (_hostMainWindowHandle == IntPtr.Zero || !NativeMethods.NativeMethods.IsWindowVisible(_hostMainWindowHandle))
                return true;
            if (!IsHostProcessForeground())
                return false;
            return GetMainThreadActiveWindow(_hostActiveWindowHandle) == _hostActiveWindowHandle;
        }

        private bool IsHostProcessForeground()
        {
            var foregroundWindow = NativeMethods.NativeMethods.GetForegroundWindow();
            NativeMethods.NativeMethods.GetWindowThreadProcessId(foregroundWindow, out var processId);
            if (processId == _hostProcessId || foregroundWindow == _dialogWindowHandle)
                return true;
            return IsGhostWindow(foregroundWindow);
        }

        private bool IsGhostWindow(IntPtr candidateHandle)
        {
            if (candidateHandle == IntPtr.Zero)
                return false;
            var lpString = new StringBuilder();
            if (NativeMethods.NativeMethods.GetClassName(candidateHandle, lpString, 6) != 5 || lpString.ToString() != "Ghost")
                return false;
            candidateHandle = GetRootOwnerWindow(candidateHandle);
            var rootOwnerWindow = GetRootOwnerWindow(_hostActiveWindowHandle);
            NativeMethods.NativeMethods.GetWindowRect(candidateHandle, out var lpRect1);
            NativeMethods.NativeMethods.GetWindowRect(rootOwnerWindow, out var lpRect2);
            return lpRect1.Size == lpRect2.Size && NativeMethods.NativeMethods.GetWindowText(candidateHandle).StartsWith(_hostRootWindowCaption);
        }

        private static IntPtr GetMainThreadActiveWindow(IntPtr activeWindowHandle)
        {
            var windowThreadProcessId = NativeMethods.NativeMethods.GetWindowThreadProcessId(activeWindowHandle, out _);
            var lpgui = new GuiThreadInfo
            {
                CbSize = Marshal.SizeOf(typeof(GuiThreadInfo))
            };
            if (windowThreadProcessId != 0U && NativeMethods.NativeMethods.GetGUIThreadInfo(windowThreadProcessId, out lpgui))
                return lpgui.HwndActive;
            return IntPtr.Zero;
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

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }


        private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
