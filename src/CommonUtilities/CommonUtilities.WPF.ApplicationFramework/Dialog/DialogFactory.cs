using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public class DialogFactory : IDialogFactory
{
    private readonly IWindowService _windowService;

    public DialogFactory(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _windowService = serviceProvider.GetRequiredService<IWindowService>();
    }

    public DialogWindow Create(IDialogViewModel viewModel)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            _windowService.ShowWindow();
            var dialog = CreateDialog(viewModel);
            _windowService.SetOwner(dialog);
            return dialog;
        });
    }

    protected virtual DialogWindow CreateDialog(IDialogViewModel viewModel)
    {
        return new DialogWindow(viewModel);
    }
}