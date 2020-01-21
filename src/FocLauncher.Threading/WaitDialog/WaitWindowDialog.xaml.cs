using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using FocLauncher.NativeMethods;
using FocLauncher.ScreenUtilities;

namespace FocLauncher.WaitDialog
{
    public partial class WaitWindowDialog
    {
        public event EventHandler Cancelled;

        private static readonly TimeSpan RetryTimerDelay = TimeSpan.FromMilliseconds(250.0);
        private static readonly IntPtr TopMost = new IntPtr(-1);
        private static readonly IntPtr Bottom = new IntPtr(1);

        private readonly WindowInteropHelper _interopHelper;
        private readonly IntPtr _dialogWindowHandle;
        private IntPtr _hostMainWindowHandle;
        private IntPtr _hostActiveWindowHandle;
        private readonly int _hostProcessId;
        private readonly TraceSource _logger;
        private readonly DispatcherTimer _dispatcherTimer;
        private string _hostRootWindowCaption;

        public WaitWindowDialog(IntPtr hostMainWindowHandle, int hostProcessId, TraceSource logger)
        {
            InitializeComponent();
            _hostMainWindowHandle = hostMainWindowHandle;
            _hostProcessId = hostProcessId;
            _logger = logger;
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _interopHelper = new WindowInteropHelper(this);
            _dialogWindowHandle = _interopHelper.EnsureHandle();
        }

        public void TryShowDialog(IntPtr hostMainWindowHandle, IntPtr hostActiveWindowHandle, string rootWindowCaption)
        {
            _hostMainWindowHandle = hostMainWindowHandle;
            _hostActiveWindowHandle = hostActiveWindowHandle;
            _hostRootWindowCaption = rootWindowCaption;
            LogInfo("Enter", nameof(TryShowDialog));
            if (CanShowDialog())
                PositionAndShowDialog();
            if (_hostActiveWindowHandle != IntPtr.Zero)
            {
                _dispatcherTimer.Interval = RetryTimerDelay;
                _dispatcherTimer.Start();
            }
            else
                _dispatcherTimer.Stop();
        }

        public void HideDialog()
        {
            LogInfo("Enter", nameof(HideDialog));
            _dispatcherTimer.Stop();
            Hide();
        }

        private void WaitDialogWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            NativeMethods.NativeMethods.SetWindowLong(_dialogWindowHandle, (int) Gwl.Style, NativeMethods.NativeMethods.GetWindowLong(_dialogWindowHandle, (int) Gwl.Style) & -524289 | int.MinValue);
            NativeMethods.NativeMethods.SetProp(_dialogWindowHandle, "UIA_WindowPatternEnabled", new IntPtr(1));
            NativeMethods.NativeMethods.SetProp(_dialogWindowHandle, "UIA_WindowVisibilityOverridden", new IntPtr(1));
        }

