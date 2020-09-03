using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using FocLauncher.Game;
using FocLauncher.Items;
using FocLauncher.Threading;
using FocLauncher.Utilities;

namespace FocLauncher.Controls
{
    public sealed class LauncherListBoxPane : ContentControl
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(LauncherItem), typeof(LauncherListBoxPane), new PropertyMetadata(default(LauncherItem)));

        public LauncherItem SelectedItem
        {
            get => (LauncherItem) GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }


        private bool _initializedItemManagerEvents;


        private readonly ObservableCollection<ILauncherItem> _itemCollection = new ObservableCollection<ILauncherItem>();

        public ReadOnlyObservableCollection<ILauncherItem> ItemCollection { get; }

        internal LauncherListBox ListBox { get; }


        private LauncherItemManager ItemManager => LauncherItemManager.Instance;

        public LauncherListBoxPane()
        {
            UseLayoutRounding = false;
            Focusable = true;
            FocusVisualStyle = null;
            ListBox = new LauncherListBox();
            ItemCollection = new ReadOnlyObservableCollection<ILauncherItem>(_itemCollection);
            CreateBindings();
        }


        private void CreateBindings()
        {
            BindingOperations.SetBinding(ListBox, ItemsControl.ItemsSourceProperty,
                new Binding(nameof(ItemCollection)) { Source = this });

            BindingOperations.SetBinding(this, SelectedItemProperty,
                new Binding(nameof(ListBox.SelectedItem)) {Source = ListBox});
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitializeItemManagerEvents();
            Content = ListBox;
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            if (e.OriginalSource != this || !(Content is UIElement content))
                return;
            FocusHelper.MoveFocusInto(content);
        }

        public bool AddGame(IGame game, bool changeSelection = true)
        {
            var result = false;
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                result = await AddGameAsync(game, changeSelection);
            });
            return result;
        }

        public async Task<bool> AddGameAsync(IGame game, bool changeSelection = true)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var item = ItemManager.GetGameObjectItem(game);
            var position = GetGamePosition();

            _itemCollection.Insert(position, item);

            if (changeSelection)
                ListBox.SelectedItem = item;
            return true;
        }

        private int GetGamePosition()
        {
            return ItemCollection.Count;
        }

        private void InitializeItemManagerEvents()
        {
            if (!_initializedItemManagerEvents)
            {
                _initializedItemManagerEvents = true;
                var itemManager = ItemManager;
                itemManager.AfterInvalidateItems += OnAfterInvalidateItems;
                itemManager.OnItemAdded += OnItemAdded;
            }
        }

        private void OnItemAdded(object sender, LauncherItemEventArgs e)
        {
            _itemCollection.Add(e.Item);
        }

        private void OnAfterInvalidateItems(object sender, LauncherItemEventArgs e)
        {
            
        }
    }
}