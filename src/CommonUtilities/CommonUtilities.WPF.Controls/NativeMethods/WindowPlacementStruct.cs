using System.Runtime.InteropServices;

namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

[StructLayout(LayoutKind.Sequential)]
internal class WindowPlacementStruct
{
    public int length = Marshal.SizeOf<WindowPlacementStruct>();
    public uint flags;
    public int showCmd;
    public PointStruct ptMinPosition;
    public PointStruct ptMaxPosition;
    public RectStruct rcNormalPosition;
}