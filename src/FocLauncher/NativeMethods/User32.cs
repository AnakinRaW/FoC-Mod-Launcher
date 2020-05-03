using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal static class User32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetMessagePos();
    }
}