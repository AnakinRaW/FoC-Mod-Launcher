using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;

public class ViewModelPresenterService : IViewModelPresenter
{
    public event EventHandler<IViewModel>? ViewRequested;

    public void ShowViewModel(IViewModel viewModel)
    {
        ViewRequested?.Invoke(this, viewModel);
    }
}