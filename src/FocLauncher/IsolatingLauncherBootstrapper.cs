using System;

namespace FocLauncher
{
    public class IsolatingLauncherBootstrapper : MarshalByRefObject
    {
        public void StartLauncherApplication()
        {
            var app = new LauncherApp();
            app.Run();
        }
    }
}