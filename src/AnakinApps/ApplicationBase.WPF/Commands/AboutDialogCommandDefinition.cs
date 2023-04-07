using System;
using System.Windows.Input;
using AnakinRaW.ApplicationBase.Imaging;
using AnakinRaW.ApplicationBase.ViewModels.Dialogs;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ApplicationBase.Commands;

public class AboutDialogCommandDefinition : ICommandDefinition
{
    private readonly IServiceProvider _serviceProvider;
    public string Text { get; }
    public string? Tooltip => null;
    public ImageKey Image => ImageKeys.StatusHelpIcon;
    public ICommand Command { get; }

    public AboutDialogCommandDefinition(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Command = new DelegateCommand(OpenAboutDialog);
        var appName = serviceProvider.GetRequiredService<IApplicationEnvironment>().ApplicationName;
        Text = $"About {appName}";
    }

    private void OpenAboutDialog()
    {
        _serviceProvider.GetRequiredService<IQueuedDialogService>().ShowDialog(new ApplicationAboutDialogViewModel(_serviceProvider));
    }
}