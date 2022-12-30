using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

public sealed partial class MenuItemViewModel : CommandBarControlViewModel, ICommandBarMenuControlViewModel
{
    public override CommandBarType Type => CommandBarType.Menu;

    [ObservableProperty] private bool _isEnabled = true;

    public ObservableCollection<ICommandBarControlViewModel> Items { get; } = new();

    public MenuItemViewModel(string text)
    {
        Text = text;
    }
}