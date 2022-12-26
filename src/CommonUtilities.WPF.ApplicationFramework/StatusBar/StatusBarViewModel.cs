using System.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

public interface IStatusBarViewModel : INotifyPropertyChanged
{
    bool IsVisible { get; set; }
}