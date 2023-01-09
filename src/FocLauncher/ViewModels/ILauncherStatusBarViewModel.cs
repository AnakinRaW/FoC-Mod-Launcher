using System.Windows.Media;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

namespace FocLauncher.ViewModels;

internal interface ILauncherStatusBarViewModel : IStatusBarViewModel
{
    ICommandDefinition AboutCommand { get; }
    ICommandDefinition SettingsCommand { get; }
    ICommandDefinition UpdateCommand { get; }

    Brush Background { get; set; }

    string InstalledGameType { get; set; }
}