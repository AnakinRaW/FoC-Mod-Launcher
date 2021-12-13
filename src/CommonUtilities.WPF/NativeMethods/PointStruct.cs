using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

internal struct PointStruct
{
    public int x;
    public int y;

    public static PointStruct FromPoint(Point pt) => new PointStruct()
    {
        x = (int)pt.X,
        y = (int)pt.Y
    };
}