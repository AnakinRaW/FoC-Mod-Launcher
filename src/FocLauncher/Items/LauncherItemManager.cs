using System;
using System.Collections.Generic;
using FocLauncher.Game;
using FocLauncher.Utilities;

namespace FocLauncher.Items
{
    public class LauncherItemManager : ILauncherItemManager
    {
        public event EventHandler<LauncherItemEventArgs>? OnItemAdded;

        private readonly Dictionary<IPetroglyhGameableObject, LauncherItem> _items = new Dictionary<IPetroglyhGameableObject, LauncherItem>();
        private readonly Dictionary<IPetroglyhGameableObject, LauncherItemEventSink> _events = new Dictionary<IPetroglyhGameableObject, LauncherItemEventSink>();

        private static LauncherItemManager? _instance;
        private int _itemChangeScopeCount;

        public static LauncherItemManager Instance => _instance ??= new LauncherItemManager(); 
        
        public bool IsChangingItems => _itemChangeScopeCount > 0;

        public ILauncherItem GetGameObjectItem(IPetroglyhGameableObject gameObject)
        {
            return GetItemImpl(gameObject);
        }

        public bool TryGetItem(IPetroglyhGameableObject gameObject, out ILauncherItem? item)
        {
            item = TryGetItem(gameObject);
            return item != null;
        }

        public LauncherItem? TryGetItem(IPetroglyhGameableObject gameableObject)
        {
            return _items.TryGetValue(gameableObject, out var item) ? item : null;
        }

        internal LauncherItem GetItemImpl(IPetroglyhGameableObject gameObject)
        {
            var item = TryGetItem(gameObject);
            if (item == null)
            {
                item = new LauncherItem(this, gameObject);
                AddItem(item);
            }

            return item;
        }

        internal IDisposable BeginChangingItems()
        {
            return new ItemChangeScope(this);
        }

        private void AddItem(LauncherItem item)
        {
            _items.Add(item.GameObject, item);
            RegisterEventSinks(item);
        }

        private void RegisterEventSinks(LauncherItem item)
        {
            if (!_events.TryGetValue(item.GameObject, out var eventSinks))
            {
                eventSinks = new LauncherItemEventSink(this, item.GameObject);
                _events.Add(item.GameObject, eventSinks);
            }
        }

        private class ItemChangeScope : DisposableObject
        {
            private readonly LauncherItemManager _manager;

            public ItemChangeScope(LauncherItemManager manager)
            {
                _manager = manager;
                ++_manager._itemChangeScopeCount;
            }

            protected override void DisposeManagedResources() => --_manager._itemChangeScopeCount;
        }

        public void RaiseOnItemAdded(ILauncherItem item)
        {
            OnItemAdded?.Invoke(this, new LauncherItemEventArgs(item));
        }
    }
}
