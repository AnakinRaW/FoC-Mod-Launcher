using System;
using System.Windows.Input;
using FocLauncher.Imaging;
using FocLauncher.Update.ViewModels;
using FocLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Input;

namespace FocLauncher.Commands;

public class UpdateWindowCommandDefinition : ICommandDefinition
{
    private readonly IServiceProvider _serviceProvider;
    public string Text => "Update FoC Launcher";
    public string? Tooltip => null;
    public ImageKey Image => ImageKeys.UpdateIcon;
    public ICommand Command { get; }

    private IUpdateWindowViewModel? _updateWindowViewModel;

    private IUpdateWindowViewModel UpdateWindowViewModel
    {
        get { return _updateWindowViewModel ??= new UpdateWindowViewModel(_serviceProvider); }

    }

    public UpdateWindowCommandDefinition(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Command = new DelegateCommand(OpenAboutDialog);
    }

    private void OpenAboutDialog()
    {
        _serviceProvider.GetRequiredService<IModalWindowService>().ShowModal(UpdateWindowViewModel);
    }
}