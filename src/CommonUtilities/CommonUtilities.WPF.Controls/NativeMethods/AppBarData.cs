using System;

namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

internal struct AppBarData
{
    public int CbSize;
    public IntPtr Hwnd;
    public uint UCallbackMessage;
    public uint UEdge;
    public RectStruct Rc;
    public IntPtr LParam;
}