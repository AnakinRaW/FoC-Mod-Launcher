﻿using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FocLauncher.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Controls.ViewModels;
using Validation;

namespace FocLauncher.ViewModels;

public partial class UnhandledExceptionDialogViewModel : ObservableObject, IUnhandledExceptionDialogViewModel
{
    [ObservableProperty]
    private Exception _exception;

    public string WindowCaption => LauncherEnvironment.LauncherProgramName;

    public string Header => "Oh no, something went wrong!";

    public string HandlerDescription => "If the problem keeps appearing you can file an issue on GitHub.com";

    public ICommand Handler => new RelayCommand(OnCreateIssue);
    public ImageSource? HandlerIcon => new BitmapImage(ImageCatalog.GithubDefinition.Source);
    
    public string HandlerName => "Create _Issue...";

    public UnhandledExceptionDialogViewModel(Exception exception)
    {
        Requires.NotNull(exception, nameof(exception));
        _exception = exception;
    }

    private static void OnCreateIssue()
    {
        Process.Start("https://github.com/AnakinSklavenwalker/FoC-Mod-Launcher/issues");
    }
}