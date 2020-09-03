using System;

namespace FocLauncher.Items
{
    public class LauncherItemEventArgs : EventArgs
    {
        public ILauncherItem Item { get; }

        public LauncherItemEventArgs(ILauncherItem item)
        {
            Item = item;
        }
    }
}