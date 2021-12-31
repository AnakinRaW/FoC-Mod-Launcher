using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls.ViewModels;

public interface IUnhandledExceptionDialogViewModel : INotifyPropertyChanged
{
    string WindowCaption { get; }

    string? Header { get; }

    string? HandlerDescription { get; }

    Exception Exception { get; }

    ICommand? Handler { get; }

    ImageSource? HandlerIcon { get; }

    string? HandlerName { get; }
}