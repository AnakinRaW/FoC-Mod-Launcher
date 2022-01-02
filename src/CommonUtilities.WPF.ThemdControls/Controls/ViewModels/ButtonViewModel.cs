using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public partial class ButtonViewModel : ObservableObject, IButtonViewModel
{
    [ObservableProperty]
    private string _id;
    [ObservableProperty]
    private bool _themed;
    [ObservableProperty]
    private bool _isDefault;
    [ObservableProperty]
    private bool _isCancel;
    [ObservableProperty]
    private ICommandDefinition _commandDefinition;

    public ButtonViewModel(string id, ICommandDefinition commandDefinition)
    {
        _id = id;
        _commandDefinition = commandDefinition;
    }
}

public partial class DropDownButtonViewModel : ButtonViewModel, IDropDownButtonViewModel
{
    public bool IsEnabled { get; set; }
    public ObservableCollection<ICommandDefinition> DropdownButtons { get; }

    public DropDownButtonViewModel(string id, ICommandDefinition commandDefinition) : base(id, commandDefinition)
    {
    }
}

public partial class CommandDefinition : ObservableObject, ICommandDefinition
{
    public string Text { get; }

    [ObservableProperty] private string? _tooltip;
    [ObservableProperty] private ImageMoniker _image;
    [ObservableProperty] private ICommand? _command;

    public CommandDefinition(string text)
    {
        Text = text;
    }
}