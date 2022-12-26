using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

public partial class CommandDefinition : ObservableObject, ICommandDefinition
{
    [ObservableProperty] private string? _tooltip;
    [ObservableProperty] private ImageMoniker _image;
    [ObservableProperty] private ICommand? _command;

    public string Text { get; }

    public CommandDefinition(string text)
    {
        Text = text;
    }
}