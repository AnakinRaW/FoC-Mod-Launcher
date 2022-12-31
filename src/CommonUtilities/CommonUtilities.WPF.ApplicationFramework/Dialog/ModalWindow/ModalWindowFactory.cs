using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public class ModalWindowFactory : IModalWindowFactory
{
    private readonly IWindowService _windowService;

    public ModalWindowFactory(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _windowService = serviceProvider.GetRequiredService<IWindowService>();
    }

    public ModalWindow Create(IModalWindowViewModel viewModel)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            _windowService.ShowWindow();
            var window = CreateWindow(viewModel);
            window.Content = viewModel;
            _windowService.SetOwner(window);
            return window;
        });
    }

    protected virtual ModalWindow CreateWindow(IModalWindowViewModel viewModel)
    {
        return new ModalWindow(viewModel);
    }
}