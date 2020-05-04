using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocLauncher.Mods;

namespace FocLauncher.Game
{
    internal static class GameCommandArgumentsUtilities
    {
        internal static string ToArgs(this GameCommandArguments args)
        {
            var sb = new StringBuilder();

            sb.Append(CreateModArgs(args.Mods));
            
            if (args.IgnoreAsserts)
                sb.Append("IGNOREASSERTS ");
            if (args.NoArtProcess)
                sb.Append("NOARTPROCESS ");
            if (args.Windowed)
                sb.Append("WINDOWED ");

            return sb.ToString().TrimEnd(' ');
        }

        private static string CreateModArgs(IList<IMod> mods)
        {
            if (mods is null || !mods.Any())
                return string.Empty;
            if (mods.Count == 1)
                return ModToArg(mods[0]);

            var sb = new StringBuilder();
            foreach (var mod in mods.Reverse())
            {
                sb.Append(ModToArg(mod));
                sb.Append(' ');
            }

            return sb.ToString();
        }

        private static string ModToArg(IMod mod)
        {
            return mod.WorkshopMod ? $"STEAMMOD={mod.FolderName}" : $"MODPATH=Mods/{mod.FolderName}";
        }
    }
}