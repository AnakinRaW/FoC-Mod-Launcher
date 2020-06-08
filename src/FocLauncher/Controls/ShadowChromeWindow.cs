using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FocLauncher.NativeMethods;
using FocLauncher.ScreenUtilities;
using FocLauncher.Utilities;
using Point = System.Windows.Point;

namespace FocLauncher.Controls
{
    public class ShadowChromeWindow : Window
    {
        private readonly GlowWindow[] _glowWindows = new GlowWindow[4];
        private Rect _logicalSizeForRestore = Rect.Empty;

        public static readonly DependencyProperty ActiveGlowColorProperty =
            DependencyProperty.Register(nameof(ActiveGlowColor), typeof(Color), typeof(ShadowChromeWindow),
                new FrameworkPropertyMetadata(Colors.Transparent, OnGlowColorChanged));

        public static readonly DependencyProperty InactiveGlowColorProperty =
            DependencyProperty.Register(nameof(InactiveGlowColor), typeof(Color), typeof(ShadowChromeWindow),
                new FrameworkPropertyMetadata(Colors.Transparent, OnGlowColorChanged));

        public static readonly DependencyProperty NonClientFillColorProperty =
            DependencyProperty.Register(nameof(NonClientFillColor), typeof(Color), typeof(ShadowChromeWindow),
                new FrameworkPropertyMetadata(Colors.Black));

        private int _lastWindowPlacement;
        private int _deferGlowChangesCount;
        private bool _isGlowVisible;
        private DispatcherTimer _makeGlowVisibleTimer;
        private bool _isNonClientStripVisible;
        private IntPtr _ownerForActivate;
        private Dock _taskbarDirection;
        private bool _isDragging;
        private WindowInteropHelper _windowInteropHelper;
        private bool _useLogicalSizeForRestore;
        private bool _updatingZOrder;

        static ShadowChromeWindow()
        {
            ResizeModeProperty.OverrideMetadata(typeof(ShadowChromeWindow),
                new FrameworkPropertyMetadata(OnResizeModeChanged));
        }
        
        public Color ActiveGlowColor
        {
            get => (Color) GetValue(ActiveGlowColorProperty);
            set => SetValue(ActiveGlowColorProperty, value);
        }

        public Color InactiveGlowColor
        {
            get => (Color) GetValue(InactiveGlowColorProperty);
            set => SetValue(InactiveGlowColorProperty, value);
        }

        public Color NonClientFillColor
        {
            get => (Color) GetValue(NonClientFillColorProperty);
            set => SetValue(NonClientFillColorProperty, value);
        }

        private WindowInteropHelper WindowHelper => _windowInteropHelper ??= new WindowInteropHelper(this);

        protected override void OnActivated(EventArgs e)
        {
            UpdateGlowActiveState();
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            UpdateGlowActiveState();
            base.OnDeactivated(e);
        }

        private static void OnResizeModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((ShadowChromeWindow)obj).DestroyGlowWindows();
            ((ShadowChromeWindow)obj).CreateGlowWindowHandlesNoResize();
            ((ShadowChromeWindow)obj).UpdateGlowVisibility(false);
        }

        private void CreateGlowWindowHandlesNoResize()
        {
            for (var direction = 0; direction < _glowWindows.Length; ++direction)
                GetOrCreateGlowWindow(direction, false).EnsureHandle();
        }
        
