using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using FocLauncher.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
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