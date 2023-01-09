using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

internal sealed partial class ButtonControlViewModel : CommandBarControlViewModel, IExecutableCommandBarControlViewModel
{
    [ObservableProperty] private string? _shortcutText;
    [ObservableProperty] private ImageKey? _imageKey;

    public override CommandBarType Type => CommandBarType.Button;

    public ICommand Command { get; }

    public ButtonControlViewModel(ICommandDefinition commandDefinition)
    {
        Text = commandDefinition.Text;
        Command = commandDefinition.Command;
        ImageKey = commandDefinition.Image;
        Tooltip = commandDefinition.Tooltip;
    }
}