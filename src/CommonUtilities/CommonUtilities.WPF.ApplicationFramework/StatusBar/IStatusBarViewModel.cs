using System.ComponentModel;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

public interface IStatusBarViewModel : INotifyPropertyChanged
{
    bool IsVisible { get; set; }
}