using FocLauncher.Game;
using FocLauncher.Mods;
using FocLauncher.Threading;
using FocLauncher.Utilities;

namespace FocLauncher.Items
{
    internal class LauncherItemEventSink : DisposableObject
    {
        public LauncherItemManager Manager { get; }

        public IPetroglyhGameableObject GameObject { get; }

        public LauncherItemEventSink(LauncherItemManager manager, IPetroglyhGameableObject gameObject)
        {
            Manager = manager;
            GameObject = gameObject;
            if (!(gameObject is IModContainer modContainer))
                return;
            modContainer.ModCollectionModified += OnModsChanged;
        }

        protected override void DisposeManagedResources()
        {
            if (GameObject is IModContainer modContainer)
                modContainer.ModCollectionModified -= OnModsChanged;
        }

        private void OnModsChanged(object sender, ModCollectionChangedEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                OnModsChanged(e);
            });
        }

        private void OnModsChanged(ModCollectionChangedEventArgs e)
        {
            using (Manager.BeginChangingItems())
            {
                if (e.Action == ModCollectionChangedAction.Add)
                    OnModAdded(e.Mod);
                else if (e.Action == ModCollectionChangedAction.Remove)
                    OnModRemoved(e.Mod);
            }
        }

        private void OnModAdded(IMod mod)
        {
            Manager.RaiseOnItemAdded(Manager.GetGameObjectItem(mod));
        }

        private void OnModRemoved(IMod eMod)
        {
        }
    }
}