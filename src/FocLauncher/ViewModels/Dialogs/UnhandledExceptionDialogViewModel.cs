using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using AnakinRaW.CommonUtilities.Wpf.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FocLauncher.Imaging;
using Validation;

namespace FocLauncher.ViewModels.Dialogs;

public partial class UnhandledExceptionDialogViewModel : ModalWindowViewModel, IUnhandledExceptionDialogViewModel
{
    [ObservableProperty]
    private Exception _exception = null!;

    public string WindowCaption => LauncherEnvironment.LauncherProgramName;

    public string Header => "Oh no, something went wrong!";

    public string HandlerDescription => "If the problem keeps appearing you can file an issue on GitHub.com";

    public ICommand Handler => new DelegateCommand(OnCreateIssue);

    public ImageSource? HandlerIcon => new BitmapImage(ImageCatalog.GithubDefinition.Source);
    
    public string HandlerName => "Create _Issue...";

    public UnhandledExceptionDialogViewModel(Exception exception)
    {
        Requires.NotNull(exception, nameof(exception));
        Exception = exception;
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        IsResizable = false;
        IsCloseButtonEnabled = false;
    }

    private static void OnCreateIssue()
    {
        Process.Start("https://github.com/AnakinRaW/FoC-Mod-Launcher/issues");
    }
}