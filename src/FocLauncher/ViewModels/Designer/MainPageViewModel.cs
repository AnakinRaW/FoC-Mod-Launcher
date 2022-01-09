using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.ViewModels.Designer;

internal class MainPageViewModel : IMainPageViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsLoading { get; }
    public string? LoadingText { get; }

    public Task InitializeAsync()
    {
        throw new System.NotImplementedException();
    }
}

internal class ImageDialogViewModel : ViewModels.IImageDialogViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public WindowState MinMaxState { get; set; }
    public bool RightToLeft { get; set; }
    public string? Title { get; set; }
    public bool IsFullScreen { get; set; }
    public bool IsResizable { get; set; }
    public bool HasMinimizeButton { get; set; }
    public bool HasMaximizeButton { get; set; }
    public bool IsGripVisible { get; set; }
    public event EventHandler? CloseDialogRequest;
    public string? ResultButton { get; }
    public IList<IButtonViewModel> Buttons { get; }
    public void CloseDialog()
    {
        throw new NotImplementedException();
    }

    public ImageMoniker Image => Monikers.Settings;
}

internal class ErrorMessageDialogViewModel : IErrorMessageDialogViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? CloseDialogRequest;

    public WindowState MinMaxState { get; set; }
    public bool RightToLeft { get; set; }
    public string? Title { get; set; }
    public bool IsFullScreen { get; set; }
    public bool IsResizable { get; set; }
    public bool HasMinimizeButton { get; set; }
    public bool HasMaximizeButton { get; set; }
    public bool IsGripVisible { get; set; }
    public string? ResultButton { get; }
    public IList<IButtonViewModel> Buttons { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public void CloseDialog()
    {
    }

    public string Header => "This is a test message for the error dialog.";
    public string Message => "This is a very long message, which may represent an exception message containing a lot of details, end-users don't under stand anyway. However this is better than a hexed error code.";
    public ImageMoniker Image => Monikers.Settings;
}

