using System;
using System.Windows.Input;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

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