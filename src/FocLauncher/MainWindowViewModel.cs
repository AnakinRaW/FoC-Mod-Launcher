using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
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
            var args = new GameRunArguments(DataModel.SelectedMod)
            {
                UseDebug = DataModel.UseDebugBuild,
                IgnoreAsserts = DataModel.IgnoreAsserts,
                NoArtProcess = DataModel.NoArtProcess
            };
            var started = DataModel.FoC.PlayGame(args);
            if (!started)
            {
                MessageBox.Show("Game did not start.", "FoC Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Settings.Default.AutoSwitchTheme &&
                ThemeManager.Instance.TryGetThemeByMod(DataModel.SelectedMod, out var theme))
                ThemeManager.Instance.Theme = theme;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
