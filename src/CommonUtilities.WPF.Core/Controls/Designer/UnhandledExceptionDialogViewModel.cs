using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.Controls.ViewModels;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls.Designer;

internal class UnhandledExceptionDialogViewModel : IUnhandledExceptionDialogViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Exception Exception => new TestException("Test Message", new StackTrace().ToString());

    public string WindowCaption => "Window Title";

    public string Header => "Oh no, something went wrong!";

    public string? HandlerDescription =>
        "This is a long text, telling the user what the custom button can do. This can be logging the error, etc.";

    public ICommand? Handler => new RoutedUICommand();
    public ImageSource? HandlerIcon => null;

    public string? HandlerName => "_Handle...";


    private class TestException : Exception
    {
        public override string StackTrace { get; }

        public TestException(string message, string stackTrace) : base(message)
        {
            StackTrace = stackTrace;
        }
    }
}