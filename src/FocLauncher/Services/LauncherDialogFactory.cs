using System;
using FocLauncher.Controls;
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
        return new ImageDialog(viewModel);
    }
}