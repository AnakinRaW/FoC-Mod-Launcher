﻿using System;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework;

public class ViewPresenterService : IViewModelPresenter
{
    public event EventHandler<IViewModel>? ViewRequested;

    public void ShowViewModel(IViewModel viewModel)
    {
        ViewRequested?.Invoke(this, viewModel);
    }
}