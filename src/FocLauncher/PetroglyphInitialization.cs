using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using FocLauncher.Game;
using FocLauncher.Game.Detection;
using FocLauncher.Mods;
using FocLauncher.Properties;
using FocLauncher.Theming;
using NLog;

namespace FocLauncher
{
    public class PetroglyphInitialization
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string IconPath = Path.Combine(LauncherConstants.ApplicationBasePath, "foc.ico");
        private ICollection<IPetroglyhGameableObject> _gameObjects;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Initialized;
        
        public PetroglyphGameManager GameManager { get; private set; }

        public IGame EaW { get; private set; }

        public IGame FoC { get; private set; }

        public GameType FocGameType { get; private set; }

        public void Initialize()
        {
            InitAppDataDirectory();

            if (!InitGames(out var result))
            {
                var message = string.Empty;
                if (result.EawExe == null || !result.EawExe.Exists)
                    message = "Could not find Empire at War!\r\n";
                else if (result.FocExe == null || !result.FocExe.Exists)
                    message += "Could not find Forces of Corruption\r\n";

                MessageBox.Show(message + "\r\nThe launcher will now be closed", "FoC Launcher", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            Logger.Info(GetInstalledGameInfo);
            SearchGameObjects();
            RegisterThemes();
            OnInitialized();
        }
        
        private bool InitGames(out GameDetection result)
        {
            result = GameDetectionHelper.GetGameInstallations();

            if (result.IsError)
                return false;

            try
            {
                EaW = new Eaw(result.EawExe.Directory?.FullName);
            }
            catch
            {
                return false;
            }

            switch (result.FocType)
            {
                case GameType.SteamGold:
                    FoC = new SteamGame(result.FocExe.Directory?.FullName);
                    SteamModNamePersister.CreateInstance();
                    break;
                case GameType.Disk:
                case GameType.Origin:
                case GameType.GoG:
                case GameType.DiskGold:
                    FoC = new Foc(result.FocExe.Directory?.FullName, result.FocType);
                    break;
            }

            FocGameType = result.FocType;
            return true;
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

        private void InitAppDataDirectory()
        {
            if (!Directory.Exists(LauncherConstants.ApplicationBasePath))
                Directory.CreateDirectory(LauncherConstants.ApplicationBasePath);
            if (File.Exists(IconPath))
                return;
            using var fs = new FileStream(IconPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            Resources.foc.Save(fs);
        }

        private string GetInstalledGameInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("----------Installed Game Information----------");
            sb.AppendLine(EaW == null ? "EaW is null" : $"EaW found at: {EaW.GameDirectory};");
            sb.AppendLine(FoC == null ? "FoC is null" : $"FoC found at: {FoC.GameDirectory}; FoC Version: {FocGameType}");
            return sb.ToString();
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
}