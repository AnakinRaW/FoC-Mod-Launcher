using System.Collections.ObjectModel;
using System.ComponentModel;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;

public interface IDropDownButtonViewModel : IButtonViewModel, INotifyPropertyChanged
{
    bool IsEnabled { get; set; }

    ObservableCollection<ICommandDefinition> DropdownButtons { get; }
}