using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

namespace FocLauncher.ViewModels;

internal interface ILauncherStatusBarViewModel : IStatusBarViewModel
{
    ICommandDefinition AboutCommand { get; }
    ICommandDefinition SettingsCommand { get; }
    ICommandDefinition UpdateCommand { get; }

    Brush Background { get; set; }

    string InstalledGameType { get; set; }
}