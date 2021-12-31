using System.ComponentModel;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IWindowViewModel : INotifyPropertyChanged
{
    WindowState MinMaxState { get; set; }

    bool LeftToRight { get; set; }

    string Title { get; set; }

    bool IsFullScreen { get; set; }

    bool IsResizable { get; set; }

    bool HasMinimizeButton { get; set; }

    bool HasMaximizeButton { get; set; }

    bool IsGripVisible { get; set; }
}