using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public abstract partial class LoadingViewModelBase : ViewModelBase, ILoadingViewModel
{
    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _loadingText;

    protected LoadingViewModelBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}