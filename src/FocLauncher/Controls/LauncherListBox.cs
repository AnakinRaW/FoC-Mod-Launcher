using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FocLauncher.Controls
{
    class LauncherListBox : ListBox
    {
        private object _pendingFocusTarget;
        private WeakReference _lastFocusedItem;
        internal bool IsDirectlyGainingKeyboardFocus { get; set; }

        static LauncherListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LauncherListBox), new FrameworkPropertyMetadata(typeof(LauncherListBox)));
        }

        public LauncherListBox()
        {
            AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
            SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
        }

        // TODO OnContextMenuOpening

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsKeyboardFocusWithin || Mouse.Captured != null || e.ClickCount != 1)
                return;
            Focus();
        }

        public void SelectFirstItem()
        {
            if (Items.Count <= 0)
                return;
            SelectedItem = Items[0];
        }

        public void FocusSelectedItem()
        {
            if (SelectedItem == null)
                SelectFirstItem();
            if (SelectedItem == null)
                return;
            FocusTarget(SelectedItem);
        }

        public ISet<TItem> GetDistinctSelection<TItem>() where TItem : class
        {
            var objSet = new HashSet<TItem>();
            foreach (var selectedItem in SelectedItems)
            {
                if (GetTreeNodeKeyFromItem(selectedItem) is TItem treeNodeKeyFromItem)
                    objSet.Add(treeNodeKeyFromItem);
            }
            return objSet;
        }

        protected virtual object GetTreeNodeKeyFromItem(object item)
        {
            return item;
        }


        protected bool FocusTarget(object selectedItem)
        {
            if (!IsDirectlyGainingKeyboardFocus)
                ScrollIntoView(selectedItem);
            if (ItemContainerGenerator.ContainerFromItem(selectedItem) is IInputElement inputElement)
            {
                inputElement.Focus();
                _pendingFocusTarget = null;
                return true;
            }
            _pendingFocusTarget = selectedItem;
            return false;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is LauncherListBoxItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new LauncherListBoxItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (item != _pendingFocusTarget)
                return;
            _pendingFocusTarget = null;
            if (!(element is IInputElement inputElement))
                return;
            var previousFocus = Keyboard.FocusedElement;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (Keyboard.FocusedElement != previousFocus)
                    return;
                inputElement.Focus();
            }), DispatcherPriority.Normal);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus == this)
            {
                IsDirectlyGainingKeyboardFocus = true;

                try
                {
                    FocusDefaultItem();
                }
                finally
                {
                    IsDirectlyGainingKeyboardFocus = false;
                }
            }
            else
                _lastFocusedItem = new WeakReference(ItemContainerGenerator.ItemFromContainer((DependencyObject) e.NewFocus));

            base.OnGotKeyboardFocus(e);
        }

        private void FocusDefaultItem()
        {
            if (_lastFocusedItem?.Target is IInputElement inputElement &&
                (!(inputElement is DependencyObject dependencyObject) ||
                 PresentationSource.FromDependencyObject(dependencyObject) != null))
                inputElement.Focus();
            else
                FocusSelectedItem();
        }
    }
}
