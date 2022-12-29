using System.Collections.ObjectModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;

public partial class DropDownButtonViewModel : ButtonViewModel, IDropDownButtonViewModel
{
    public bool IsEnabled { get; set; }
    public ObservableCollection<ICommandDefinition> DropdownButtons { get; }

    public DropDownButtonViewModel(string id, ICommandDefinition commandDefinition) : base(id, commandDefinition)
    {
    }
}