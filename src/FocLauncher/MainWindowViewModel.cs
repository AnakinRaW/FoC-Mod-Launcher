using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FocLauncher.Annotations;
using FocLauncher.Game;
using FocLauncher.Input;
using FocLauncher.Properties;
using FocLauncher.Theming;

namespace FocLauncher
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ICommand LaunchCommand => new UICommand(ExecutedLaunch, () => true);

        public IDataModel DataModel { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(IDataModel dataModel)
        {
            DataModel = dataModel;
        }

        private void ExecutedLaunch()
        {
            var debugOptions = new DebugOptions(DataModel.UseDebugBuild, DataModel.IgnoreAsserts, DataModel.NoArtProcess);
            DataModel.FoC.PlayGame(DataModel.SelectedMod, debugOptions);
            if (Settings.Default.AutoSwitchTheme &&
                ThemeManager.Instance.TryGetThemeByMod(DataModel.SelectedMod, out var theme))
                ThemeManager.Instance.Theme = theme;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
