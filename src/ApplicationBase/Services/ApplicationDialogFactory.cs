using System;
using AnakinRaW.ApplicationBase.Controls;
using AnakinRaW.ApplicationBase.ViewModels.Dialogs;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Controls;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace AnakinRaW.ApplicationBase.Services;

internal class ApplicationDialogFactory : DialogFactory
{
    public ApplicationDialogFactory(IServiceProvider serviceProvider) : base(serviceProvider)
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