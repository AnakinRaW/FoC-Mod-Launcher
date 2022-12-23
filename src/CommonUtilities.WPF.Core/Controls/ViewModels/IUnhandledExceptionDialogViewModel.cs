using System;
using System.Windows.Input;
using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IUnhandledExceptionDialogViewModel : IModalWindowViewModel
{
    string WindowCaption { get; }

    string? Header { get; }

    string? HandlerDescription { get; }

    Exception Exception { get; }

    ICommand? Handler { get; }

    ImageSource? HandlerIcon { get; }

    string? HandlerName { get; }
}