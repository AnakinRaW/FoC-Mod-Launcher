using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Sklavenwalker.CommonUtilities.Wpf.Utilities;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;


public class VirtualizingTreeViewItem : ListBoxItem
{
    public static readonly RoutedEvent InvokedEvent;
    public static readonly RoutedEvent InvokeFailedEvent;

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

    public static readonly DependencyProperty IsSelectionTargetProperty =
        DependencyProperty.RegisterAttached("IsSelectionTarget", typeof(bool), typeof(VirtualizingTreeViewItem),
            new FrameworkPropertyMetadata(Boxes.BooleanFalse));

    public static readonly DependencyProperty IsExpandableProperty = DependencyProperty.Register(nameof(IsExpandable),
        typeof(bool), typeof(VirtualizingTreeViewItem),
        new FrameworkPropertyMetadata(Boxes.BooleanFalse));

    public static readonly DependencyProperty ExpandOnDoubleClickProperty =
        DependencyProperty.Register(nameof(ExpandOnDoubleClick), typeof(bool), typeof(VirtualizingTreeViewItem),
            new FrameworkPropertyMetadata(Boxes.BooleanTrue));

    private bool _shouldChangeSelectionOnMouseUp;

    public bool IsExpandable
    {
        get => (bool)GetValue(IsExpandableProperty);
        set => SetValue(IsExpandableProperty, Boxes.Box(value));
    }

    public bool ExpandOnDoubleClick
    {
        get => (bool)GetValue(ExpandOnDoubleClickProperty);
        set => SetValue(ExpandOnDoubleClickProperty, Boxes.Box(value));
    }

    public ITreeNode Node => TreeNode;

    protected VirtualizingTreeView? ParentTreeView => TreeNode.ParentTreeView;

    protected bool CanExpandOnInput
    {
        get
        {
            var parentTreeView = ParentTreeView;
            if (parentTreeView == null || !CanExpand || !IsEnabled)
                return false;
            if (TreeNode.Parent is null)
                return false;
            return !TreeNode.Parent.IsVirtualRoot || parentTreeView.ShowRootExpander;
        }
    }

    internal VirtualizingTreeView.TreeNode TreeNode { get; set; } = null!;

    private static bool IsControlKeyDown => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

    private bool CanExpand => IsExpandable;

    protected bool IsExpanded
    {
        get => TreeNode.IsExpanded;
        set
        {
            TreeNode.IsExpanded = value;
            if (!value)
                return;
            TreeNode.ScrollExpansionIntoView();
        }
    }

