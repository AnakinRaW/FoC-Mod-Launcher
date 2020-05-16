using System.Collections.Generic;
using System.ComponentModel;
using FocLauncher.Mods;

namespace FocLauncher
{
    internal interface ILauncherWindowModel : INotifyPropertyChanged
    {
        bool UseDebugBuild { get; set; }

        bool IgnoreAsserts { get; set; }

        bool NoArtProcess { get; set; }

        bool Windowed { get; set; }

        IReadOnlyCollection<IMod> Mods { get; }
    }
}