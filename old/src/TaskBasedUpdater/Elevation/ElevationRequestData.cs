using System;
using TaskBasedUpdater.Component;

namespace TaskBasedUpdater.Elevation
{
    public class ElevationRequestData : IEquatable<ElevationRequestData>
    {
        public IComponent Component { get; }

        public Exception Exception { get; }

        public ElevationRequestData(Exception exception, IComponent component)
        {
            Exception = exception;
            Component = component;
        }

        public bool Equals(ElevationRequestData? other)
        {
            if (other is null)
                return false;
            return ReferenceEquals(this, other) || Component.Equals(other.Component);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) 
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;
            return obj.GetType() == GetType() && Equals((ElevationRequestData) obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Component);
        }
    }
}