        private static void OnGlowColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((ShadowChromeWindow) obj).UpdateGlowColors();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            HwndSource.FromHwnd(WindowHelper.Handle).AddHook(HwndSourceHook);
            CreateGlowWindowHandles();
            base.OnSourceInitialized(e);
        }

        private void CreateGlowWindowHandles()
        {
            for (var direction = 0; direction < _glowWindows.Length; ++direction)
                GetOrCreateGlowWindow(direction).EnsureHandle();
        }

        protected void EnsureOnScreen()
        {
            var deviceRect = this.LogicalToDeviceRect();
            var displayForWindowRect = Screen.FindDisplayForWindowRect(deviceRect);
            var onScreenPosition = Screen.GetOnScreenPosition(deviceRect, displayForWindowRect, true, false);
            User32.SetWindowPos(WindowHelper.Handle, IntPtr.Zero, (int) onScreenPosition.Left,
                (int) onScreenPosition.Top, (int) onScreenPosition.Width, (int) onScreenPosition.Height, 20);
        }

        protected virtual IntPtr HwndSourceHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 6:
                    WmActivate(wParam, lParam);
                    break;
                case 12:
                case 128:
                    return CallDefWindowProcWithoutRedraw(hWnd, msg, wParam, lParam, ref handled);
                case 70:
                    WmWindowPosChanging(hWnd, lParam);
                    break;
                case 71:
                    WmWindowPosChanged(hWnd, lParam);
                    break;
                case 131:
                    return WmNcCalcSize(hWnd, wParam, lParam, ref handled);
                case 132:
                    return WmNcHitTest(hWnd, lParam, ref handled);
                case 133:
                    return WmNcPaint(hWnd, wParam, lParam, ref handled);
                case 134:
                    return WmNcActivate(hWnd, wParam, lParam, ref handled);
                case 164:
                case 165:
                case 166:
                    RaiseNonClientMouseMessageAsClient(hWnd, msg, wParam, lParam);
                    handled = true;
                    break;
                case 174:
                case 175:
                    handled = true;
                    break;
                case 274:
                    WmSysCommand(hWnd, wParam, lParam);
                    break;
                case 561:
                    _isDragging = true;
                    break;
                case 562:
                    _isDragging = false;
                    break;
            }

            return IntPtr.Zero;
        }

        private static void RaiseNonClientMouseMessageAsClient(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam)
        {
            var point = new POINT
            {
                X = NativeMethods.NativeMethods.GetXlParam(lParam.ToInt32()),
                Y = NativeMethods.NativeMethods.GetYlParam(lParam.ToInt32())
            };
            User32.ScreenToClient(hWnd, ref point);
            User32.SendMessage(hWnd, msg + 513 - 161, new IntPtr(PressedMouseButtons),
                NativeMethods.NativeMethods.MakeParam(point.X, point.Y));
        }

        private static int PressedMouseButtons
        {
            get
            {
                var num = 0;
                if (NativeMethods.NativeMethods.IsKeyPressed(1))
                    num |= 1;
                if (NativeMethods.NativeMethods.IsKeyPressed(2))
                    num |= 2;
                if (NativeMethods.NativeMethods.IsKeyPressed(4))
                    num |= 16;
                if (NativeMethods.NativeMethods.IsKeyPressed(5))
                    num |= 32;
                if (NativeMethods.NativeMethods.IsKeyPressed(6))
                    num |= 64;
                return num;
            }
        }

        private IntPtr CallDefWindowProcWithoutRedraw(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            using (new SuppressRedrawScope(hWnd))
            {
                handled = true;
                return User32.DefWindowProc(hWnd, msg, wParam, lParam);
            }
        }

        private void WmActivate(IntPtr wParam, IntPtr lParam)
        {
            if (!(_ownerForActivate != IntPtr.Zero))
                return;
            User32.SendMessage(_ownerForActivate, NativeMethods.NativeMethods.NotifyOwnerActive, wParam, lParam);
        }

        private IntPtr WmNcActivate(IntPtr hWnd, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = true;
            return User32.DefWindowProc(hWnd, 134, wParam, new IntPtr(-1));
        }

        private IntPtr WmNcPaint(IntPtr hWnd, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_isNonClientStripVisible)
            {
                var hrgnClip = wParam == new IntPtr(1) ? IntPtr.Zero : wParam;
                var dcEx = User32.GetDCEx(hWnd, hrgnClip, 155);
                if (dcEx != IntPtr.Zero)
                {
                    try
                    {
                        var nonClientFillColor = NonClientFillColor;
                        var solidBrush =
                            Gdi32.CreateSolidBrush(nonClientFillColor.B << 16 | nonClientFillColor.G << 8 |
                                                   nonClientFillColor.R);
                        try
                        {
                            var relativeToWindowRect = GetClientRectRelativeToWindowRect(hWnd);
                            switch (_taskbarDirection)
                            {
                                case Dock.Left:
                                    relativeToWindowRect.Right = relativeToWindowRect.Left;
                                    relativeToWindowRect.Left = relativeToWindowRect.Right - 1;
                                    break;
                                case Dock.Top:
                                    relativeToWindowRect.Bottom = relativeToWindowRect.Top;
                                    relativeToWindowRect.Top = relativeToWindowRect.Bottom - 1;
                                    break;
                                case Dock.Right:
                                    relativeToWindowRect.Left = relativeToWindowRect.Right;
                                    relativeToWindowRect.Right = relativeToWindowRect.Left + 1;
                                    break;
                                case Dock.Bottom:
                                    relativeToWindowRect.Top = relativeToWindowRect.Bottom;
                                    relativeToWindowRect.Bottom = relativeToWindowRect.Top + 1;
                                    break;
                            }

                            User32.FillRect(dcEx, ref relativeToWindowRect, solidBrush);
                        }
                        finally
                        {
                            Gdi32.DeleteObject(solidBrush);
                        }
                    }
                    finally
                    {
                        User32.ReleaseDC(hWnd, dcEx);
                    }
                }
            }

            handled = true;
            return IntPtr.Zero;
        }

        private static RECT GetClientRectRelativeToWindowRect(
            IntPtr hWnd)
        {
            User32.GetWindowRect(hWnd, out var lpRect1);
            User32.GetClientRect(hWnd, out var lpRect2);
            var point = new POINT
            {
                X = 0,
                Y = 0
            };
            User32.ClientToScreen(hWnd, ref point);
            lpRect2.Offset(point.X - lpRect1.Left, point.Y - lpRect1.Top);
            return lpRect2;
        }

        private IntPtr WmNcCalcSize(IntPtr hWnd, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            _isNonClientStripVisible = false;
            if (NativeMethods.NativeMethods.GetWindowPlacement(hWnd).showCmd == 3)
            {
                var structure1 = (RECT) Marshal.PtrToStructure(lParam, typeof(RECT));
                User32.DefWindowProc(hWnd, 131, wParam, lParam);
                var structure2 = (RECT) Marshal.PtrToStructure(lParam, typeof(RECT));
                structure2.Top -= (int) Math.Ceiling(this.LogicalToDeviceUnitsY(SystemParameters.CaptionHeight) + 1.0);
                var monitorinfo = MonitorInfoFromWindow(hWnd);
                if (monitorinfo.RcMonitor.Height == monitorinfo.RcWork.Height &&
                    monitorinfo.RcMonitor.Width == monitorinfo.RcWork.Width)
                {
                    _isNonClientStripVisible = true;
                    var pData = new AppBarData {cbSize = Marshal.SizeOf(typeof(AppBarData))};
                    _taskbarDirection = !Convert.ToBoolean(Shell32.SHAppBarMessage(5U, ref pData))
                        ? ~Dock.Left
                        : (Dock) pData.uEdge;
                    switch (_taskbarDirection)
                    {
                        case Dock.Left:
                            ++structure2.Left;
                            break;
                        case Dock.Top:
                            ++structure2.Top;
                            break;
                        case Dock.Right:
                            --structure2.Right;
                            break;
                        case Dock.Bottom:
                            --structure2.Bottom;
                            break;
                    }
                }

                Marshal.StructureToPtr(structure2, lParam, true);
            }

            handled = true;
            return IntPtr.Zero;
        }

        private IntPtr WmNcHitTest(IntPtr hWnd, IntPtr lParam, ref bool handled)
        {
            if (PresentationSource.FromDependencyObject(this) == null)
                return new IntPtr(0);
            var point1 = new Point(NativeMethods.NativeMethods.GetXlParam(lParam.ToInt32()),
                NativeMethods.NativeMethods.GetYlParam(lParam.ToInt32()));
            var point2 = PointFromScreen(point1);
            DependencyObject visualHit = null;
            VisualUtilities.HitTestVisibleElements(this, target =>
            {
                visualHit = target.VisualHit;
                return HitTestResultBehavior.Stop;
            }, new PointHitTestParameters(point2));
            var num = 0;
            for (; visualHit != null; visualHit = visualHit.GetVisualOrLogicalParent())
            {
                if (visualHit is INonClientArea nonClientArea)
                {
                    num = nonClientArea.HitTest(point1);
                    if (num != 0)
                        break;
                }
            }

            if (num == 0)
                num = 1;
            handled = true;
            return new IntPtr(num);
        }

        private void WmSysCommand(IntPtr hWnd, IntPtr wParam, IntPtr lParam)
        {
            var scWparam = NativeMethods.NativeMethods.GetScWparam(wParam);
            if (scWparam == 61456)
                User32.RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero,
                    RedrawWindowFlags.Invalidate | RedrawWindowFlags.NoChildren | RedrawWindowFlags.UpdateNow |
                    RedrawWindowFlags.Frame);
            if ((scWparam == 61488 || scWparam == 61472 || (scWparam == 61456 || scWparam == 61440)) &&
                (WindowState == WindowState.Normal && !IsAeroSnappedToMonitor(hWnd)))
                _logicalSizeForRestore = new Rect(Left, Top, Width, Height);
            if (scWparam != 61728 || WindowState == WindowState.Minimized ||
                (_logicalSizeForRestore.Width <= 0.0 || _logicalSizeForRestore.Height <= 0.0))
                return;
            Left = _logicalSizeForRestore.Left;
            Top = _logicalSizeForRestore.Top;
            Width = _logicalSizeForRestore.Width;
            Height = _logicalSizeForRestore.Height;
            _useLogicalSizeForRestore = true;
        }

        private bool IsAeroSnappedToMonitor(IntPtr hWnd)
        {
            var monitorinfo = MonitorInfoFromWindow(hWnd);
            var rect = new Rect(Left, Top, Width, Height);
            var deviceRect = hWnd.LogicalToDeviceRect(rect);
            return monitorinfo.RcWork.Height == deviceRect.Height &&
                   monitorinfo.RcWork.Top == deviceRect.Top;
        }

        private void WmWindowPosChanging(IntPtr hwnd, IntPtr lParam)
        {
            var structure = (WindowPos) Marshal.PtrToStructure(lParam, typeof(WindowPos));
            if (((int) structure.flags & 2) != 0 || ((int) structure.flags & 1) != 0 ||
                (structure.cx <= 0 || structure.cy <= 0))
                return;
            var floatRect = new Rect(structure.x, structure.y, structure.cx, structure.cy);
            if (_useLogicalSizeForRestore)
            {
                floatRect = hwnd.LogicalToDeviceRect(_logicalSizeForRestore);
                _logicalSizeForRestore = Rect.Empty;
                _useLogicalSizeForRestore = false;
            }

            var rect = _isDragging ? floatRect : Screen.GetOnScreenPosition(floatRect, -1, false, false);
            structure.x = (int) rect.X;
            structure.y = (int) rect.Y;
            structure.cx = (int) rect.Width;
            structure.cy = (int) rect.Height;
            Marshal.StructureToPtr(structure, lParam, true);
        }

        private void WmWindowPosChanged(IntPtr hWnd, IntPtr lParam)
        {
            try
            {
                var structure = (WindowPos) Marshal.PtrToStructure(lParam, typeof(WindowPos));
                var windowPlacement = NativeMethods.NativeMethods.GetWindowPlacement(hWnd);
                var currentBounds = new RECT(structure.x, structure.y, structure.x + structure.cx,
                    structure.y + structure.cy);
                if (((int) structure.flags & 1) != 1)
                    UpdateClipRegion(hWnd, windowPlacement, ClipRegionChangeType.FromSize, currentBounds);
                else if (((int) structure.flags & 2) != 2)
                    UpdateClipRegion(hWnd, windowPlacement, ClipRegionChangeType.FromPosition, currentBounds);
                OnWindowPosChanged(hWnd, windowPlacement.showCmd, windowPlacement.rcNormalPosition.ToInt32Rect());
                UpdateGlowWindowPositions(((int) structure.flags & 64) == 0);
                UpdateZOrderOfThisAndOwner();
            }
            catch (Win32Exception)
            {
            }
        }

        private void UpdateZOrderOfThisAndOwner()
        {
            if (_updatingZOrder)
                return;
            try
            {
                _updatingZOrder = true;
                var handle = WindowHelper.Handle;
                foreach (var loadedGlowWindow in LoadedGlowWindows)
                {
                    if (User32.GetWindow(loadedGlowWindow.Handle, 3) != handle)
                        User32.SetWindowPos(loadedGlowWindow.Handle, handle, 0, 0, 0, 0, 19);
                    handle = loadedGlowWindow.Handle;
                }

                var owner = WindowHelper.Owner;
                if (!(owner != IntPtr.Zero))
                    return;
                UpdateZOrderOfOwner(owner);
            }
            finally
            {
                _updatingZOrder = false;
            }
        }

        private void UpdateZOrderOfOwner(IntPtr hwndOwner)
        {
            var lastOwnedWindow = IntPtr.Zero;
            User32.EnumThreadWindows(Kernel32.GetCurrentThreadId(), (hwnd, lParam) =>
            {
                if (User32.GetWindow(hwnd, 4) == hwndOwner)
                    lastOwnedWindow = hwnd;
                return true;
            }, IntPtr.Zero);
            if (lastOwnedWindow == IntPtr.Zero || !(User32.GetWindow(hwndOwner, 3) != lastOwnedWindow))
                return;
            if (IsGlowVisible && lastOwnedWindow == WindowHelper.Handle)
                lastOwnedWindow = LoadedGlowWindows.LastOrDefault().Handle;
            User32.SetWindowPos(hwndOwner, lastOwnedWindow, 0, 0, 0, 0, 19);
        }

        protected virtual void OnWindowPosChanged(IntPtr hWnd, int showCmd, Int32Rect rcNormalPosition)
        {
        }

        protected void UpdateClipRegion(
            ClipRegionChangeType regionChangeType = ClipRegionChangeType.FromPropertyChange)
        {
            var hwndSource = (HwndSource) PresentationSource.FromVisual(this);
            if (hwndSource == null)
                return;
            User32.GetWindowRect(hwndSource.Handle, out var lpRect);
            var windowPlacement = NativeMethods.NativeMethods.GetWindowPlacement(hwndSource.Handle);
            UpdateClipRegion(hwndSource.Handle, windowPlacement, regionChangeType, lpRect);
        }

        private void UpdateClipRegion(IntPtr hWnd, WindowPlacement placement, ClipRegionChangeType changeType,
            RECT currentBounds)
        {
            UpdateClipRegionCore(hWnd, placement.showCmd, changeType, currentBounds.ToInt32Rect());
            _lastWindowPlacement = placement.showCmd;
        }

        protected virtual bool UpdateClipRegionCore(
            IntPtr hWnd,
            int showCmd,
            ClipRegionChangeType changeType,
            Int32Rect currentBounds)
        {
            if (showCmd == 3)
            {
                UpdateMaximizedClipRegion(hWnd);
                return true;
            }

            if (changeType != ClipRegionChangeType.FromSize && changeType != ClipRegionChangeType.FromPropertyChange &&
                _lastWindowPlacement == showCmd)
                return false;
            SetRoundRect(hWnd, currentBounds.Width, currentBounds.Height);
            return true;
        }

        private Windowinfo GetWindowInfo(IntPtr hWnd)
        {
            var pwi = new Windowinfo();
            pwi.CbSize = Marshal.SizeOf(pwi);
            User32.GetWindowInfo(hWnd, ref pwi);
            return pwi;
        }

        private void UpdateMaximizedClipRegion(IntPtr hWnd)
        {
            var relativeToWindowRect = GetClientRectRelativeToWindowRect(hWnd);
            if (_isNonClientStripVisible)
            {
                switch (_taskbarDirection)
                {
                    case Dock.Left:
                        --relativeToWindowRect.Left;
                        break;
                    case Dock.Top:
                        --relativeToWindowRect.Top;
                        break;
                    case Dock.Right:
                        ++relativeToWindowRect.Right;
                        break;
                    case Dock.Bottom:
                        ++relativeToWindowRect.Bottom;
                        break;
                }
            }

            var rectRgnIndirect = Gdi32.CreateRectRgnIndirect(ref relativeToWindowRect);
            User32.SetWindowRgn(hWnd, rectRgnIndirect, User32.IsWindowVisible(hWnd));
        }

        private static Monitorinfo MonitorInfoFromWindow(
            IntPtr hWnd)
        {
            var hMonitor = User32.MonitorFromWindow(hWnd, 2);
            var monitorInfo = new Monitorinfo();
            monitorInfo.CbSize = (uint) Marshal.SizeOf(typeof(Monitorinfo));
            User32.GetMonitorInfo(hMonitor, ref monitorInfo);
            return monitorInfo;
        }

        protected void SetRoundRect(IntPtr hWnd, int width, int height)
        {
            var roundRectRegion = ComputeRoundRectRegion(hWnd, 0, 0, width, height);
            User32.SetWindowRgn(hWnd, roundRectRegion, User32.IsWindowVisible(hWnd));
        }

        private IntPtr ComputeRoundRectRegion(IntPtr hwnd, int left, int top, int width, int height)
        {
            var deviceUnits1 = hwnd.LogicalToDeviceUnits(0);
            var deviceUnits2 = hwnd.LogicalToDeviceUnits(0);
            return Gdi32.CreateRoundRectRgn(left, top, left + width + 1, top + height + 1, deviceUnits1, deviceUnits2);
        }
        
        public static void ShowWindowMenu(HwndSource source, Visual element, Point elementPoint, Size elementSize)
        {
            if (elementPoint.X < 0.0 || elementPoint.X > elementSize.Width ||
                (elementPoint.Y < 0.0 || elementPoint.Y > elementSize.Height))
                return;
            var screen = element.PointToScreen(elementPoint);
            ShowWindowMenu(source, screen, true);
        }

        protected static void ShowWindowMenu(HwndSource source, Point screenPoint, bool canMinimize)
        {
            var systemMetrics = User32.GetSystemMetrics(40);
            var systemMenu = User32.GetSystemMenu(source.Handle, false);
            var windowPlacement = NativeMethods.NativeMethods.GetWindowPlacement(source.Handle);
            using (new SuppressRedrawScope(source.Handle))
            {
                var num = canMinimize ? 0U : 1U;
                if (windowPlacement.showCmd == 1)
                {
                    User32.EnableMenuItem(systemMenu, 61728U, 1U);
                    User32.EnableMenuItem(systemMenu, 61456U, 0U);
                    User32.EnableMenuItem(systemMenu, 61440U, 0U);
                    User32.EnableMenuItem(systemMenu, 61488U, 0U);
                    User32.EnableMenuItem(systemMenu, 61472U, 0U | num);
                    User32.EnableMenuItem(systemMenu, 61536U, 0U);
                }
                else if (windowPlacement.showCmd == 3)
                {
                    User32.EnableMenuItem(systemMenu, 61728U, 0U);
                    User32.EnableMenuItem(systemMenu, 61456U, 1U);
                    User32.EnableMenuItem(systemMenu, 61440U, 1U);
                    User32.EnableMenuItem(systemMenu, 61488U, 1U);
                    User32.EnableMenuItem(systemMenu, 61472U, 0U | num);
                    User32.EnableMenuItem(systemMenu, 61536U, 0U);
                }
            }

            var fuFlags = (uint) (systemMetrics | 256 | 128 | 2);
            var num1 = User32.TrackPopupMenuEx(systemMenu, fuFlags, (int) screenPoint.X, (int) screenPoint.Y,
                source.Handle, IntPtr.Zero);
            if (num1 == 0)
                return;
            User32.PostMessage(source.Handle, 274, new IntPtr(num1), IntPtr.Zero);
        }

        protected override void OnClosed(EventArgs e)
        {
            StopTimer();
            DestroyGlowWindows();
            base.OnClosed(e);
        }

        private bool IsGlowVisible
        {
            get => _isGlowVisible;
            set
            {
                if (_isGlowVisible == value)
                    return;
                _isGlowVisible = value;
                for (var direction = 0; direction < _glowWindows.Length; ++direction)
                    GetOrCreateGlowWindow(direction).IsVisible = value;
            }
        }

        private GlowWindow GetOrCreateGlowWindow(int direction, bool isSubclass = true)
        {
            if (_glowWindows[direction] == null)
            {
                _glowWindows[direction] = new GlowWindow(this, (Dock) direction, isSubclass);
                _glowWindows[direction].ActiveGlowColor = ActiveGlowColor;
                _glowWindows[direction].InactiveGlowColor = InactiveGlowColor;
                _glowWindows[direction].IsActive = IsActive;
            }

            return _glowWindows[direction];
        }

        private IEnumerable<GlowWindow> LoadedGlowWindows
        {
            get { return _glowWindows.Where(w => w != null); }
        }

        private void DestroyGlowWindows()
        {
            for (var index = 0; index < _glowWindows.Length; ++index)
            {
                using (_glowWindows[index])
                    _glowWindows[index] = null;
            }
        }

        private void UpdateGlowWindowPositions(bool delayIfNecessary)
        {
            using (DeferGlowChanges())
            {
                UpdateGlowVisibility(delayIfNecessary);
                foreach (var loadedGlowWindow in LoadedGlowWindows)
                    loadedGlowWindow.UpdateWindowPos();
            }
        }

        private void UpdateGlowActiveState()
        {
            using (DeferGlowChanges())
            {
                foreach (var loadedGlowWindow in LoadedGlowWindows)
                    loadedGlowWindow.IsActive = IsActive;
            }
        }

        public void ChangeOwnerForActivate(IntPtr newOwner)
        {
            _ownerForActivate = newOwner;
        }

        public void ChangeOwner(IntPtr newOwner)
        {
            WindowHelper.Owner = newOwner;
            foreach (var loadedGlowWindow in LoadedGlowWindows)
                loadedGlowWindow.ChangeOwner(newOwner);
            UpdateZOrderOfThisAndOwner();
        }

        private void UpdateGlowVisibility(bool delayIfNecessary)
        {
            var shouldShowGlow = ShouldShowGlow;
            if (shouldShowGlow == IsGlowVisible)
                return;
            if (SystemParameters.MinimizeAnimation & shouldShowGlow & delayIfNecessary)
            {
                if (_makeGlowVisibleTimer != null)
                {
                    _makeGlowVisibleTimer.Stop();
                }
                else
                {
                    _makeGlowVisibleTimer = new DispatcherTimer();
                    _makeGlowVisibleTimer.Interval = TimeSpan.FromMilliseconds(200.0);
                    _makeGlowVisibleTimer.Tick += OnDelayedVisibilityTimerTick;
                }

                _makeGlowVisibleTimer.Start();
            }
            else
            {
                StopTimer();
                IsGlowVisible = shouldShowGlow;
            }
        }

        protected virtual bool ShouldShowGlow
        {
            get
            {
                var handle = WindowHelper.Handle;
                return User32.IsWindowVisible(handle) && !User32.IsIconic(handle) && !User32.IsZoomed(handle) &&
                       (uint) ResizeMode > 0U;
            }
        }

        private void StopTimer()
        {
            if (_makeGlowVisibleTimer == null)
                return;
            _makeGlowVisibleTimer.Stop();
            _makeGlowVisibleTimer.Tick -= OnDelayedVisibilityTimerTick;
            _makeGlowVisibleTimer = null;
        }

        private void OnDelayedVisibilityTimerTick(object sender, EventArgs e)
        {
            StopTimer();
            UpdateGlowWindowPositions(false);
        }

        private void UpdateGlowColors()
        {
            using (DeferGlowChanges())
            {
                foreach (var loadedGlowWindow in LoadedGlowWindows)
                {
                    loadedGlowWindow.ActiveGlowColor = ActiveGlowColor;
                    loadedGlowWindow.InactiveGlowColor = InactiveGlowColor;
                }
            }
        }

        private IDisposable DeferGlowChanges()
        {
            return new ChangeScope(this);
        }

        private void EndDeferGlowChanges()
        {
            foreach (var loadedGlowWindow in LoadedGlowWindows)
                loadedGlowWindow.CommitChanges();
        }

        protected enum ClipRegionChangeType
        {
            FromSize,
            FromPosition,
            FromPropertyChange,
            FromUndockSingleTab,
        }

        private class SuppressRedrawScope : IDisposable
        {
            private readonly IntPtr _hwnd;
            private readonly bool _suppressedRedraw;

            public SuppressRedrawScope(IntPtr hwnd)
            {
                _hwnd = hwnd;
                if ((User32.GetWindowLong(hwnd, GWL.Style) & 268435456) == 0)
                    return;
                SetRedraw(false);
                _suppressedRedraw = true;
            }

            public void Dispose()
            {
                if (!_suppressedRedraw)
                    return;
                SetRedraw(true);
                var flags = RedrawWindowFlags.Invalidate | RedrawWindowFlags.AllChildren | RedrawWindowFlags.Frame;
                User32.RedrawWindow(_hwnd, IntPtr.Zero, IntPtr.Zero, flags);
            }

            private void SetRedraw(bool state)
            {
                User32.SendMessage(_hwnd, 11, new IntPtr(Convert.ToInt32(state)));
            }
        }

        private class ChangeScope : DisposableObject
        {
            private readonly ShadowChromeWindow _window;

            public ChangeScope(ShadowChromeWindow window)
            {
                _window = window;
                ++_window._deferGlowChangesCount;
            }

            protected override void DisposeManagedResources()
            {
                --_window._deferGlowChangesCount;
                if (_window._deferGlowChangesCount != 0)
                    return;
                _window.EndDeferGlowChanges();
            }
        }

        private sealed class GlowBitmap : DisposableObject
        {
            private static readonly CachedBitmapInfo[] TransparencyMasks = new CachedBitmapInfo[16];
            private readonly IntPtr _pbits;
            private readonly BitmapInfo _bitmapInfo;

            public GlowBitmap(IntPtr hdcScreen, int width, int height)
            {
                _bitmapInfo.BiSize = Marshal.SizeOf(typeof(BitmapInfoHeader));
                _bitmapInfo.BiPlanes = 1;
                _bitmapInfo.BiBitCount = 32;
                _bitmapInfo.BiCompression = 0;
                _bitmapInfo.BiXPelsPerMeter = 0;
                _bitmapInfo.BiYPelsPerMeter = 0;
                _bitmapInfo.BiWidth = width;
                _bitmapInfo.BiHeight = -height;
                Handle = Gdi32.CreateDIBSection(hdcScreen, ref _bitmapInfo, 0U, out _pbits, IntPtr.Zero, 0U);
            }

            public IntPtr Handle { get; }

            public IntPtr DiBits => _pbits;

            public int Width => _bitmapInfo.BiWidth;

            public int Height => -_bitmapInfo.BiHeight;

            protected override void DisposeNativeResources()
            {
                Gdi32.DeleteObject(Handle);
            }

            private static byte PremultiplyAlpha(byte channel, byte alpha)
            {
                return (byte) (channel * alpha / (double) byte.MaxValue);
            }

            public static GlowBitmap Create(GlowDrawingContext drawingContext, GlowBitmapPart bitmapPart, Color color)
            {
                var alphaMask = GetOrCreateAlphaMask(bitmapPart);
                var glowBitmap = new GlowBitmap(drawingContext.ScreenDc, alphaMask.Width, alphaMask.Height);
                for (var ofs = 0; ofs < alphaMask.DiBits.Length; ofs += 4)
                {
                    var diBit = alphaMask.DiBits[ofs + 3];
                    var val1 = PremultiplyAlpha(color.R, diBit);
                    var val2 = PremultiplyAlpha(color.G, diBit);
                    var val3 = PremultiplyAlpha(color.B, diBit);
                    Marshal.WriteByte(glowBitmap.DiBits, ofs, val3);
                    Marshal.WriteByte(glowBitmap.DiBits, ofs + 1, val2);
                    Marshal.WriteByte(glowBitmap.DiBits, ofs + 2, val1);
                    Marshal.WriteByte(glowBitmap.DiBits, ofs + 3, diBit);
                }

                return glowBitmap;
            }

            private static CachedBitmapInfo GetOrCreateAlphaMask(
                GlowBitmapPart bitmapPart)
            {
                var index = (int) bitmapPart;
                if (TransparencyMasks[index] == null)
                {
                    var bitmapImage = new BitmapImage(MakePackUri(typeof(GlowBitmap).Assembly,
                        "Resources/ShadowImages/" + bitmapPart + ".png"));
                    var diBits = new byte[4 * bitmapImage.PixelWidth * bitmapImage.PixelHeight];
                    var stride = 4 * bitmapImage.PixelWidth;
                    bitmapImage.CopyPixels(diBits, stride, 0);
                    TransparencyMasks[index] =
                        new CachedBitmapInfo(diBits, bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                }

                return TransparencyMasks[index];
            }

            private static Uri MakePackUri(Assembly assembly, string path)
            {
                return new Uri($"pack://application:,,,/{assembly.GetName().Name};component/{path}", UriKind.Absolute);
            }

            private sealed class CachedBitmapInfo
            {
                public readonly int Width;
                public readonly int Height;
                public readonly byte[] DiBits;

                public CachedBitmapInfo(byte[] diBits, int width, int height)
                {
                    Width = width;
                    Height = height;
                    DiBits = diBits;
                }
            }
        }

        private enum GlowBitmapPart
        {
            CornerTopLeft,
            CornerTopRight,
            CornerBottomLeft,
            CornerBottomRight,
            TopLeft,
            Top,
            TopRight,
            LeftTop,
            Left,
            LeftBottom,
            BottomLeft,
            Bottom,
            BottomRight,
            RightTop,
            Right,
            RightBottom,
        }

        private sealed class GlowDrawingContext : DisposableObject
        {
            public BlendFunction Blend;
            private readonly GlowBitmap _windowBitmap;

            public GlowDrawingContext(int width, int height)
            {
                ScreenDc = User32.GetDC(IntPtr.Zero);
                if (ScreenDc == IntPtr.Zero)
                    return;
                WindowDc = Gdi32.CreateCompatibleDC(ScreenDc);
                if (WindowDc == IntPtr.Zero)
                    return;
                BackgroundDc = Gdi32.CreateCompatibleDC(ScreenDc);
                if (BackgroundDc == IntPtr.Zero)
                    return;
                Blend.BlendOp = 0;
                Blend.BlendFlags = 0;
                Blend.SourceConstantAlpha = byte.MaxValue;
                Blend.AlphaFormat = 1;
                _windowBitmap = new GlowBitmap(ScreenDc, width, height);
                Gdi32.SelectObject(WindowDc, _windowBitmap.Handle);
            }

            public bool IsInitialized => ScreenDc != IntPtr.Zero && WindowDc != IntPtr.Zero &&
                                         BackgroundDc != IntPtr.Zero && _windowBitmap != null;

            public IntPtr ScreenDc { get; }

            public IntPtr WindowDc { get; }

            public IntPtr BackgroundDc { get; }

            public int Width
            {
                get
                {
                    var windowBitmap = _windowBitmap;
                    return windowBitmap?.Width ?? 0;
                }
            }

            public int Height
            {
                get
                {
                    var windowBitmap = _windowBitmap;
                    return windowBitmap?.Height ?? 0;
                }
            }

            protected override void DisposeManagedResources()
            {
                _windowBitmap?.Dispose();
            }

            protected override void DisposeNativeResources()
            {
                if (ScreenDc != IntPtr.Zero)
                    User32.ReleaseDC(IntPtr.Zero, ScreenDc);
                if (WindowDc != IntPtr.Zero)
                    Gdi32.DeleteDC(WindowDc);
                if (!(BackgroundDc != IntPtr.Zero))
                    return;
                Gdi32.DeleteDC(BackgroundDc);
            }
        }

        private sealed class GlowWindow : HwndWrapper
        {
            private readonly GlowBitmap[] _activeGlowBitmaps = new GlowBitmap[16];
            private readonly GlowBitmap[] _inactiveGlowBitmaps = new GlowBitmap[16];
            private Color _activeGlowColor = Colors.Transparent;
            private Color _inactiveGlowColor = Colors.Transparent;
            private readonly ShadowChromeWindow _targetWindow;
            private readonly Dock _orientation;
            private static ushort _sharedWindowClassAtom;
            private static WndProc _sharedWndProc;
            private static long _createdGlowWindows;
            private int _left;
            private int _top;
            private int _width;
            private int _height;
            private bool _isVisible;
            private bool _isActive;
            private FieldInvalidationTypes _invalidatedValues;
            private bool _pendingDelayRender;

            public GlowWindow(ShadowChromeWindow owner, Dock orientation, bool isWindowSubclass)
            {
                _targetWindow = owner;
                _orientation = orientation;
                ++_createdGlowWindows;
                IsWindowSubclassed = isWindowSubclass;
            }

            private bool IsDeferringChanges => _targetWindow._deferGlowChangesCount > 0;

            private static ushort SharedWindowClassAtom
            {
                get
                {
                    if (_sharedWindowClassAtom == 0)
                    {
                        var lpWndClass = new WndClass
                        {
                            cbClsExtra = 0,
                            cbWndExtra = 0,
                            hbrBackground = IntPtr.Zero,
                            hCursor = IntPtr.Zero,
                            hIcon = IntPtr.Zero,
                            lpfnWndProc = _sharedWndProc = User32.DefWindowProc,
                            lpszClassName = "MafGlowWindow",
                            lpszMenuName = null,
                            style = 0
                        };
                        _sharedWindowClassAtom =
                            User32.RegisterClass(ref lpWndClass);
                    }

                    return _sharedWindowClassAtom;
                }
            }

            protected override bool IsWindowSubclassed { get; }

            private void UpdateProperty<T>(
                ref T field,
                T value,
                FieldInvalidationTypes invalidatedValues)
                where T : struct
            {
                if (field.Equals(value))
                    return;
                field = value;
                _invalidatedValues |= invalidatedValues;
                if (IsDeferringChanges)
                    return;
                CommitChanges();
            }

            public bool IsVisible
            {
                get => _isVisible;
                set => UpdateProperty(ref _isVisible, value,
                    FieldInvalidationTypes.Render | FieldInvalidationTypes.Visibility);
            }

            public int Left
            {
                get => _left;
                set => UpdateProperty(ref _left, value, FieldInvalidationTypes.Location);
            }

            public int Top
            {
                get => _top;
                set => UpdateProperty(ref _top, value, FieldInvalidationTypes.Location);
            }

            public int Width
            {
                get => _width;
                set => UpdateProperty(ref _width, value,
                    FieldInvalidationTypes.Size | FieldInvalidationTypes.Render);
            }

            public int Height
            {
                get => _height;
                set => UpdateProperty(ref _height, value,
                    FieldInvalidationTypes.Size | FieldInvalidationTypes.Render);
            }

            public bool IsActive
            {
                get => _isActive;
                set => UpdateProperty(ref _isActive, value, FieldInvalidationTypes.Render);
            }

            public Color ActiveGlowColor
            {
                get => _activeGlowColor;
                set => UpdateProperty(ref _activeGlowColor, value,
                    FieldInvalidationTypes.ActiveColor | FieldInvalidationTypes.Render);
            }

            public Color InactiveGlowColor
            {
                get => _inactiveGlowColor;
                set => UpdateProperty(ref _inactiveGlowColor, value,
                    FieldInvalidationTypes.InactiveColor | FieldInvalidationTypes.Render);
            }

            private IntPtr TargetWindowHandle => new WindowInteropHelper(_targetWindow).Handle;

            protected override ushort CreateWindowClassCore()
            {
                return SharedWindowClassAtom;
            }

            protected override void DestroyWindowClassCore()
            {
            }

            protected override IntPtr CreateWindowCore()
            {
                return User32.CreateWindowEx(524416, new IntPtr(WindowClassAtom), string.Empty, -2046820352, 0, 0, 0, 0,
                    new WindowInteropHelper(_targetWindow).Owner, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }

            public void ChangeOwner(IntPtr newOwner)
            {
                User32.SetWindowLongPtr(Handle, Gwlp.Hwndparent, newOwner);
            }
            
            protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
            {
                switch (msg)
                {
                    case 6:
                        return IntPtr.Zero;
                    case 70:
                        var structure = (WindowPos) Marshal.PtrToStructure(lParam, typeof(WindowPos));
                        structure.flags |= 16U;
                        Marshal.StructureToPtr(structure, lParam, true);
                        break;
                    case 126:
                        if (IsVisible) 
                            RenderLayeredWindow();
                        break;
                    case 132:
                        return new IntPtr(WmNcHitTest(lParam));
                    case 161:
                    case 163:
                    case 164:
                    case 166:
                    case 167:
                    case 169:
                    case 171:
                    case 173:
                        var targetWindowHandle = TargetWindowHandle;
                        User32.SendMessage(targetWindowHandle, 6, new IntPtr(2), IntPtr.Zero);
                        User32.SendMessage(targetWindowHandle, msg, wParam, IntPtr.Zero);
                        return IntPtr.Zero;
                }

                return base.WndProc(hwnd, msg, wParam, lParam);
            }

            private int WmNcHitTest(IntPtr lParam)
            {
                var xlParam = NativeMethods.NativeMethods.GetXlParam(lParam.ToInt32());
                var ylParam = NativeMethods.NativeMethods.GetYlParam(lParam.ToInt32());
                User32.GetWindowRect(Handle, out var lpRect);
                switch (_orientation)
                {
                    case Dock.Left:
                        if (ylParam - 18 < lpRect.Top)
                            return 13;
                        return ylParam + 18 > lpRect.Bottom ? 16 : 10;
                    case Dock.Top:
                        if (xlParam - 18 < lpRect.Left)
                            return 13;
                        return xlParam + 18 > lpRect.Right ? 14 : 12;
                    case Dock.Right:
                        if (ylParam - 18 < lpRect.Top)
                            return 14;
                        return ylParam + 18 > lpRect.Bottom ? 17 : 11;
                    default:
                        if (xlParam - 18 < lpRect.Left)
                            return 16;
                        return xlParam + 18 > lpRect.Right ? 17 : 15;
                }
            }

            public void CommitChanges()
            {
                InvalidateCachedBitmaps();
                UpdateWindowPosCore();
                UpdateLayeredWindowCore();
                _invalidatedValues = FieldInvalidationTypes.None;
            }

            private bool InvalidatedValuesHasFlag(
                FieldInvalidationTypes flag)
            {
                return (uint) (_invalidatedValues & flag) > 0U;
            }

            private void InvalidateCachedBitmaps()
            {
                if (InvalidatedValuesHasFlag(FieldInvalidationTypes.ActiveColor))
                    ClearCache(_activeGlowBitmaps);
                if (!InvalidatedValuesHasFlag(FieldInvalidationTypes.InactiveColor))
                    return;
                ClearCache(_inactiveGlowBitmaps);
            }

            private void UpdateWindowPosCore()
            {
                if (!InvalidatedValuesHasFlag(FieldInvalidationTypes.Location | FieldInvalidationTypes.Size |
                                              FieldInvalidationTypes.Visibility))
                    return;
                var flags = 532;
                if (InvalidatedValuesHasFlag(FieldInvalidationTypes.Visibility))
                {
                    if (IsVisible)
                        flags |= 64;
                    else
                        flags |= 131;
                }

                if (!InvalidatedValuesHasFlag(FieldInvalidationTypes.Location))
                    flags |= 2;
                if (!InvalidatedValuesHasFlag(FieldInvalidationTypes.Size))
                    flags |= 1;
                User32.SetWindowPos(Handle, IntPtr.Zero, Left, Top, Width, Height, flags);
            }

            private void UpdateLayeredWindowCore()
            {
                if (!IsVisible || !InvalidatedValuesHasFlag(FieldInvalidationTypes.Render))
                    return;
                if (IsPositionValid)
                {
                    BeginDelayedRender();
                }
                else
                {
                    CancelDelayedRender();
                    RenderLayeredWindow();
                }
            }

            private bool IsPositionValid => !InvalidatedValuesHasFlag(
                FieldInvalidationTypes.Location | FieldInvalidationTypes.Size | FieldInvalidationTypes.Visibility);

            private void BeginDelayedRender()
            {
                if (_pendingDelayRender)
                    return;
                _pendingDelayRender = true;
                CompositionTarget.Rendering += CommitDelayedRender;
            }

            private void CancelDelayedRender()
            {
                if (!_pendingDelayRender)
                    return;
                _pendingDelayRender = false;
                CompositionTarget.Rendering -= CommitDelayedRender;
            }

            private void CommitDelayedRender(object sender, EventArgs e)
            {
                CancelDelayedRender();
                if (!IsVisible)
                    return;
                RenderLayeredWindow();
            }

            private void RenderLayeredWindow()
            {
                using var drawingContext = new GlowDrawingContext(Width, Height);
                if (!drawingContext.IsInitialized)
                    return;
                switch (_orientation)
                {
                    case Dock.Left:
                        DrawLeft(drawingContext);
                        break;
                    case Dock.Top:
                        DrawTop(drawingContext);
                        break;
                    case Dock.Right:
                        DrawRight(drawingContext);
                        break;
                    default:
                        DrawBottom(drawingContext);
                        break;
                }

                var pptDest = new POINT
                {
                    X = Left,
                    Y = Top
                };
                var psize = new Win32Size
                {
                    Cx = Width,
                    Cy = Height
                };
                var pptSrc = new POINT
                {
                    X = 0,
                    Y = 0
                };
                User32.UpdateLayeredWindow(Handle, drawingContext.ScreenDc, ref pptDest, ref psize,
                    drawingContext.WindowDc, ref pptSrc, 0U, ref drawingContext.Blend, 2U);
            }

            private GlowBitmap GetOrCreateBitmap(
                GlowDrawingContext drawingContext,
                GlowBitmapPart bitmapPart)
            {
                GlowBitmap[] glowBitmapArray;
                Color color;
                if (IsActive)
                {
                    glowBitmapArray = _activeGlowBitmaps;
                    color = ActiveGlowColor;
                }
                else
                {
                    glowBitmapArray = _inactiveGlowBitmaps;
                    color = InactiveGlowColor;
                }

                var index = (int) bitmapPart;
                return glowBitmapArray[index] ??
                       (glowBitmapArray[index] = GlowBitmap.Create(drawingContext, bitmapPart, color));
            }

            private static void ClearCache(IList<GlowBitmap> cache)
            {
                for (var index = 0; index < cache.Count; ++index)
                {
                    using (cache[index])
                        cache[index] = null;
                }
            }

            protected override void DisposeManagedResources()
            {
                ClearCache(_activeGlowBitmaps);
                ClearCache(_inactiveGlowBitmaps);
            }
            
            private void DrawLeft(
                GlowDrawingContext drawingContext)
            {
                var bitmap1 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.CornerTopLeft);
                var bitmap2 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.LeftTop);
                var bitmap3 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.Left);
                var bitmap4 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.LeftBottom);
                var bitmap5 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.CornerBottomLeft);
                var height = bitmap1.Height;
                var yoriginDest1 = height + bitmap2.Height;
                var yoriginDest2 = drawingContext.Height - bitmap5.Height;
                var yoriginDest3 = yoriginDest2 - bitmap4.Height;
                var hDest = yoriginDest3 - yoriginDest1;
                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap1.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, 0, 0, bitmap1.Width, bitmap1.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap1.Width, bitmap1.Height, drawingContext.Blend);
                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap2.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, 0, height, bitmap2.Width, bitmap2.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap2.Width, bitmap2.Height, drawingContext.Blend);
                if (hDest > 0)
                {
                    Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap3.Handle);
                    Msimg32.AlphaBlend(drawingContext.WindowDc, 0, yoriginDest1, bitmap3.Width, hDest,
                        drawingContext.BackgroundDc, 0, 0, bitmap3.Width, bitmap3.Height, drawingContext.Blend);
                }

                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap4.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, 0, yoriginDest3, bitmap4.Width, bitmap4.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap4.Width, bitmap4.Height, drawingContext.Blend);
                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap5.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, 0, yoriginDest2, bitmap5.Width, bitmap5.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap5.Width, bitmap5.Height, drawingContext.Blend);
            }

            private void DrawRight(
                GlowDrawingContext drawingContext)
            {
                var bitmap1 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.CornerTopRight);
                var bitmap2 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.RightTop);
                var bitmap3 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.Right);
                var bitmap4 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.RightBottom);
                var bitmap5 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.CornerBottomRight);
                var height = bitmap1.Height;
                var yoriginDest1 = height + bitmap2.Height;
                var yoriginDest2 = drawingContext.Height - bitmap5.Height;
                var yoriginDest3 = yoriginDest2 - bitmap4.Height;
                var hDest = yoriginDest3 - yoriginDest1;
                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap1.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, 0, 0, bitmap1.Width, bitmap1.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap1.Width, bitmap1.Height, drawingContext.Blend);
                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap2.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, 0, height, bitmap2.Width, bitmap2.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap2.Width, bitmap2.Height, drawingContext.Blend);
                if (hDest > 0)
                {
                    Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap3.Handle);
                    Msimg32.AlphaBlend(drawingContext.WindowDc, 0, yoriginDest1, bitmap3.Width, hDest,
                        drawingContext.BackgroundDc, 0, 0, bitmap3.Width, bitmap3.Height, drawingContext.Blend);
                }

                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap4.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, 0, yoriginDest3, bitmap4.Width, bitmap4.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap4.Width, bitmap4.Height, drawingContext.Blend);
                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap5.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, 0, yoriginDest2, bitmap5.Width, bitmap5.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap5.Width, bitmap5.Height, drawingContext.Blend);
            }

            private void DrawTop(
                GlowDrawingContext drawingContext)
            {
                var bitmap1 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.TopLeft);
                var bitmap2 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.Top);
                var bitmap3 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.TopRight);
                var xoriginDest1 = 9;
                var xoriginDest2 = xoriginDest1 + bitmap1.Width;
                var xoriginDest3 = drawingContext.Width - 9 - bitmap3.Width;
                var wDest = xoriginDest3 - xoriginDest2;
                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap1.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, xoriginDest1, 0, bitmap1.Width, bitmap1.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap1.Width, bitmap1.Height, drawingContext.Blend);
                if (wDest > 0)
                {
                    Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap2.Handle);
                    Msimg32.AlphaBlend(drawingContext.WindowDc, xoriginDest2, 0, wDest, bitmap2.Height,
                        drawingContext.BackgroundDc, 0, 0, bitmap2.Width, bitmap2.Height, drawingContext.Blend);
                }

                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap3.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, xoriginDest3, 0, bitmap3.Width, bitmap3.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap3.Width, bitmap3.Height, drawingContext.Blend);
            }

            private void DrawBottom(
                GlowDrawingContext drawingContext)
            {
                var bitmap1 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.BottomLeft);
                var bitmap2 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.Bottom);
                var bitmap3 = GetOrCreateBitmap(drawingContext, GlowBitmapPart.BottomRight);
                var xoriginDest1 = 9;
                var xoriginDest2 = xoriginDest1 + bitmap1.Width;
                var xoriginDest3 = drawingContext.Width - 9 - bitmap3.Width;
                var wDest = xoriginDest3 - xoriginDest2;
                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap1.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, xoriginDest1, 0, bitmap1.Width, bitmap1.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap1.Width, bitmap1.Height, drawingContext.Blend);
                if (wDest > 0)
                {
                    Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap2.Handle);
                    Msimg32.AlphaBlend(drawingContext.WindowDc, xoriginDest2, 0, wDest, bitmap2.Height,
                        drawingContext.BackgroundDc, 0, 0, bitmap2.Width, bitmap2.Height, drawingContext.Blend);
                }

                Gdi32.SelectObject(drawingContext.BackgroundDc, bitmap3.Handle);
                Msimg32.AlphaBlend(drawingContext.WindowDc, xoriginDest3, 0, bitmap3.Width, bitmap3.Height,
                    drawingContext.BackgroundDc, 0, 0, bitmap3.Width, bitmap3.Height, drawingContext.Blend);
            }

            public void UpdateWindowPos()
            {
                var targetWindowHandle = TargetWindowHandle;
                User32.GetWindowRect(targetWindowHandle, out var lpRect);
                NativeMethods.NativeMethods.GetWindowPlacement(targetWindowHandle);
                if (!IsVisible)
                    return;
                switch (_orientation)
                {
                    case Dock.Left:
                        Left = lpRect.Left - 9;
                        Top = lpRect.Top - 9;
                        Width = 9;
                        Height = lpRect.Height + 18;
                        break;
                    case Dock.Top:
                        Left = lpRect.Left - 9;
                        Top = lpRect.Top - 9;
                        Width = lpRect.Width + 18;
                        Height = 9;
                        break;
                    case Dock.Right:
                        Left = lpRect.Right;
                        Top = lpRect.Top - 9;
                        Width = 9;
                        Height = lpRect.Height + 18;
                        break;
                    default:
                        Left = lpRect.Left - 9;
                        Top = lpRect.Bottom;
                        Width = lpRect.Width + 18;
                        Height = 9;
                        break;
                }
            }

            [Flags]
            private enum FieldInvalidationTypes
            {
                None = 0,
                Location = 1,
                Size = 2,
                ActiveColor = 4,
                InactiveColor = 8,
                Render = 16, // 0x00000010
                Visibility = 32, // 0x00000020
            }
        }
    }
}
