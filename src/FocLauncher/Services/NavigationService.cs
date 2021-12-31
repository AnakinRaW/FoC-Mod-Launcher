using System;
using FocLauncher.ViewModels;

namespace FocLauncher.Services;

internal class ViewPresenterService : IViewModelPresenter
{
    public event EventHandler<ILauncherViewModel>? ViewRequested;

    public void ShowViewModel(ILauncherViewModel viewModel)
    {
        ViewRequested?.Invoke(this, viewModel);
    }
}