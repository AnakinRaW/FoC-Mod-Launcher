using System;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();
    }
}
