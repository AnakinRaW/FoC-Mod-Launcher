using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FocLauncher.Game;
using FocLauncher.Input;
using FocLauncher.Mods;
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
            GameRunArguments args;
            if (DataModel.SelectedMod is IMod mod)
                args = new GameRunArguments(mod);
            else
                args = new GameRunArguments();

            args.UseDebug = DataModel.UseDebugBuild;
            args.IgnoreAsserts = DataModel.IgnoreAsserts;
            args.NoArtProcess = DataModel.NoArtProcess;
            args.Windowed = DataModel.Windowed;


            var started = DataModel.FoC.PlayGame(args, DataModel.SelectedMod.IconFile);
            if (!started)
            {
                MessageBox.Show("Game did not start.", "FoC Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Settings.Default.AutoSwitchTheme &&
                ThemeManager.Instance.TryGetThemeByMod(DataModel.SelectedMod as IMod, out var theme))
                ThemeManager.Instance.Theme = theme;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
