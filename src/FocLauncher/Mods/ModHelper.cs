using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FocLauncher.Game;
using FocLauncher.ModInfo;

namespace FocLauncher.Mods
{
    public static class ModHelper
    {
        public static IEnumerable<IMod> FindMods(IGame focGame)
        {
            if (focGame == null)
                throw new ArgumentNullException(nameof(focGame));

            if (focGame is Eaw)
                throw new ArgumentException("EaW is not allowed");
            return focGame is SteamGame ? SearchSteamMods(focGame) : SearchDiskMods(focGame);
        }

        private static IEnumerable<IMod> SearchDiskMods(IGame game)
        {
            var modsPath = Path.Combine(game.Directory.FullName, "Mods");
            if (!Directory.Exists(modsPath))
                return new List<IMod>();
            var modDirs = Directory.EnumerateDirectories(modsPath);
            return modDirs.Select(modDir =>
            {
                return new Mod(game, new DirectoryInfo(modDir), false);
            }).Cast<IMod>().ToList();
        }

        private static IEnumerable<IMod> SearchSteamMods(IGame game)
        {
            var mods = new List<IMod>();
            mods.AddRange(SearchDiskMods(game));

            var workshopsPath = Path.Combine(game.Directory.FullName, @"..\..\..\workshop\content\32470\");
            if (!Directory.Exists(workshopsPath))
                return mods;

            var modDirs = Directory.EnumerateDirectories(workshopsPath);
            mods.AddRange(modDirs.Select(modDir =>
            {
                return new Mod(game, new DirectoryInfo(modDir), true);
            }));
            return mods;
        }
    }

    public static class ModFactory
    {
        public static IMod CreateMod(IGame game, ModType type, DirectoryInfo directory, ModInfoData modInfo)
        {
            throw new NotImplementedException();
        }

        public static IMod CreateMod(IGame game, ModType type, DirectoryInfo directory, bool searchModFileOnDisk)
        {
            throw new NotImplementedException();
        }

        //public ModInfoData? ModInfo
        //{
        //    get
        //    {
        //        //if (_modInfo == null && !_modfileWasLookedUp)
        //        //{
        //        //    if (ModInfo.ModInfoFile.TryParse(Path.Combine(ModDirectory, "modinfo.json"), out var modInfo))
        //        //        _modInfo = modInfo;
        //        //    else
        //        //        _modInfo = null;
        //        //    _modfileWasLookedUp = true;
        //        //}

        //        return _modInfo;
        //    }
        //}
    }
}
