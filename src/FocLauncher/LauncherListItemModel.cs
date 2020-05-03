using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FocLauncher.Controls.Controllers;
using FocLauncher.Game;
using FocLauncher.Input;
using FocLauncher.Mods;

namespace FocLauncher
{
    public class LauncherListItemModel : IHasInvocationController, IHasContextMenuController
    {
        internal IEnumerable<MenuItem> MenuItems => CreateMenuItems();

        public IPetroglyhGameableObject GameObject { get; }

        public IInvocationController InvocationController { get; }

        public ICommand OpenExplorerCommand => new UICommand(OpenInExplorer, () => true);

        public LauncherListItemModel(IPetroglyhGameableObject gameObject, IInvocationController controller)
        {
            GameObject = gameObject;
            InvocationController = controller;
        }

        IContextMenuController IHasContextMenuController.ContextMenuController => LauncherItemContextMenuController.Instance;


        private IEnumerable<MenuItem> CreateMenuItems()
        {
            var items = new HashSet<MenuItem>();
            var openExplorerItem = new MenuItem {Header = "Show in Explorer", Command = OpenExplorerCommand};
            items.Add(openExplorerItem);
            return items;
        }


        private void OpenInExplorer()
        {
            string directory;
            switch (GameObject)
            {
                case IGame game:
                    directory = game.GameDirectory;
                    break;
                case IMod mod:
                    directory = mod.ModDirectory;
                    break;
                default:
                    throw new NotSupportedException("The game object type is not supported.");
            }

            if (!Directory.Exists(directory))
                return;
            Process.Start("explorer.exe", directory);
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
    }
}