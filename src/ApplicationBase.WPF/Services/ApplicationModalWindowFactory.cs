using System;
using AnakinRaW.ApplicationBase.Controls;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace AnakinRaW.ApplicationBase.Services;

internal class ApplicationModalWindowFactory : ModalWindowFactory
{
    public ApplicationModalWindowFactory(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override ModalWindow CreateWindow(IModalWindowViewModel viewModel)
    {
        return new ThemedModalWindow(viewModel);
    }
}