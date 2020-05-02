using System.Text;
using FocLauncher.Mods;

namespace FocLauncher.Game
{
    internal static class CommandArgumentsUtilities
    {
        internal static string ToArgs(this GameCommandArguments args)
        {
            var sb = new StringBuilder();

            var mod = args.Mod;
            if (mod != null && !string.IsNullOrEmpty(mod.FolderName))
            {
                var modArg = mod.WorkshopMod ? $"STEAMMOD={mod.FolderName}" : $"MODPATH=Mods/{mod.FolderName}";
                sb.Append(modArg);
                sb.Append(' ');
            }

            if (args.IgnoreAsserts)
                sb.Append("IGNOREASSERTS ");
            if (args.NoArtProcess)
                sb.Append("NOARTPROCESS ");

            return sb.ToString().TrimEnd(' ');
        }
    }
    
    public sealed class GameCommandArguments
    {
        public bool IgnoreAsserts { get; set; }

        public bool NoArtProcess { get; set; }

        public bool Windowed { get; set; }

        public IMod? Mod { get; set; }

        public GameCommandArguments()
        {
        }

        public GameCommandArguments(IMod mod)
        {
            Mod = mod;
        }
    }
}