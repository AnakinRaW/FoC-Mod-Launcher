using System;
using FocLauncher.Mods;

namespace FocLauncher.Game
{
    public class ModCollectionChangedEventArgs : EventArgs
    {
        public ModCollectionChangedAction Action { get; }

        public IMod Mod { get; }

        public ModCollectionChangedEventArgs(IMod mod, ModCollectionChangedAction action)
        {
            Mod = mod;
            Action = action;
        }
    }
}