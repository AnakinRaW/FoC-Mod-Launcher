using System;
using System.Collections.Generic;
using FocLauncherHost.Updater.Restart;

namespace FocLauncherHost.Updater
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

        public void RequestElevation(UnauthorizedAccessException accessException, string deniedLocation)
        {
            var data = new ElevationRequestData(accessException, deniedLocation);
            OnElevationRequested(data);
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
        public string Location { get; }

        public Exception Exception { get; }

        public ElevationRequestData(Exception exception, string location)
        {
            Exception = exception;
            Location = location;
        }

        public bool Equals(ElevationRequestData? other)
        {
            if (other is null)
                return false;
            return ReferenceEquals(this, other) || string.Equals(Location, other.Location, StringComparison.OrdinalIgnoreCase);
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
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Location);
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
