using System.Collections.ObjectModel;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

public sealed class MenuItemViewModel : CommandBarControlViewModel, ICommandBarMenuControlViewModel
{
    public override CommandBarType Type => CommandBarType.Menu;

    public bool IsEnabled { get; set; } = true;

    public ObservableCollection<ICommandBarControlViewModel> Items { get; } = new();

    public MenuItemViewModel()
    {
        Text = "132";
    }
}