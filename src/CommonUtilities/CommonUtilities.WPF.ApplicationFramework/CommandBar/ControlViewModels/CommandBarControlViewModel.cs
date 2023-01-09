using CommunityToolkit.Mvvm.ComponentModel;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

internal abstract partial class CommandBarControlViewModel : ObservableObject, ICommandBarControlViewModel
{
    public abstract CommandBarType Type { get; }

    [ObservableProperty] private string? _text;
    [ObservableProperty] private string? _tooltip;
}