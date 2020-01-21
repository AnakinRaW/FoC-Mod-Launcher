using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using FocLauncherApp.NativeMethods;
using FocLauncherApp.ScreenUtilities;


namespace FocLauncherApp.WaitDialog
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
        private readonly DispatcherTimer _dispatcherTimer;
        private string _hostRootWindowCaption;

        public WaitWindowDialog(IntPtr hostMainWindowHandle, int hostProcessId)
        {
            InitializeComponent();
            _hostMainWindowHandle = hostMainWindowHandle;
            _hostProcessId = hostProcessId;
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
            catch (InvalidOperationException)
            {
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!IsVisible)
                TryShowDialog(_hostMainWindowHandle, _hostActiveWindowHandle, _hostRootWindowCaption);
            else if (CanShowDialog())
                NativeMethods.NativeMethods.SetWindowPos(_dialogWindowHandle, TopMost, 0, 0, 0, 0, 19);
            else
            {
                var window = NativeMethods.NativeMethods.GetWindow(_hostActiveWindowHandle, 3);
                if (NativeMethods.NativeMethods.SetWindowPos(_dialogWindowHandle, window != IntPtr.Zero ? window : Bottom, 0, 0, 0, 0, 19))
                    return;
                NativeMethods.NativeMethods.SetWindowPos(_dialogWindowHandle, Bottom, 0, 0, 0, 0, 19);
            }
        }

        private bool CanShowDialog()
        {
            if (_hostMainWindowHandle == IntPtr.Zero || !NativeMethods.NativeMethods.IsWindowVisible(_hostActiveWindowHandle))
                return true;

            if (!IsHostProcessForeground())
                return false;

            var hostCurrentActiveWindow = GetMainThreadActiveWindow(_hostActiveWindowHandle);
            return hostCurrentActiveWindow == _hostActiveWindowHandle;
        }


        private static void TransformToPixels(Visual visual, double unitX, double unitY, out int pixelX, out int pixelY)
        {
            Matrix matrix;
            var source = PresentationSource.FromVisual(visual);
            if (source != null)
            {
                matrix = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using var src = new HwndSource(new HwndSourceParameters());
                matrix = src.CompositionTarget.TransformToDevice;
            }

            pixelX = (int)(matrix.M11 * unitX);
            pixelY = (int)(matrix.M22 * unitY);
        }

        private void PositionAndShowDialog()
        {
            Topmost = true;
            if (_hostActiveWindowHandle == IntPtr.Zero || !NativeMethods.NativeMethods.GetWindowRect(_hostActiveWindowHandle, out var hostWindowRect) || 
                hostWindowRect.Width == 0 || hostWindowRect.Height == 0)
                NativeMethods.NativeMethods.GetWindowRect(NativeMethods.NativeMethods.GetDesktopWindow(), out hostWindowRect);
            var rect = hostWindowRect.ToRect();

            var twdDeviceWidth = (int)Screen.LogicalToDeviceUnitsX(_hostActiveWindowHandle, 468);
            var twdDeviceHeight = (int)Screen.LogicalToDeviceUnitsY(_hostActiveWindowHandle, 124);
            var twdDeviceTop = (int)(rect.Top + (rect.Height - twdDeviceHeight) / 2.0);
            var twdDeviceLeft = (int)(rect.Left + (rect.Width - twdDeviceWidth) / 2.0);
            Screen.SetInitialWindowRect(_dialogWindowHandle, this, new Int32Rect(twdDeviceLeft, twdDeviceTop, twdDeviceWidth, twdDeviceHeight));
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
            var rootOwnerWindow = GetRootOwnerWindow(_hostActiveWindowHandle);
            NativeMethods.NativeMethods.GetWindowRect(candidateHandle, out var lpRect1);
            NativeMethods.NativeMethods.GetWindowRect(rootOwnerWindow, out var lpRect2);
            if (lpRect1.Size != lpRect2.Size)
                return false;
            return NativeMethods.NativeMethods.GetWindowText(candidateHandle).StartsWith(_hostRootWindowCaption);
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
