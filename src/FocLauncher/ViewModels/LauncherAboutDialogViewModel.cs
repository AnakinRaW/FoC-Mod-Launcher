using System;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace FocLauncher.ViewModels;

public class LauncherAboutDialogViewModel : DialogViewModel
{
    public LauncherAboutDialogViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Title = "About Foc Launcher";
        IsResizable = false;
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        HasDialogFrame = true;
    }

}