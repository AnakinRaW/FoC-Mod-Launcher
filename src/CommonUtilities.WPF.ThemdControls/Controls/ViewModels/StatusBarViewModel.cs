using System.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IStatusBarViewModel : INotifyPropertyChanged
{
    bool IsVisible { get; set; }
}