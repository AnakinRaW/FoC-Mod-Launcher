using FocLauncher.Game;

namespace FocLauncher
{
    internal interface ILauncherWindowModel
    {
        GameType GameType { get; set; }

        bool UseDebugBuild { get; set; }

        bool IgnoreAsserts { get; set; }

        bool NoArtProcess { get; set; }

        bool Windowed { get; set; }
    }
}