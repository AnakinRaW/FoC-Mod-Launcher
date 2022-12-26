using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

public interface IDropDownButtonViewModel : IButtonViewModel, INotifyPropertyChanged
{
    bool IsEnabled { get; set; }

    ObservableCollection<ICommandDefinition> DropdownButtons { get; }
}