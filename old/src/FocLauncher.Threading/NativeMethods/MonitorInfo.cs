namespace FocLauncher.NativeMethods
{
    internal struct MonitorInfo
    {
        public uint CbSize;
        public RECT RcMonitor;
        public RECT RcWork;
        public uint DwFlags;

        public bool IsPrimary => (DwFlags & 1U) > 0U;
    }
}