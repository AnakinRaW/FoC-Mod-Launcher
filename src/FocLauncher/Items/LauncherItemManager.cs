using System;
using FocLauncher.Game;

namespace FocLauncher.Items
{
    public class LauncherItemManager : ILauncherItemManager
    {
        public event EventHandler<LauncherItemEventArgs> AfterInvalidateItems;
        public event EventHandler<LauncherItemEventArgs> OnItemAdded;

        public bool IsChangingItems { get; }

        public ILauncherItem GetGameObjectItem(IPetroglyhGameableObject gameObject, uint itemid)
        {
            throw new NotImplementedException();
        }

        public bool TryGetHierarchyItem(IPetroglyhGameableObject gameObject, uint itemid, out ILauncherItem item)
        {
            throw new NotImplementedException();
        }
    }
}
