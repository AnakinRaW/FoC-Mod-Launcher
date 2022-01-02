using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IButtonViewModel
{
    string Id { get; }

    ICommandDefinition CommandDefinition { get; }

    bool IsDefault { get; }

    bool IsCancel { get; }

    bool Themed { get; }
}

public interface IDropDownButtonViewModel : IButtonViewModel, INotifyPropertyChanged
{
    bool IsEnabled { get; set; }

    ObservableCollection<ICommandDefinition> DropdownButtons { get; }
}

public interface ICommandDefinition
{
    string Text { get; }

    string? Tooltip { get; }

    ImageMoniker Image { get; }

    ICommand? Command { get; }
}