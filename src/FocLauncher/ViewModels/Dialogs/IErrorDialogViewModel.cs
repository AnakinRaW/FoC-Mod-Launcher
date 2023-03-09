﻿using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.ViewModels.Dialogs;

public interface IErrorMessageDialogViewModel : IViewModel, IImageDialogViewModel
{
    string Header { get; }

    string Message { get; }
}