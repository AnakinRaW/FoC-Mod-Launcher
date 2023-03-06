using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace FocLauncher.ViewModels.Dialogs;

public class LauncherAboutDialogViewModel : DialogViewModel
{
    public override IDialogAdditionalInformationViewModel? AdditionalInformation { get; } =
        new LauncherVersionViewModel();

    public LauncherAboutDialogViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Title = "About Foc Launcher";
        IsResizable = false;
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        HasDialogFrame = true;
    }

}