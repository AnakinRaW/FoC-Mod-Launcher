using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal class NativeMethods
    {
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
    }
}
