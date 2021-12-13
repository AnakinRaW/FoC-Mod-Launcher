using System.Windows;
using System.Windows.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Input;

/// <summary>
/// Places a given context menu on the screen.
/// </summary>
public interface IContextMenuPlotter
{
    /// <summary>
    /// Shows a context menu at specified position.
    /// </summary>
    /// <param name="contextMenu">The context menu to show</param>
    /// <param name="position">The absolute position.</param>
    /// <param name="placementTarget">the placement target of the context menu.</param>
    bool ShowContextMenu(ContextMenu contextMenu, Point position, UIElement? placementTarget);


    bool ShowContextMenu(ContextMenu contextMenu, UIElement? placementTarget);
}