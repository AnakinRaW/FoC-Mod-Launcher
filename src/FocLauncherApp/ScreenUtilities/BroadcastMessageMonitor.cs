using System;
using System.Windows.Interop;

namespace FocLauncherApp.ScreenUtilities
{
    public class BroadcastMessageMonitor
    {
        public event EventHandler DisplayChange;
        public event EventHandler Activated;
        public event EventHandler Deactivated;

        private static BroadcastMessageMonitor _instance;
        private HwndSource _hwndSource;
        private bool _isActive;

        public static BroadcastMessageMonitor Instance => _instance ??= new BroadcastMessageMonitor();

        public bool IsActive
        {
            get
            {
                return HwndSource == null ? IsApplicationActive() : _isActive;
            }
            private set
            {
                if (_isActive == value)
                    return;
                _isActive = value;
                if (_isActive)
                    OnActivated();
                else
                    OnDeactivated();
            }
        }

        internal HwndSource HwndSource
        {
            get => _hwndSource;
            set
            {
                if (_hwndSource == value)
                    return;
                _hwndSource?.RemoveHook(WndProcHook);
                _hwndSource = value;
                if (_hwndSource == null)
                    return;
                _hwndSource.AddHook(WndProcHook);
                IsActive = IsApplicationActive();
            }
        }

        private IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 28:
                    IsActive = wParam != IntPtr.Zero;
                    break;
                case 126:
                    OnDisplayChange();
                    break;
            }
            return IntPtr.Zero;
        }

        private static bool IsApplicationActive()
        {
            var foregroundWindow = NativeMethods.NativeMethods.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return false;
            NativeMethods.NativeMethods.GetWindowThreadProcessId(foregroundWindow, out var processId);
            var currentProcessId = NativeMethods.NativeMethods.GetCurrentProcessId();
            return (int)processId == (int)currentProcessId;
        }

        private void OnActivated()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        private void OnDeactivated()
        {
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        private void OnDisplayChange()
        {
            DisplayChange?.Invoke(this, EventArgs.Empty);
        }
    }
}