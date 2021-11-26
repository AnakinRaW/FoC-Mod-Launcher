using System;

namespace FocLauncher
{
    internal static class LauncherProgram
    {
        [STAThread]
        private static void Main()
        {
            var window = new MainWindow();
            var app = new LauncherApplication();
            app.Run(window);
        }
    }
}
