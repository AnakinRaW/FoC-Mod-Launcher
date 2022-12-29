using CommunityToolkit.Mvvm.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

public abstract partial class CommandBarControlViewModel : ObservableObject, ICommandBarControlViewModel
{
    public abstract CommandBarType Type { get; }

    [ObservableProperty] private string? _text;
    [ObservableProperty] private string? _tooltip;
}