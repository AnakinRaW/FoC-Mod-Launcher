using System;
using FocLauncher.Mods;

namespace FocLauncher.ModInfo
{
    public interface IModReference : IEquatable<IModReference>
    {
        string Identifier { get; }

        ModType Type { get; }
    }
}