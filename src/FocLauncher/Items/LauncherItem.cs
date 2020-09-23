using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using EawModinfo.Spec;
using FocLauncher.Controls.Controllers;
using FocLauncher.Game;
using FocLauncher.Mods;
using FocLauncher.Utilities;
using Microsoft.VisualStudio.Threading;

namespace FocLauncher.Items
{
    public class LauncherItem : ILauncherItem, IHasInvocationController, IHasContextMenuController
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly LauncherGameObjectCommandHandler _commandHandler;

        internal IEnumerable<MenuItem> MenuItems => CreateMenuItems();

        public string Text { get; private set; } = "Resolving name...";

        // TODO: Get async from GameObject
        public ImageSource ImageSource { get; }

        public LauncherItemManager Manager { get; }

        public IPetroglyhGameableObject GameObject { get; }

        public int Depth
        {
            get
            {
                if (GameObject is IGame)
                    return 0;
                return 1;
            }
        }

        IInvocationController IHasInvocationController.InvocationController => LauncherItemInvocationController.Instance;

        IContextMenuController IHasContextMenuController.ContextMenuController => LauncherItemContextMenuController.Instance;


        public LauncherItem(LauncherItemManager manager, IPetroglyhGameableObject gameObject)
        {
            Manager = manager;
            GameObject = gameObject;
            _commandHandler = new LauncherGameObjectCommandHandler(GameObject);
            SetNameAsync().Forget();
        }

        public override string ToString()
        {
            return Text;
        }
        
        private IEnumerable<MenuItem> CreateMenuItems()
        {
            var items = new HashSet<MenuItem>();
            var openExplorerItem = new MenuItem {Header = "Show in Explorer", Command = _commandHandler.OpenExplorerCommand};
            items.Add(openExplorerItem);
#if DEBUG
            var showArgs = new MenuItem { Header = "Show Launch Args", Command = _commandHandler.ShowArgsCommand };
            items.Add(showArgs);

            var showLangs = new MenuItem{ Header = "Installed Languages", Command = _commandHandler.ShowLanguages};
            items.Add(showLangs);
#endif
            return items;
        }

        private async Task SetNameAsync()
        {
            if (!(GameObject is Mod mod))
            {
                Text = GameObject.Name;
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

            Text = name!;
            OnPropertyChanged(nameof(Text));
        }
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static IReadOnlyList<IPetroglyhGameableObject> GetSelectionGameObject(IEnumerable<object> items)
        {
            return items.OfType<LauncherItem>().Select(x => x.GameObject).ToList();
        }

        private class LauncherItemInvocationController  : IInvocationController
        {
            private static IInvocationController? _instance;

            public static IInvocationController Instance
            {
                get
                {
                    return _instance ??= new LauncherItemInvocationController();
                }
            }

            public bool Invoke(IEnumerable<object> items)
            {
                var itemsList = GetSelectionGameObject(items);

                if (itemsList.Count == 0)
                    return false;

                if (itemsList.Count != 1)
                    return false;

                return LauncherGameObjectCommandHandler.Launch(itemsList[0]);
            }
        }

        private class LauncherItemContextMenuController : IContextMenuController
        {
            private static IContextMenuController? _instance;

            public static IContextMenuController Instance => _instance ??= new LauncherItemContextMenuController();


            public bool ShowContextMenu(IEnumerable<object> items, Point location)
            {
                if (items is null)
                    return false;
                var itemsList = items.ToList();
                if (itemsList.Count != 1)
                    return false;

                if (!(itemsList[0] is LauncherItem item))
                    return false;

                var contextMenu = BuildContextMenu(item);

                contextMenu.Placement = PlacementMode.AbsolutePoint;
                contextMenu.HorizontalOffset = location.X;
                contextMenu.VerticalOffset = location.Y;
                contextMenu.PlacementRectangle = new Rect(location, Size.Empty);
                contextMenu.IsOpen = true;

                return true;
            }

            private static ContextMenu BuildContextMenu(LauncherItem item)
            {
                var c = new ContextMenu();
                foreach (var menuItem in item.MenuItems) 
                    c.Items.Add(menuItem);
                return c;
            }
        }
    }

}