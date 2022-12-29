using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

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
        Id = id;
        CommandDefinition = commandDefinition;
    }
}