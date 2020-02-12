using System;

namespace FocLauncherHost.Updater.Component
{
    public class OriginInfo
    {
        public Uri Origin { get; }

        public Version? Version { get; }

        public ValidationContext? ValidationContext { get; }

        public OriginInfo(Uri origin, Version version = null, ValidationContext validationContext = null)
        {
            Origin = origin ?? throw new ArgumentNullException(nameof(origin));
            Version = version;
            ValidationContext = validationContext;
        }
    }
}