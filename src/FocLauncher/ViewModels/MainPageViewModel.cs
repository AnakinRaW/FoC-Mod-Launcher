using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FocLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace FocLauncher.ViewModels;

internal partial class MainPageViewModel : LoadingViewModelBase, IMainPageViewModel
{
    private bool _isInitialized;

    [ObservableProperty]
    private object? _selectedGameObject;

    public ObservableCollection<object> Games { get; }

    public IGameArgumentsViewModel ArgumentsViewModel { get; }

    public MainPageViewModel(IGameArgumentsViewModel argumentsViewModel, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Requires.NotNull(argumentsViewModel, nameof(argumentsViewModel));
        ArgumentsViewModel = argumentsViewModel;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(SelectedGameObject))
            ArgumentsViewModel.CurrentGameObject = SelectedGameObject;
    }

    public override Task InitializeAsync()
    {
        return Task.Run(async () =>
        {
            try
            {
                if (_isInitialized)
                    return;
                lock (this)
                {
                    if (_isInitialized)
                        return;
                    _isInitialized = true;
                }

                //var vm = new UpdateWindowViewModel();
                //await ServiceProvider.GetRequiredService<IModalWindowService>().ShowModal(vm);

                //await Application.Current.Dispatcher.InvokeAsync(() =>
                //{
                //    new UpdateWindow(ServiceProvider)
                //    {
                //        Owner = Application.Current.MainWindow
                //    }.ShowDialog();
                //});

                //await Task.Delay(2000);

                IsLoading = false;
            }
            finally
            {
                ServiceProvider.GetRequiredService<IWindowService>().ShowWindow();
            }
        });
    }
}