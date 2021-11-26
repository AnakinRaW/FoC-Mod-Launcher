namespace FocLauncher.NativeMethods
{
    internal struct POINT
    {
        public int X;
        public int Y;

        public static POINT FromPoint(System.Windows.Point pt)
        {
            return new POINT { X = (int)pt.X, Y = (int)pt.Y };
        }
    }
}