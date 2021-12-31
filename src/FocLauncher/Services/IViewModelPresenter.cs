using System;
using FocLauncher.ViewModels;

namespace FocLauncher.Services;

internal interface IViewModelPresenter
{
    event EventHandler<ILauncherViewModel>? ViewRequested;

    void ShowViewModel(ILauncherViewModel viewModel);
}