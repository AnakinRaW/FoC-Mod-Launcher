using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FocLauncher.ViewModels;

public partial class MainPageViewModel : ObservableObject, IMainPageViewModel
{
    private readonly IServiceProvider _serviceProvider;
    
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _loadingText;

    public MainPageViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}