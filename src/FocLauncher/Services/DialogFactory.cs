using System;
using System.Windows;
using FocLauncher.Controls;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Validation;

namespace FocLauncher.Services;

internal class DialogFactory : IDialogFactory
{
    private readonly IWindowService _windowService;

    public DialogFactory(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _windowService = serviceProvider.GetRequiredService<IWindowService>();
    }

    public ModalWindow Create(IDialogViewModel viewModel)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            _windowService.ShowWindow();
            var dialog = new ImageDialog(viewModel);
            _windowService.SetOwner(dialog);
            return dialog;
        });
    }
}