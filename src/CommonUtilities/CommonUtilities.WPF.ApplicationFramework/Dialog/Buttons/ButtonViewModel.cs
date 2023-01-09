using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;

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