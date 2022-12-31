using System;
using FocLauncher.Controls;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace FocLauncher.Services;

internal class LauncherModalWindowFactory : ModalWindowFactory
{
    public LauncherModalWindowFactory(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override ModalWindow CreateWindow(IModalWindowViewModel viewModel)
    {
        return new ThemedModalWindow(viewModel);
    }
}