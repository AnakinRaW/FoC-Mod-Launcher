using System.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public interface IStatusBarViewModel : INotifyPropertyChanged
{
    bool IsVisible { get; set; }
}