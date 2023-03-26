using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Input;
using FocLauncher.Imaging;

namespace FocLauncher.Commands;

internal class TestCommand : CommandDefinition
{
    public override ImageKey Image => ImageKeys.AppIcon;
    public override string Text => "Test";
    public override ICommand Command => new DelegateCommand(() => {});
    public override string Tooltip => "Tooltip";
}