using System;
using FocLauncher.Game;

namespace FocLauncher.Items
{
    internal interface ILauncherItemManager
    {
        event EventHandler<LauncherItemEventArgs>? OnItemAdded;

        bool IsChangingItems { get; }

        ILauncherItem GetGameObjectItem(IPetroglyhGameableObject gameObject);

        bool TryGetItem(IPetroglyhGameableObject gameObject, out ILauncherItem? item);
    }
}