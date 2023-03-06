using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;
using FocLauncher.ViewModels.Dialogs;

namespace FocLauncher.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class ImageDialogViewModel : IImageDialogViewModel
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
    public bool ShowIcon { get; set; }
    public bool HasDialogFrame { get; set; }
    public bool IsCloseButtonEnabled { get; set; }
    public event EventHandler? CloseDialogRequest;
    public string? ResultButton { get; }
    public IList<IButtonViewModel> Buttons { get; }
    public IDialogAdditionalInformationViewModel? AdditionalInformation { get; }

    public void CloseDialog()
    {
    }

    public void OnClosing(CancelEventArgs e)
    {
    }

    public ImageKey Image => ImageKeys.Settings;
}