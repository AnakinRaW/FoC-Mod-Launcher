using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using FocLauncher.AssemblyHelper;
using FocLauncher.Theming;

namespace FocLauncher
{
    public partial class App
    {
        static App()
        {
            AssemblyLoader.LoadAssemblies();
        }

        private void OnStartUp(object sender, StartupEventArgs e)
        {
            var tm = new ThemeManager();


            var assembly = Assembly.LoadFrom("FocLauncher.Theme.dll");

            var type = assembly.GetType("FocLauncher.Theme.LauncherTheme");

            var theme = (ITheme) Activator.CreateInstance(type);

            ResourceDictionary resources = Current.Resources;
            resources.Clear();

            resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = theme.GetResourceUri()
            });



            var mainWindow = new MainWindow();
            var viewModel = new MainWindowViewModel(new LauncherDataModel());

            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }
    }
}
