using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FocLauncher.Utilities;

namespace FocLauncher.Controls
{
    class LauncherListBoxItem : ListBoxItem
    {
        public static readonly RoutedEvent InvokedEvent;
        public static readonly RoutedEvent InvokeFailedEvent;

        protected LauncherListBox ParentListBox => this.FindAncestor<LauncherListBox>();

        public event RoutedEventHandler Invoked
        {
            add => AddHandler(InvokedEvent, value);
            remove => RemoveHandler(InvokedEvent, value);
        }

        public event RoutedEventHandler InvokeFailed
        {
            add => AddHandler(InvokeFailedEvent, value);
            remove => RemoveHandler(InvokeFailedEvent, value);
        }

        static LauncherListBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LauncherListBoxItem), new FrameworkPropertyMetadata(typeof(LauncherListBoxItem)));
            InvokedEvent = EventManager.RegisterRoutedEvent("Invoked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LauncherListBoxItem));
            InvokeFailedEvent = EventManager.RegisterRoutedEvent("InvokeFailed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LauncherListBoxItem));
        }

        public LauncherListBoxItem()
        {
            Focusable = true;
            AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
        }

        public bool InvokeItem()
        {
            if (!IsSelected)
                return false;

            var invocableItems = ParentListBox.GetDistinctSelection<object>().OfType<IHasInvocationController>();
            if (InvocationController.Invoke(invocableItems))
            {
                RaiseEvent(new RoutedEventArgs(InvokedEvent, this));
                return true;
            }
            RaiseEvent(new RoutedEventArgs(InvokeFailedEvent, this));
            return false;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            var parentTreeView = ParentListBox;
            if (parentTreeView != null && parentTreeView.IsDirectlyGainingKeyboardFocus)
                RaiseEvent(e);
            else
                base.OnGotFocus(e);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            if (e.NewFocus != this)
                return;
            if (ParentListBox.SelectedItem == null)
                RaiseEvent(new RoutedEventArgs(SelectedEvent));
        }


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsSelected)
            {
                Focus();
                e.Handled = true;
            }
            base.OnMouseLeftButtonDown(e);
            if (ParentListBox is null)
                return;
            if (e.ClickCount % 2 != 0)
                return;
            InvokeItem();
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
                Focus();
            base.OnPreviewMouseRightButtonDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (true)
                base.OnKeyDown(e);
            if (!IsSelected || e.Key != Key.Return)
                return;
            e.Handled = true;
            InvokeItem();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.Handled || e.ChangedButton != MouseButton.Left)
                return;
            Focus();
        }
    }

    internal class InvocationController : ControllerBase<IHasInvocationController, IInvocationController>
    {
        public static bool Invoke(IEnumerable<IHasInvocationController> items)
        {
            return Invoke(items, item => item.InvocationController);
        }

        private static bool Invoke(IEnumerable<IHasInvocationController> items, Func<IHasInvocationController, IInvocationController> getController)
        {
            var dictionary = BuildInvocationMap(items, getController);
            var flag = false;
            foreach (var keyValuePair in dictionary)
                flag = keyValuePair.Key.Invoke(keyValuePair.Value) | flag;
            return flag;
        }
    }

    internal class ControllerBase<TPattern, TController>
    {
        protected static Dictionary<TController, IList<TPattern>> BuildInvocationMap(IEnumerable<TPattern> items, Func<TPattern, TController> selectController)
        {
            var dictionary = new Dictionary<TController, IList<TPattern>>();
            foreach (var pattern in items)
            {
                var key = selectController(pattern);
                if (key != null)
                {
                    if (!dictionary.TryGetValue(key, out var patternList))
                    {
                        patternList = new List<TPattern>();
                        dictionary[key] = patternList;
                    }
                    patternList.Add(pattern);
                }
            }
            return dictionary;
        }
    }
}