using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal struct Monitorinfo
    {
        public uint CbSize;
        public RECT RcMonitor;
        public RECT RcWork;
        public uint DwFlags;

        public bool IsPrimary => (DwFlags & 1U) > 0U;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class MonitorInfo
    {
        /// <summary>
        /// </summary>            
        public int cbSize = Marshal.SizeOf(typeof(MonitorInfo));

        /// <summary>
        /// </summary>            
        public RECT rcMonitor = new RECT();

        /// <summary>
        /// </summary>            
        public RECT rcWork = new RECT();

        /// <summary>
        /// </summary>            
        public int dwFlags = 0;
    }
}