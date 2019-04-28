using System;
using System.Runtime.InteropServices;

namespace FocLauncherApp.NativeMethods
{
    internal static class NativeMethods
    {
        [DllImport("wininet.dll", SetLastError = true)]
        internal static extern bool InternetGetConnectedState(out ConnectionStates lpdwFlags, int dwReserved);

        [Flags]
        internal enum ConnectionStates
        {
            Modem = 0x1,
            LAN = 0x2,
            Proxy = 0x4,
            RasInstalled = 0x10,
            Offline = 0x20,
            Configured = 0x40,
        }
    }
}
