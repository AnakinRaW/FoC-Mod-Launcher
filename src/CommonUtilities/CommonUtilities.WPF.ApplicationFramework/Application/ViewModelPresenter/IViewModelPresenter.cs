using System;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework;

public interface IViewModelPresenter
{
    event EventHandler<IViewModel>? ViewRequested;

    void ShowViewModel(IViewModel viewModel);
}