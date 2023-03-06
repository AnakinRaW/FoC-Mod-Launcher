using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Controls;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using FocLauncher.Controls;
using FocLauncher.ViewModels.Dialogs;

namespace FocLauncher.Services;

internal class LauncherDialogFactory : DialogFactory
{
    public LauncherDialogFactory(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override DialogWindow CreateDialog(IDialogViewModel viewModel)
    {
        if (viewModel is IImageDialogViewModel)
            return new ImageDialog(viewModel);
        var dialog = new DialogWindow(viewModel)
        {
            Content = viewModel
        };
        return dialog;
    }
}