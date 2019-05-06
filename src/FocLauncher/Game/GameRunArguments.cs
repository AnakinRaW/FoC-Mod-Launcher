using System.Text;

namespace FocLauncher.Core.Game
{
    public struct GameRunArguments
    {
        public bool UseDebug { get; set; }

        public bool IgnoreAsserts { get; set; }

        public bool NoArtProcess { get; set; }

        public bool IsWorkshopMod { get; set; }

        public string ModPath { get; set; }

        public string SteamMod { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (IsWorkshopMod && !string.IsNullOrEmpty(SteamMod))
                sb.Append($"STEAMMOD={SteamMod} ");
            else if (!IsWorkshopMod && !string.IsNullOrEmpty(ModPath))
                sb.Append($"MODPATH={ModPath} ");

            if (IgnoreAsserts)
                sb.Append("IGNOREASSERTS ");
            if (NoArtProcess)
                sb.Append("NOARTPROCESS ");

            return sb.ToString().TrimEnd(' ');
        }
    }
}