using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FocLauncher.Controls;
using FocLauncher.Game;
using FocLauncher.Game.Detection;
using FocLauncher.Input;
using FocLauncher.Items;
using Microsoft.VisualStudio.Threading;
using NLog;

namespace FocLauncher
{

    //public class PetroglyphInitialization
    //{
    //    public void Initialize()
    //    {
    //        RegisterThemes();
    //    }

    //    private void RegisterThemes()
    //    {
    //        foreach (var mod in _gameObjects.OfType<IMod>())
    //            RegisterTheme(mod);
    //    }

    //    private static void RegisterTheme(IMod mod)
    //    {
    //        var custom = mod.ModInfo?.Custom;
    //        if (custom == null || !custom.ContainsKey("launcherTheme"))
    //            return;
    //        var relativeThemePath = custom.Value<string>("launcherTheme");
    //        var themePath = Path.Combine(mod.ModDirectory, relativeThemePath);
    //        if (!File.Exists(themePath))
    //            return;

    //        var theme = ThemeManager.GetThemeFromFile(themePath);
    //        if (theme == null)
    //            return;

    //        ThemeManager.Instance.RegisterTheme(theme);
    //        ThemeManager.Instance.AssociateThemeToMod(mod, theme);
    //    }
    //}


    public class MainWindowViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ICommand LaunchCommand => new UICommand(ExecutedLaunch, CanExecute);

        private LauncherListBoxPane ListBoxPane { get; }

        public MainWindowViewModel(MainWindow window)
        {
            ListBoxPane = window.ListBoxPane;
            ListBoxPane.Focus();
            InitializeLauncherWindowAsync().ForgetButThrow();
        }

        private async Task InitializeLauncherWindowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;
                var gameDetection = await FindGamesAsync();


                if (gameDetection.IsError)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    CloseApplication(gameDetection);
                }

                await SetupGamesAsync(gameDetection);

                await ListBoxPane.AddGameAsync(LauncherGameManager.Instance.ForcesOfCorruption!);
                await ListBoxPane.AddGameAsync(LauncherGameManager.Instance.EmpireAtWar!, false);
            });
            ListBoxPane.Focus();
        }

        private static async Task<GameDetection> FindGamesAsync()
        {
            try
            {
                var gameDetection = GameDetection.NotInstalled;
                await Task.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    gameDetection = GameDetectionHelper.GetGameInstallations();
                }).ConfigureAwait(false);

                return gameDetection;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to initialize MainWindow view model: {e.Message}");
                throw;
            }
        }

        private Task SetupGamesAsync(GameDetection e)
        {
            var gameManager = LauncherGameManager.Instance;
            gameManager.Initialize(e);
            var foc = gameManager.ForcesOfCorruption;
            var eaw = gameManager.EmpireAtWar;
            RegisterEvents();

            new TaskFactory().StartNew(() =>
            {
                foc!.Setup(GameSetupOptions.ResolveModDependencies);
                //eaw!.Setup(GameSetupOptions.ResolveModDependencies);
            }).Forget();

            return Task.CompletedTask;
        }


        private static void RegisterEvents()
        {
            LauncherGameManager.Instance.GameStarted += OnGameStarted;
        }

        private static void OnGameStarted(object sender, GameStartedEventArgs e)
        {
            //if (Settings.Default.AutoSwitchTheme && ThemeManager.Instance.TryGetThemeByMod(e as IMod, out var theme))
            //    ThemeManager.Instance.Theme = theme;
        }

        private static void ExecutedLaunch(object obj)
        {
            if (!(obj is IPetroglyhGameableObject gameObject))
                return;
            LauncherGameObjectCommandHandler.Launch(gameObject);
        }

        private static bool CanExecute(object obj)
        {
            return obj is IPetroglyhGameableObject;
        }

        private static void CloseApplication(GameDetection gameDetection)
        {
            var message = string.Empty;
            if (gameDetection.EawExe == null || !gameDetection.EawExe.Exists)
                message = "Could not find Empire at War!\r\n";
            else if (gameDetection.FocExe == null || !gameDetection.FocExe.Exists)
                message += "Could not find Forces of Corruption\r\n";
            MessageBox.Show(message + "\r\nThe launcher will now be closed", "FoC Launcher",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }
}
