using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater.Tasks
{
    internal class FileInstaller
    {
        private readonly IComponent _component;

        public FileInstaller(IComponent component)
        {
            _component = component;
        }
    }
}