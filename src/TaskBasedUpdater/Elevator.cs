using System;
using System.Collections.Generic;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Restart;

namespace TaskBasedUpdater
{
    public class Elevator
    {
        private static Elevator _instance;

        public event EventHandler<ElevationRequestData> ElevationRequested;

        public static Elevator Instance => _instance ??= new Elevator();

        public static Lazy<bool> LazyProcessElevated = new Lazy<bool>(IsElevated);

        public static bool IsProcessElevated => LazyProcessElevated.Value;

        private Elevator()
        {
        }

        public bool RequestElevation(UnauthorizedAccessException accessException, IComponent component)
        {
            if (IsElevated())
                return false;
            var data = new ElevationRequestData(accessException, component);
            OnElevationRequested(data);
            return true;
        }

        public static void RestartElevated()
        {
            ApplicationRestartManager.RestartApplication(true);
        }

        private static bool IsElevated()
        {
            return false;
        }

        protected virtual void OnElevationRequested(ElevationRequestData data)
        {
            ElevationRequested?.Invoke(this, data);
        }
    }

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

    public class ElevationRequireException : Exception
    {
        public IEnumerable<ElevationRequestData> Requests { get; }

        public ElevationRequireException(IEnumerable<ElevationRequestData> requests)
        {
            Requests = requests;
        }
    }
}
