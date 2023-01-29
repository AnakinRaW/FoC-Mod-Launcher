using System;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Input;
using FocLauncher.Imaging;
using FocLauncher.Update.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Commands;

public class UpdateWindowCommandDefinition : ICommandDefinition
{
    private readonly IServiceProvider _serviceProvider;
    public string Text => "Update FoC Launcher";
    public string? Tooltip => null;
    public ImageKey Image => ImageKeys.UpdateIcon;
    public ICommand Command { get; }
    
    public UpdateWindowCommandDefinition(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Command = new DelegateCommand(OpenAboutDialog);
    }

    private async void OpenAboutDialog()
    {
        // Singletone instance of this view model drastically increases closing/cancellation complexity.
        // Creating a new model for each request should be good enough. 
        var viewModel = new UpdateWindowViewModel(_serviceProvider);
        await _serviceProvider.GetRequiredService<IModalWindowService>().ShowModal(viewModel);
    }
}