using System;
using System.IO;
using FocLauncher.Game;

namespace FocLauncher
{
    /*
    public class PetroglyphInitialization
    {
        private ICollection<IPetroglyhGameableObject> _gameObjects;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Initialized;

        public void Initialize()
        {
            SteamModNamePersister.CreateInstance();

            SearchGameObjects();
            RegisterThemes();
            OnInitialized();
        }
        
        internal IReadOnlyCollection<IPetroglyhGameableObject> SearchGameObjects()
        {
            if (_gameObjects == null)
            {
                _gameObjects = new HashSet<IPetroglyhGameableObject> {FoC};
                foreach (var mod in ModHelper.FindMods(FoC))
                    _gameObjects.Add(mod);
            }
            return _gameObjects.ToList();
        }

        private void RegisterThemes()
        {
            foreach (var mod in _gameObjects.OfType<IMod>())
                RegisterTheme(mod);
        }

        private static void RegisterTheme(IMod mod)
        {
            var custom = mod.ModInfoFile?.Custom;
            if (custom == null || !custom.ContainsKey("launcherTheme"))
                return;
            var relativeThemePath = custom.Value<string>("launcherTheme");
            var themePath = Path.Combine(mod.ModDirectory, relativeThemePath);
            if (!File.Exists(themePath))
                return;

            var theme = ThemeManager.GetThemeFromFile(themePath);
            if (theme == null)
                return;

            ThemeManager.Instance.RegisterTheme(theme);
            ThemeManager.Instance.AssociateThemeToMod(mod, theme);
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }
    }

    */

    public class GameFactory
    {
        public static IGame CreateFocGame(DirectoryInfo directory, GameType type)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            switch (type)
            {
                case GameType.SteamGold:
                    return new SteamGame(directory.FullName);
                case GameType.Disk:
                case GameType.Origin:
                case GameType.GoG:
                case GameType.DiskGold:
                    return new Foc(directory.FullName, type);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public static IGame CreateEawGame(DirectoryInfo directory, GameType type)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            // TODO: Consume type and create SteamGameEaw
            return new Eaw(directory.FullName);
        }
    }
}