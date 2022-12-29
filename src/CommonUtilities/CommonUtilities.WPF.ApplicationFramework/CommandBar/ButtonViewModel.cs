using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

public partial class ButtonViewModel : CommandBarControlViewModel, IExecutableCommandBarControlViewModel
{
    [ObservableProperty] private string? _shortcutText;
    [ObservableProperty] private ImageKey? _imageKey;

    public override CommandBarType Type => CommandBarType.Button;

    public ICommand Command { get; }

    public ButtonViewModel(ICommandDefinition commandDefinition)
    {
        Text = commandDefinition.Text;
        Command = commandDefinition.Command;
        ImageKey = commandDefinition.Image;
        Tooltip = commandDefinition.Tooltip;
    }
}