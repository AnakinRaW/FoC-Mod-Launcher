using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FocLauncher.Controls.Controllers;
using FocLauncher.Game;
using FocLauncher.Input;
using FocLauncher.Mods;
using FocLauncher.Utilities;
using Microsoft.VisualStudio.Threading;

namespace FocLauncher
{
    public class LauncherListItemModel : IHasInvocationController, IHasContextMenuController, INotifyPropertyChanged
    {
        private string _name = "Resolving name...";
        internal IEnumerable<MenuItem> MenuItems => CreateMenuItems();

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public IPetroglyhGameableObject GameObject { get; }

        public IInvocationController InvocationController { get; }

        public ICommand OpenExplorerCommand => new UICommand(OpenInExplorer, () => GameObject is IHasDirectory);

        public ICommand ShowArgsCommand => new UICommand(ShowArgs, () => GameObject is IMod);


        public LauncherListItemModel(IPetroglyhGameableObject gameObject, IInvocationController controller)
        {
            GameObject = gameObject;
            InvocationController = controller;
            SetNameAsync().Forget();
        }

        IContextMenuController IHasContextMenuController.ContextMenuController => LauncherItemContextMenuController.Instance;


        private IEnumerable<MenuItem> CreateMenuItems()
        {
            var items = new HashSet<MenuItem>();
            var openExplorerItem = new MenuItem {Header = "Show in Explorer", Command = OpenExplorerCommand};
            items.Add(openExplorerItem);
            // TODO: Remove from Release
            var showArgs = new MenuItem {Header = "Show Launch Args", Command = ShowArgsCommand};
            items.Add(showArgs);
            return items;
        }


        private void OpenInExplorer()
        {
            var directory = (GameObject as IHasDirectory)?.Directory;
            if (directory == null || !directory.Exists)
                return;
            Process.Start("explorer.exe", directory.FullName);
        }

        private void ShowArgs()
        {
            if (!(GameObject is IMod mod))
                return;
            var traverser = new ModDependencyTraverser(mod);
            var gameArgs = new GameCommandArguments {Mods = traverser.Traverse()};
            var args = gameArgs.ToArgs();
            var message = $"Launch Arguments for Mod {GameObject.Name}:\r\n\r\n{args}\r\n\r\nMod's location: '{mod.Identifier}'";

            Task.Run(() => MessageBox.Show(message, "FoC Launcher", MessageBoxButton.OK));
        }


        private async Task SetNameAsync()
        {
            if (!(GameObject is Mod mod))
            {
                Name = GameObject.Name;
                return;
            }

            var name = mod.ModInfo?.Name;
            if (string.IsNullOrEmpty(name))
            {
                var folderName = mod.Directory.Name;
                switch (mod.Type)
                {
                    case ModType.Default:
                        name = mod.Directory.Name.Replace('_', ' ');
                        break;
                    case ModType.Workshops when SteamModNamePersister.Instance.TryFind(folderName, out var modName):
                        name = modName;
                        break;
                    case ModType.Workshops:
                    {
                        var doc = await HtmlDownloader.GetSteamModPageDocument(folderName);
                        name = new WorkshopNameResolver().GetName(doc, folderName);
                        SteamModNamePersister.Instance.AddModName(folderName, name);
                        break;
                    }
                }
            }
            
            if (string.IsNullOrEmpty(name))
                name = "Unnamed (Failed to resolve)";

            Name = name!;
        }


        private class LauncherItemContextMenuController : IContextMenuController
        {
            private static IContextMenuController _instance;

            public static IContextMenuController Instance => _instance ??= new LauncherItemContextMenuController();


            public bool ShowContextMenu(IEnumerable<object> items, Point location)
            {
                if (items is null)
                    return false;
                var itemsList = items.ToList();
                if (itemsList.Count != 1)
                    return false;

                if (!(itemsList[0] is LauncherListItemModel item))
                    return false;

                var contextMenu = BuildContextMenu(item);

                contextMenu.Placement = PlacementMode.AbsolutePoint;
                contextMenu.HorizontalOffset = location.X;
                contextMenu.VerticalOffset = location.Y;
                contextMenu.PlacementRectangle = new Rect(location, Size.Empty);
                contextMenu.IsOpen = true;

                return true;
            }

            private static ContextMenu BuildContextMenu(LauncherListItemModel item)
            {
                var c = new ContextMenu();
                foreach (var menuItem in item.MenuItems) c.Items.Add(menuItem);
                return c;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}