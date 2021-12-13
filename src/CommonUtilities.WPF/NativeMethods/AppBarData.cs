using System;

namespace Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

internal struct AppBarData
{
    public int cbSize;
    public IntPtr hwnd;
    public uint uCallbackMessage;
    public uint uEdge;
    public RectStruct rc;
    public IntPtr lParam;
}