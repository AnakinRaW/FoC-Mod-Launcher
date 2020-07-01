using System;
using FocLauncher.Game;

namespace FocLauncher.Items
{
    internal interface ILauncherItemManager
    {
        ILauncherItem GetGameObjectItem(IPetroglyhGameableObject gameObject, uint itemid);

        bool TryGetHierarchyItem(IPetroglyhGameableObject gameObject, uint itemid, out ILauncherItem item);

        bool IsChangingItems { get; }

        event EventHandler<LauncherItemEventArgs> AfterInvalidateItems;

        event EventHandler<LauncherItemEventArgs> OnItemAdded;
    }
}