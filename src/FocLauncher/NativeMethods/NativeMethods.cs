using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace FocLauncher.NativeMethods
{
    internal class NativeMethods
    {
        private static int _vsmNotifyOwnerActivate;

        public static int NotifyOwnerActive
        {
            get
            {
                if (_vsmNotifyOwnerActivate == 0)
                    _vsmNotifyOwnerActivate = User32.RegisterWindowMessage("NOTIFYOWNERACTIVATE{A982313C-756C-4da9-8BD0-0C375A45784B}");
                return _vsmNotifyOwnerActivate;
            }
        }

        internal static int GetXlParam(int lParam)
        {
            return LoWord(lParam);
        }

        internal static int GetYlParam(int lParam)
        {
            return HiWord(lParam);
        }

        internal static int HiWord(int value)
        {
            return (short)(value >> 16);
        }

        internal static int LoWord(int value)
        {
            return (short)(value & ushort.MaxValue);
        }

        public static int SignedHigh(int n)
        {
            return (short)(n >> 16 & ushort.MaxValue);
        }

        public static int SignedLow(int n)
        {
            return (short)(n & ushort.MaxValue);
        }

        internal static IntPtr MakeParam(int lowWord, int highWord)
        {
            return new IntPtr((lowWord & ushort.MaxValue) | (highWord << 16));
        }

        internal static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 4
                ? User32.SetWindowLongPtr32(hWnd, nIndex, dwNewLong)
                : User32.SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        internal static bool IsKeyPressed(int vKey)
        {
            return User32.GetKeyState(vKey) < 0;
        }

        internal static int GetScWparam(IntPtr wParam)
        {
            return (int) wParam & 65520;
        }

        internal static Monitorinfo MonitorInfoFromWindow(Window window)
        {
            var interop = new WindowInteropHelper(window);
            interop.EnsureHandle();
            var handle = interop.Handle;
            var hMonitor = User32.MonitorFromWindow(handle, 2);
            var monitorInfo = new Monitorinfo { CbSize = (uint)Marshal.SizeOf(typeof(Monitorinfo)) };
            User32.GetMonitorInfo(hMonitor, ref monitorInfo);
            return monitorInfo;
        }

        internal static WindowPlacement GetWindowPlacement(IntPtr hwnd)
        {
            var lpwndpl = new WindowPlacement();
            if (User32.GetWindowPlacement(hwnd, lpwndpl))
                return lpwndpl;
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        internal static Monitorinfo MonitorInfoFromWindow(IntPtr hWnd)
        {
            var hMonitor = User32.MonitorFromWindow(hWnd, 2);
            var monitorInfo = new Monitorinfo { CbSize = (uint)Marshal.SizeOf(typeof(Monitorinfo)) };
            User32.GetMonitorInfo(hMonitor, ref monitorInfo);
            return monitorInfo;
        }

        internal static int CombineRgn(IntPtr hrnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, CombineMode combineMode)
        {
            return Gdi32.CombineRgn(hrnDest, hrgnSrc1, hrgnSrc2, (int)combineMode);
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        public class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        public interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
    }
}
