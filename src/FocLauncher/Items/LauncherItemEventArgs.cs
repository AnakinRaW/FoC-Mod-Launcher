using System;
using FocLauncher.Game;

namespace FocLauncher.Items
{
    public class LauncherItemEventArgs : EventArgs
    {
        public IPetroglyhGameableObject Item { get; }

        public LauncherItemEventArgs(IPetroglyhGameableObject item)
        {
            Item = item;
        }
    }
}