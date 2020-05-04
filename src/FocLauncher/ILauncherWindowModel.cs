using System.Collections.Generic;
using FocLauncher.Game;
using FocLauncher.Mods;

namespace FocLauncher
{
    internal interface ILauncherWindowModel
    {
        GameType GameType { get; set; }

        bool UseDebugBuild { get; set; }

        bool IgnoreAsserts { get; set; }

        bool NoArtProcess { get; set; }

        bool Windowed { get; set; }

        IReadOnlyCollection<IMod> Mods { get; }
    }
}