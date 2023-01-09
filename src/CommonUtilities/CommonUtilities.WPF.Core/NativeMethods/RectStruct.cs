using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

internal struct RectStruct
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public RectStruct(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public RectStruct(Rect rect)
    {
        Left = (int)rect.Left;
        Top = (int)rect.Top;
        Right = (int)rect.Right;
        Bottom = (int)rect.Bottom;
    }

    public void Offset(int dx, int dy)
    {
        Left += dx;
        Right += dx;
        Top += dy;
        Bottom += dy;
    }

    public Point Position => new(Left, Top);

    public Size Size => new(Width, Height);

    public int Height
    {
        get => Bottom - Top;
        set => Bottom = Top + value;
    }

    public int Width
    {
        get => Right - Left;
        set => Right = Left + value;
    }

    public Int32Rect ToInt32Rect() => new(Left, Top, Width, Height);

    public Rect ToRect() => new(Left, Top, Width, Height);

    public static RectStruct FromInt32Rect(Int32Rect rect) => new(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
}