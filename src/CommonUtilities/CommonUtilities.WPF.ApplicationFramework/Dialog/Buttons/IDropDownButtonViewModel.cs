using System.Collections.ObjectModel;
using System.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;

public interface IDropDownButtonViewModel : IButtonViewModel, INotifyPropertyChanged
{
    bool IsEnabled { get; set; }

    ObservableCollection<ICommandDefinition> DropdownButtons { get; }
}