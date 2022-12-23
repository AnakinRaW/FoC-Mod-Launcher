using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FocLauncher.Controls.Controllers;
using FocLauncher.NativeMethods;
using FocLauncher.Utilities;

namespace FocLauncher.Controls
{
    internal class LauncherListBox : ListBox
    {
        private static readonly DependencyPropertyKey IsContextMenuOpenPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsContextMenuOpen", typeof(bool), typeof(LauncherListBox),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsContextMenuOpenProperty = IsContextMenuOpenPropertyKey.DependencyProperty;

        private object _pendingFocusTarget;
        private WeakReference _lastFocusedItem;
        internal bool IsDirectlyGainingKeyboardFocus { get; set; }

        public static bool GetIsContextMenuOpen(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (bool)element.GetValue(IsContextMenuOpenProperty);
        }

        private static void SetIsContextMenuOpen(UIElement element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(IsContextMenuOpenPropertyKey, value);
        }

        static LauncherListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LauncherListBox), new FrameworkPropertyMetadata(typeof(LauncherListBox)));
        }

        public LauncherListBox()
        {
            AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
            SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
        }
        
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (e.OriginalSource is DependencyObject originalSource && DescendantHasContextMenu(originalSource))
                return;
            using (EnterContextMenuVisualState())
            {
                var contextMenuItems = GetDistinctSelection<object>().OfType<IHasContextMenuController>();
                e.Handled = ContextMenuController.ShowContextMenu(contextMenuItems, GetContextMenuLocation());
            }
        }

        private Point GetContextMenuLocation()
        {
            if (InputManager.Current.MostRecentInputDevice is KeyboardDevice && Keyboard.FocusedElement is UIElement focusedElement)
                return focusedElement.PointToScreen(new Point(0.0, focusedElement.RenderSize.Height));
            var messagePos = User32.GetMessagePos();
            return this.DeviceToLogicalPoint(new Point(NativeMethods.NativeMethods.SignedLow(messagePos), NativeMethods.NativeMethods.SignedHigh(messagePos)));
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

        public IDisposable EnterContextMenuVisualState()
        {
            return new ContextMenuScope(this);
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

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsKeyboardFocusWithin || Mouse.Captured != null || e.ClickCount != 1)
                return;
            Focus();
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

        private bool DescendantHasContextMenu(DependencyObject descendant)
        {
            return descendant.FindAncestorOrSelf(GetParentWithinSubtree, ContextMenuIsEnabled) != null;
        }

        private DependencyObject GetParentWithinSubtree(DependencyObject o)
        {
            return o == this ? null : o.GetVisualOrLogicalParent();
        }

        private static bool ContextMenuIsEnabled(DependencyObject o)
        {
            if (ContextMenuService.GetContextMenu(o) == null || !ContextMenuService.GetIsEnabled(o))
                return false;
            return IsElementEnabled(o) || ContextMenuService.GetShowOnDisabled(o);
        }

        private static bool IsElementEnabled(DependencyObject o)
        {
            switch (o)
            {
                case UIElement uiElement:
                    return uiElement.IsEnabled;
                case ContentElement contentElement:
                    return contentElement.IsEnabled;
                case UIElement3D uiElement3D:
                    return uiElement3D.IsEnabled;
                default:
                    return true;
            }
        }

        private class ContextMenuScope : IDisposable
        {
            private readonly LauncherListBox _view;

            private bool IsDisposed { get; set; }

            public ContextMenuScope(LauncherListBox view)
            {
                _view = view;
                SetIsContextMenuOpen(view, true);
            }

            ~ContextMenuScope()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!IsDisposed)
                {
                    if (disposing)
                        SetIsContextMenuOpen(_view, false);
                    IsDisposed = true;
                }
            }
        }
    }
}
