using System;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ApplicationBase.Commands;

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