using System;

namespace FocLauncher.ScreenUtilities
{
    public struct Dpi : IEquatable<Dpi>
    {
        public static readonly Dpi Default = new Dpi(96, 96);

        public double X { get; }

        public double Y { get; }

        public bool IsValid => !double.IsInfinity(X) && !double.IsInfinity(Y) && !double.IsNaN(X) && !double.IsNaN(Y) && !(X <= 0.0) && !(Y <= 0.0);

        public Dpi(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"X:{X}, Y:{Y}";
        }

        public bool Equals(Dpi other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            return obj is Dpi other && Equals(other);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}