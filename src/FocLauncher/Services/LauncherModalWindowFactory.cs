using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using FocLauncher.Controls;

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