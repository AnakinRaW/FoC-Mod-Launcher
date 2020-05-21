using System;
using System.Collections.Generic;
using FocLauncher.Mods;

namespace FocLauncher.Game
{
    public interface IModContainer
    {
        event EventHandler<ModCollectionChangedEventArgs> ModCollectionModified;

        IReadOnlyCollection<IMod> Mods { get; }

        bool AddMod(IMod mod);

        bool RemoveMod(IMod mod);
    }
}