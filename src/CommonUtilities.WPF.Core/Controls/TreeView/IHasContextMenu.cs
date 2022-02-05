using Sklavenwalker.CommonUtilities.Wpf.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IHasContextMenu
{
    IContextMenuProvider ContextMenuProvider { get; }
}