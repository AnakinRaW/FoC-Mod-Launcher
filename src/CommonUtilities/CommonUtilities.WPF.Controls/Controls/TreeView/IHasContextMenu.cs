using AnakinRaW.CommonUtilities.Wpf.Input;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public interface IHasContextMenu
{
    IContextMenuProvider ContextMenuProvider { get; }
}