using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;

public interface IViewModelPresenter
{
    event EventHandler<IViewModel>? ViewRequested;

    void ShowViewModel(IViewModel viewModel);
}