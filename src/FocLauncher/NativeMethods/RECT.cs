using System;
using System.Windows;

namespace FocLauncher.NativeMethods
{
    [Serializable]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public RECT(Rect rect)
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

        public Point Position => new Point(Left, Top);

        public Size Size => new Size(Width, Height);

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

        public Int32Rect ToInt32Rect()
        {
            return new Int32Rect(Left, Top, Width, Height);
        }

        public Rect ToRect()
        {
            return new Rect(Left, Top, Width, Height);
        }

        public static RECT FromInt32Rect(Int32Rect rect)
        {
            return new RECT(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        }
    }
}