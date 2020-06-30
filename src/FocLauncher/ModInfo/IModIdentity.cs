using System;
using System.Collections.Generic;
using FocLauncher.Versioning;

namespace FocLauncher.ModInfo
{
    public interface IModIdentity : IEquatable<IModIdentity>
    {
        string Name { get; }
        ModVersion? Version { get; }
        IList<IModReference> Dependencies { get; }
    }
}