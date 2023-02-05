using System;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Input;

namespace FocLauncher.Update.Commands;

internal class RestartCommand : CommandDefinition
{
    public override ImageKey Image => default;
    public override string Text => "Restart";
    public override ICommand Command => new DelegateCommand(() => { });
    public override string? Tooltip => null;

    public RestartCommand(IServiceProvider serviceProvider)
    {
    }
}