        private void CaptionArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException ex)
            {
                Log(TraceEventType.Error, ex.Message, nameof(CaptionArea_MouseLeftButtonDown));
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                LogInfo("Not visible, try show dialog", nameof(DispatcherTimer_Tick));
                TryShowDialog(_hostMainWindowHandle, _hostActiveWindowHandle, _hostRootWindowCaption);
            }
            else if (CanShowDialog())
            {
                LogInfo("Visible, can show, handle ghost window", nameof(DispatcherTimer_Tick));
                NativeMethods.NativeMethods.SetWindowPos(_dialogWindowHandle, TopMost, 0, 0, 0, 0, 19);
            }
            else
            {
                LogInfo("Visible, cannot show, set to top most window", nameof(DispatcherTimer_Tick));
                var window = NativeMethods.NativeMethods.GetWindow(_hostActiveWindowHandle, 3);
                if (NativeMethods.NativeMethods.SetWindowPos(_dialogWindowHandle, window != IntPtr.Zero ? window : Bottom, 0, 0, 0, 0, 19))
                    return;
                NativeMethods.NativeMethods.SetWindowPos(_dialogWindowHandle, Bottom, 0, 0, 0, 0, 19);
            }
        }

        private bool CanShowDialog()
        {
            if (_hostMainWindowHandle == IntPtr.Zero || !NativeMethods.NativeMethods.IsWindowVisible(_hostActiveWindowHandle))
            {
                LogInfo("Main not exists or hidden", nameof(CanShowDialog));
                return true;
            }

            if (!IsHostProcessForeground())
            {
                LogInfo("Host process background", nameof(CanShowDialog));
                return false;
            }

            var hostCurrentActiveWindow = GetMainThreadActiveWindow(_hostActiveWindowHandle);
            LogInfo(() => $"Current active HWND = {hostCurrentActiveWindow}", nameof(CanShowDialog));
            return hostCurrentActiveWindow == _hostActiveWindowHandle;
        }

        private void PositionAndShowDialog()
        {
            LogInfo(() =>
            {
                if (DataContext is WaitDialogDataSource dataContext)
                    return "Caption = " + dataContext.Caption + ", WaitMsg = " + dataContext.WaitMessage;
                return "Enter";
            }, nameof(PositionAndShowDialog));
            Topmost = true;
            var hostWindowRect = new RECT();
            if (_hostActiveWindowHandle == IntPtr.Zero || !NativeMethods.NativeMethods.GetWindowRect(_hostActiveWindowHandle, out hostWindowRect) ||
                (hostWindowRect.Width == 0 || hostWindowRect.Height == 0))
                NativeMethods.NativeMethods.GetWindowRect(NativeMethods.NativeMethods.GetDesktopWindow(), out hostWindowRect);
            var rect = hostWindowRect.ToRect();
            var display = Screen.FindDisplayForWindowRect(rect);
            var twdDeviceWidth = (int)Screen.LogicalToDeviceUnitsX(display, 468);
            var twdDeviceHeight = (int)Screen.LogicalToDeviceUnitsY(display, 124);
            var twdDeviceTop = (int)(rect.Top + (rect.Height - twdDeviceHeight) / 2.0);
            var twdDeviceLeft = (int)(rect.Left + (rect.Width - twdDeviceWidth) / 2.0);
            Screen.SetInitialWindowRect(_dialogWindowHandle, this, new Int32Rect(twdDeviceLeft, twdDeviceTop, twdDeviceWidth, twdDeviceHeight));
            LogInfo(() =>
                $"Host window rect = L{hostWindowRect.Left}, T{hostWindowRect.Top}, W{hostWindowRect.Width}, H{hostWindowRect.Height}", nameof(PositionAndShowDialog));
            LogInfo(() => $"Display = {display}", nameof(PositionAndShowDialog));
            LogInfo(() =>
                $"Initial window rect = L{twdDeviceLeft}, T{twdDeviceTop}, W{twdDeviceWidth}, H{twdDeviceHeight}", nameof(PositionAndShowDialog));
            Show();
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
            IntPtr rootOwnerWindow = GetRootOwnerWindow(_hostActiveWindowHandle);
            NativeMethods.NativeMethods.GetWindowRect(candidateHandle, out var lpRect1);
            NativeMethods.NativeMethods.GetWindowRect(rootOwnerWindow, out var lpRect2);
            if (lpRect1.Size != lpRect2.Size)
                return false;
            return NativeMethods.NativeMethods.GetWindowText(candidateHandle).StartsWith(_hostRootWindowCaption);
        }

        private void LogInfo(string message = "Enter", [CallerMemberName] string callerName = null)
        {
            Log(TraceEventType.Information, message, callerName);
        }

        private void LogInfo(Func<string> getMessage, [CallerMemberName] string callerName = null)
        {
            if (getMessage == null || _logger == null)
                return;
            if (!_logger.Switch.ShouldTrace(TraceEventType.Information))
                return;
            string message;
            try
            {
                message = getMessage();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            Log(TraceEventType.Information, message, callerName);
        }

        private void Log(TraceEventType traceEventType, string message, [CallerMemberName] string callerName = null)
        {
            _logger?.TraceEvent(traceEventType, 0, $"TWDWindow (Main HWND {_hostMainWindowHandle}, Active HWND {_hostActiveWindowHandle}) - {callerName}: {message}");
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

        private static IntPtr GetMainThreadActiveWindow(IntPtr activeWindowHandle)
        {
            var windowThreadProcessId = NativeMethods.NativeMethods.GetWindowThreadProcessId(activeWindowHandle, out _);
            var threadInfo = new GuiThreadInfo
            {
                CbSize = Marshal.SizeOf(typeof(GuiThreadInfo))
            };
            if (windowThreadProcessId != 0U && NativeMethods.NativeMethods.GetGUIThreadInfo(windowThreadProcessId, out threadInfo))
                return threadInfo.HwndActive;
            return IntPtr.Zero;
        }
    }
}
