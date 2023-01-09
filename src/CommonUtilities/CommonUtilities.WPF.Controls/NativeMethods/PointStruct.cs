using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

internal struct PointStruct
{
    public int X;
    public int Y;

    public static PointStruct FromPoint(Point pt) => new()
    {
        X = (int)pt.X,
        Y = (int)pt.Y
    };
}