using System;

namespace FocLauncherHost.Updater.Component
{
    public class Component : IComponent
    {
        public string Destination { get; set; }

        public string Name { get; set; }

        public ComponentAction RequiredAction { get; set; }

        public CurrentState CurrentState { get; set; }

        public Version? CurrentVersion { get; set; }

        public OriginInfo? OriginInfo { get; set; }
        
        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name) ? $"{Name},destination='{Destination}'" : base.ToString();
        }

        public bool Equals(IComponent other)
        {
            return ComponentIdentityComparer.Default.Equals(this, other);
        }
    }
}
