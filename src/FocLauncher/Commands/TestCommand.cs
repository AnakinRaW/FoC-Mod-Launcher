using System.Windows.Input;
using FocLauncher.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Input;

namespace FocLauncher.Commands;

internal class TestCommand : CommandDefinition
{
    public override ImageKey Image => ImageKeys.Undo;
    public override string Text => "Test";
    public override ICommand Command => new DelegateCommand(() => {});
    public override string Tooltip => "Tooltip";
}