using System.Runtime.InteropServices;

namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

internal static class Kernel32
{
    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();
}