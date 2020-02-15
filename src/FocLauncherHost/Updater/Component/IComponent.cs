using System;

namespace FocLauncherHost.Updater.Component
{
    public interface IComponent : IEquatable<IComponent>
    {
        string Destination { get; set; }

        string Name { get; set; }
        
        ComponentAction RequiredAction { get; set; }

        CurrentState CurrentState { get; set; }

        Version? CurrentVersion { get; set; }

        OriginInfo? OriginInfo { get; set; }

        long? DiskSize { get; set; }
    }
}