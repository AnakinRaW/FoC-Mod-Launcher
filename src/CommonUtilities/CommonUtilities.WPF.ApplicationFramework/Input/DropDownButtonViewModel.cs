using System.Collections.ObjectModel;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

public partial class DropDownButtonViewModel : ButtonViewModel, IDropDownButtonViewModel
{
    public bool IsEnabled { get; set; }
    public ObservableCollection<ICommandDefinition> DropdownButtons { get; }

    public DropDownButtonViewModel(string id, ICommandDefinition commandDefinition) : base(id, commandDefinition)
    {
    }
}