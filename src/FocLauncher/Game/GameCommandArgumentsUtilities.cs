﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using EawModinfo.Spec;
using FocLauncher.Mods;

namespace FocLauncher.Game
{
    internal static class GameCommandArgumentsUtilities
    {
        internal static void Validate(this GameCommandArguments args)
        {
            if (args.Mods != null && args.Mods.Any(x => x.Type == ModType.Virtual))
                throw new GameArgumentException("The mod list must not contain any virtual mods");
        }

        internal static string ToArgs(this GameCommandArguments args)
        {
            var sb = new StringBuilder();

            sb.Append(CreateModArgs(args.Mods));
            
            if (args.IgnoreAsserts)
                sb.Append(" IGNOREASSERTS");
            if (args.NoArtProcess)
                sb.Append(" NOARTPROCESS");
            if (args.Windowed)
                sb.Append(" WINDOWED");

            return sb.ToString().Trim(' ');
        }

        private static string CreateModArgs(IList<IMod>? mods)
        {
            if (mods is null || !mods.Any())
                return string.Empty;
            if (mods.Count == 1)
                return mods[0].ToArgs(false);

            var sb = new StringBuilder();
            foreach (var mod in mods)
            {
                sb.Append(mod.ToArgs(false));
                sb.Append(' ');
            }

            return sb.ToString();
        }

    }
}