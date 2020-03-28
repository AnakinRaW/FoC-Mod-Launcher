using System;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal static class WiniNet
    {
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetConnectedState(out ConnectionStates lpdwFlags, int dwReserved);

        internal static bool InternetGetConnectedState(out ConnectionStates lpdwFlags)
        {
            return InternetGetConnectedState(out lpdwFlags, 0);
        }

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
