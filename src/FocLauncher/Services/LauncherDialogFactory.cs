using System;
using FocLauncher.Controls;
using FocLauncher.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

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