    static VirtualizingTreeViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(VirtualizingTreeViewItem),
            new FrameworkPropertyMetadata(typeof(VirtualizingTreeViewItem)));
        InvokedEvent = EventManager.RegisterRoutedEvent(nameof(Invoked), RoutingStrategy.Bubble, typeof(RoutedEventHandler),
            typeof(VirtualizingTreeViewItem));
        InvokeFailedEvent = EventManager.RegisterRoutedEvent(nameof(InvokeFailed), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(VirtualizingTreeViewItem));
    }

    public VirtualizingTreeViewItem()
    {
        Focusable = true;
        AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
    }

    public static bool GetIsSelectionTarget(DependencyObject element) => element != null ? (bool)element.GetValue(IsSelectionTargetProperty) : throw new ArgumentNullException(nameof(element));

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild("PART_Expander") is ToggleButton expander) 
            expander.PreviewMouseLeftButtonDown += OnExpanderPreviewLeftMouseDown;
    }

    private void OnExpanderPreviewLeftMouseDown(object sender, MouseButtonEventArgs e)
    {
        var dataContext = DataContext as VirtualizingTreeView.TreeNode;
        dataContext?.ParentTreeView.CommitChangesBeforeExpansionChange();
        IsExpanded = !IsExpanded;
        if (IsExpanded && this.IsConnectedToPresentationSource() && dataContext != null)
            dataContext.ScrollExpansionIntoView();
        e.Handled = true;
    }

    public bool InvokeItem(InputDevice sourceDevice)
    {
        if (!IsSelected)
            return false;
        var parentTreeView = ParentTreeView;
        if (parentTreeView is null)
            return false;

        var inputSource = sourceDevice switch
        {
            MouseDevice => InputSource.Mouse,
            KeyboardDevice => InputSource.Keyboard,
            _ => InputSource.None
        };
        var invocations = VirtualizingTreeView.GetItemsOfType<IInvokable>(parentTreeView.GetDistinctSelection<object>());

        if (InvocationController.Invoke(invocations, inputSource))
        {
            RaiseEvent(new RoutedEventArgs(InvokedEvent, this));
            return true;
        }
        RaiseEvent(new RoutedEventArgs(InvokeFailedEvent, this));
        if (CanExpandOnInput)
        {
            IsExpanded = !IsExpanded;
            return true;
        }
        return false;
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        if (!IsKeyboardFocusWithin)
            Focus();
        base.OnPreviewMouseRightButtonDown(e);
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);
        if (!ReferenceEquals(e.NewFocus, this))
            return;
        if (ParentTreeView is { SelectedItem: null })
            RaiseEvent(new RoutedEventArgs(SelectedEvent));
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!e.Handled && IsEnabled && ExpandOnDoubleClick && e.ClickCount % 2 == 0 && CanExpand)
        {
            IsExpanded = !IsExpanded;
            e.Handled = true;
        }
        _shouldChangeSelectionOnMouseUp = IsSelected && Keyboard.Modifiers is ModifierKeys.None or ModifierKeys.Control;
        if (_shouldChangeSelectionOnMouseUp)
        {
            Focus();
            e.Handled = true;
        }
        base.OnMouseLeftButtonDown(e);
        var parentTreeView = ParentTreeView;
        if (parentTreeView is null)
            return;
        if (parentTreeView.UseSingleMouseSelection)
        {
            parentTreeView.SelectedItems.Clear();
            MakeSingleSelection();
        }
        if (e.ClickCount % 2 != 0)
            return;
        InvokeItem(e.MouseDevice);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (_shouldChangeSelectionOnMouseUp)
        {
            switch (Keyboard.Modifiers)
            {
                case ModifierKeys.None:
                    if (IsSelected)
                    {
                        MakeSingleSelection();
                    }
                    break;
                case ModifierKeys.Control:
                    if (_shouldChangeSelectionOnMouseUp)
                    {
                        MakeToggleSelection();
                    }
                    break;
            }
        }
        base.OnMouseLeftButtonUp(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled)
            return;
        switch (e.Key)
        {
            case Key.Left:
            case Key.Right:
                if (!IsLogicalLeft(e.Key))
                {
                    if (IsControlKeyDown)
                        break;
                    if (!IsExpanded)
                    {
                        if (CanExpandOnInput)
                            IsExpanded = true;
                        e.Handled = true;
                        break;
                    }
                    if (!HandleLogicalRight())
                        break;
                    e.Handled = true;
                    break;
                }
                if (IsControlKeyDown)
                    break;
                if (IsExpanded && CanExpandOnInput)
                    IsExpanded = false;
                else
                    HandleLogicalLeft();
                e.Handled = true;
                break;
            case Key.Multiply:
                if (ParentTreeView == null || !IsEnabled || e.OriginalSource != this)
                    break;
                ParentTreeView.ExpandNodeRecursively(Node);
                e.Handled = true;
                break;
            case Key.Add:
                if (!CanExpandOnInput || IsExpanded || e.OriginalSource != this)
                    break;
                IsExpanded = true;
                e.Handled = true;
                break;
            case Key.Subtract:
                if (!CanExpandOnInput || !IsExpanded || e.OriginalSource != this)
                    break;
                IsExpanded = false;
                e.Handled = true;
                break;
        }
        if (!IsSelected || e.Key != Key.Return)
            return;
        e.Handled = true;
        InvokeItem(e.KeyboardDevice);
    }

    protected void MakeToggleSelection()
    {
        if (ItemsControl.ItemsControlFromItemContainer(this) is not ListBox listBox)
            return;
        var obj = listBox.ItemContainerGenerator.ItemFromContainer(this);
        if (obj == null)
            return;
        if (listBox.SelectedItems.Contains(obj))
            listBox.SelectedItems.Remove(obj);
        else
            listBox.SelectedItems.Add(obj);
        if (!listBox.IsKeyboardFocusWithin)
            return;
        Focus();
    }

    protected void MakeSingleSelection()
    {
        if (ItemsControl.ItemsControlFromItemContainer(this) is not ListBox listBox)
            return;
        var obj = listBox.ItemContainerGenerator.ItemFromContainer(this);
        if (obj == null)
            return;
        if (listBox.SelectedItems.Count != 1 || listBox.SelectedItems[0] != obj)
        {
            listBox.SelectedItems.Clear();
            listBox.SelectedItem = obj;
            MakeAnchorItem(obj);
        }
        if (!listBox.IsKeyboardFocusWithin)
            return;
        Focus();
    }

    protected virtual void MakeAnchorItem(object item)
    {
        ParentTreeView?.SetAnchorItem(item, false);
    }

    private bool HandleLogicalRight()
    {
        return MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
    }

    private void HandleLogicalLeft()
    {
        var parent = TreeNode.Parent;
        if (parent is null || parent.IsVirtualRoot)
            return;
        ParentTreeView?.ScrollIntoView(parent);
        ((UIElement)ParentTreeView?.ItemContainerGenerator.ContainerFromItem(parent)!).Focus();
    }

    private bool IsLogicalLeft(Key key)
    {
        return FlowDirection != FlowDirection.LeftToRight ? key == Key.Right : key == Key.Left;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!e.Handled || e.ChangedButton != MouseButton.Left)
            return;
        for (var dependencyObject = e.OriginalSource as DependencyObject; 
             dependencyObject != null && dependencyObject != this; 
             dependencyObject = dependencyObject.GetVisualOrLogicalParent())
        {
            if (GetIsSelectionTarget(dependencyObject))
            {
                MakeSingleSelection();
                break;
            }
        }
    }
}