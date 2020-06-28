using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                    return false;
            }

            if (game is null)
                return false;

            var gameOptions = LauncherGameOptions.Instance;
            var args = new GameCommandArguments();
            gameOptions.FillArgs(args);

            if (mod != null)
            {
                var modList = GetAllMods(mod);
                if (!modList.Any())
                    return false;
                args.Mods = modList;
            }
            
            RunGame(game, args, gameObject.IconFile, gameOptions.UseDebugBuild);
            return true;
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