using System.ComponentModel;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace FocLauncher.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class StatusBarViewModel : ILauncherStatusBarViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public bool IsVisible { get; set; } = true;
    public ICommandDefinition AboutCommand { get; }
    public ICommandDefinition SettingsCommand { get; }
    public ICommandDefinition UpdateCommand { get; }
    public Brush Background { get; set; } = Brushes.DarkGray;
    public string InstalledGameType { get; set; } = "Installed Game: Steam";
}