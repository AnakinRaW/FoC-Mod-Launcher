using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using FocLauncher.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Commands;

internal class TestCommand : CommandDefinition
{
    public override ImageKey Image => ImageKeys.Undo;
    public override string Text => "Test";
    public override ICommand Command => new RelayCommand(() => { });
}