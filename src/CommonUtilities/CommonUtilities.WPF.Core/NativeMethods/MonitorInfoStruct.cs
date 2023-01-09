namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

internal struct MonitorInfoStruct
{
    public uint CbSize;
    public RectStruct RcMonitor;
    public RectStruct RcWork;
    public uint DwFlags;

    public bool IsPrimary => (DwFlags & 1) > 0;
}