namespace FocLauncher.NativeMethods
{
    internal struct Windowinfo
    {
        public int CbSize;
        public RECT RcWindow;
        public RECT RcClient;
        public int DwStyle;
        public int DwExStyle;
        public uint DwWindowStatus;
        public uint CxWindowBorders;
        public uint CyWindowBorders;
        public ushort AtomWindowType;
        public ushort WCreatorVersion;
    }
}