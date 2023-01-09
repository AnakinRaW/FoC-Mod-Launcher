using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

internal sealed partial class MenuItemControlViewModel : CommandBarControlViewModel, ICommandBarMenuControlViewModel
{
    public override CommandBarType Type => CommandBarType.Menu;

    [ObservableProperty] private bool _isEnabled = true;

    public ObservableCollection<ICommandBarControlViewModel> Items { get; }

    public MenuItemControlViewModel(string text, IEnumerable<ICommandBarControlViewModel> items)
    {
        Text = text;
        Items = new ObservableCollection<ICommandBarControlViewModel>(items);
    }
}