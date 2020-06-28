using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FocLauncher.Game;
using FocLauncher.Input;
using FocLauncher.Mods;
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
            if (gameObject is null)
                return false;

            IGame game;
            switch (gameObject)
            {
                case IGame g:
                    game = g;
                    break;
                case IMod mod:
                    game = mod.Game;
                    break;
                default:
                    return false;
            }

            if (game is null)
                return false;

            return GameRunHelper.RunGame(game, null, gameObject.IconFile);
        }
    }
}