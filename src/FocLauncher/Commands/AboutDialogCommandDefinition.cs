using System;
using System.Windows.Input;
using FocLauncher.Imaging;
using FocLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Input;

namespace FocLauncher.Commands;

public class AboutDialogCommandDefinition : ICommandDefinition
{
    private readonly IServiceProvider _serviceProvider;
    public string Text => "About FoC Launcher";
    public string? Tooltip => null;
    public ImageKey Image => ImageKeys.StatusHelpIcon;
    public ICommand Command { get; }

    public AboutDialogCommandDefinition(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Command = new DelegateCommand(OpenAboutDialog);
    }

    private void OpenAboutDialog()
    {
        _serviceProvider.GetRequiredService<IQueuedDialogService>().ShowDialog(new LauncherAboutDialogViewModel(_serviceProvider));
    }
}