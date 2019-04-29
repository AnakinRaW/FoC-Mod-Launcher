using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FocLauncher.Core.Game;

namespace FocLauncher.Core.Mods
{
    public static class ModHelper
    {
        public static IEnumerable<IMod> FindMods(IGame focGame)
        {
            if (focGame == null)
                throw new ArgumentNullException(nameof(focGame));

            if (focGame is Eaw)
                throw new ArgumentException("EaW is not allowed");
            var mods = focGame is SteamGame ? SearchSteamMods(focGame) : SearchDiskMods(focGame);

            var result = mods.ToList();
            result.Insert(0, new DummyMod());
            return result;
        }

        private static IEnumerable<IMod> SearchDiskMods(IGame game)
        {
            var modsPath = Path.Combine(game.GameDirectory, "Mods");
            if (!Directory.Exists(modsPath))
                return new List<IMod>();
            var modDirs = Directory.EnumerateDirectories(modsPath);
            return modDirs.Select(modDir => new Mod(modDir)).Cast<IMod>().ToList();
        }

        private static IEnumerable<IMod> SearchSteamMods(IGame game)
        {
            var mods = new List<IMod>();
            mods.AddRange(SearchDiskMods(game));

            var workshopsPath = Path.Combine(game.GameDirectory, @"..\..\..\workshop\content\32470\");
            if (!Directory.Exists(workshopsPath))
                return mods;

            var modDirs = Directory.EnumerateDirectories(workshopsPath);
            mods.AddRange(modDirs.Select(modDir => new Mod(modDir, true)));
            return mods;
        }
    }
}
