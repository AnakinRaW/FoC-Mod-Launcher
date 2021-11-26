using System.Collections.Generic;
using System.Windows;

namespace FocLauncher.Controls.Controllers
{
    internal interface IContextMenuController
    {
        bool ShowContextMenu(IEnumerable<object> items, Point location);
    }
}