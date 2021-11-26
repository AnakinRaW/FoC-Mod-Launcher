using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FocLauncher.Controls.Controllers
{
    internal class ContextMenuController : ControllerBase<IHasContextMenuController, IContextMenuController>
    {
        public static bool ShowContextMenu(IEnumerable<IHasContextMenuController> items, Point location)
        {
            return ShowContextMenu(items, location, item => item.ContextMenuController);
        }

        private static bool ShowContextMenu(
            IEnumerable<IHasContextMenuController> items,
            Point location,
            Func<IHasContextMenuController, IContextMenuController> getController)
        {
            var source = BuildInvocationMap(items, getController);
            if (source.Count != 1)
                return false;
            var keyValuePair = source.First();
            return keyValuePair.Key.ShowContextMenu(keyValuePair.Value, location);
        }
    }
}