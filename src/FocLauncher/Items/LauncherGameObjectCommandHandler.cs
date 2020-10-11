using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FocLauncher.Game;
using FocLauncher.Input;
using FocLauncher.Mods;
using FocLauncher.Utilities;
using Microsoft.VisualStudio.Threading;

namespace FocLauncher.Items
{
    internal class LauncherGameObjectCommandHandler
    {
        public IPetroglyhGameableObject GameObject { get; }

        internal LauncherGameObjectCommandHandler(IPetroglyhGameableObject gameObject)
        {
            GameObject = gameObject;
        }

        public ICommand OpenExplorerCommand => new UICommand(OpenInExplorer, () => GameObject is IHasDirectory);

        public ICommand ShowArgsCommand => new UICommand(ShowArgs, () => GameObject is IMod);

        public ICommand ShowLanguages => new UICommand(ShowInstalledLangs, () => true);

        private void ShowInstalledLangs()
        {
            var message = GameObject.InstalledLanguages.Aggregate("Installed languages:\r\n",
                (current, language) => current + $"{language.GetLanguageEnglishName()}:{language.Support}\r\n");
            MessageBox.Show(message);
        }

        internal void OpenInExplorer()
        {
            var directory = (GameObject as IHasDirectory)?.Directory;
            if (directory == null || !directory.Exists)
                return;
            // TODO: Use ShellExecuteW (see DefaultCommandSet2KHandler)
            Process.Start("explorer.exe", directory.FullName);
        }

        internal void ShowArgs()
        {
            if (!(GameObject is IMod mod))
                return;
            var traverser = new ModDependencyTraverser(mod);
            var gameArgs = new GameCommandArguments { Mods = traverser.Traverse() };
            var args = gameArgs.ToArgs();
            var message = $"Launch Arguments for Mod {GameObject.Name}:\r\n\r\n{args}\r\n\r\nMod's location: '{mod.Identifier}'";

            Task.Run(() => MessageBox.Show(message, "FoC Launcher", MessageBoxButton.OK)).Forget();
        }

        internal static bool Launch(IPetroglyhGameableObject gameObject, IReadOnlyList<IPetroglyhGameableObject>? linkedGameObjects = null)
        {
            try
            {
                LaunchCore(gameObject, linkedGameObjects);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unable to start {gameObject.Name}:\r\n" +
                                e.Message, "FoC Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;

        }

        private static void LaunchCore(IPetroglyhGameableObject gameObject, IReadOnlyList<IPetroglyhGameableObject>? linkedGameObjects = null)
        {
            if (gameObject is null)
                throw new InvalidOperationException("No object to launch selected");

            IGame game;
            IMod? mod = null;
            switch (gameObject)
            {
                case IGame g:
                    game = g;
                    break;
                case IMod m:
                    mod = m;
                    game = mod.Game;
                    break;
                default:
                    throw new NotSupportedException("Selected object is not supported");
            }

            if (game is null)
                throw new InvalidOperationException("No game to launch.");

            var gameOptions = LauncherGameOptions.Instance;
            var args = new GameCommandArguments();
            gameOptions.FillArgs(args);

            if (mod != null)
            {
                var modList = GetAllMods(mod);
                if (!modList.Any()) 
                    throw new InvalidOperationException("The selected mod object seems to be invalid.");
                args.Mods = modList;
            }

            args.Language = gameOptions.GetLanguageFromOptions(gameObject);

            RunGame(game, args, gameObject.IconFile, gameOptions.UseDebugBuild);
        }

        private static void RunGame(IGame game, GameCommandArguments arguments, string? iconFile, bool debug)
        {
            if (debug && game is IDebugable debugable)
                debugable.DebugGame(arguments, iconFile);
            else 
                game.PlayGame(arguments, iconFile);
        }

        private static IList<IMod> GetAllMods(IMod mod)
        {
            var traverser = new ModDependencyTraverser(mod);
            if (traverser.HasDependencyCycles())
            {
                MessageBox.Show("The Mod's dependencies result in a cycles. Cannot Start the Mod!");
                return new List<IMod>(0);
            }
            return traverser.Traverse();
        }
    }
}