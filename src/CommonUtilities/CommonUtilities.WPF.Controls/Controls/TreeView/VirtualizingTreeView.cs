using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using AnakinRaW.CommonUtilities.Wpf.DPI;
using AnakinRaW.CommonUtilities.Wpf.Input;
using AnakinRaW.CommonUtilities.Wpf.Utilities;
using Vanara.PInvoke;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public class VirtualizingTreeView : ListBox
{
    public static readonly DependencyProperty RootItemsSourceProperty =
        DependencyProperty.Register(nameof(RootItemsSource), typeof(IEnumerable), typeof(VirtualizingTreeView),
            new FrameworkPropertyMetadata(OnRootItemsSourceChanged));

    public static readonly DependencyProperty ItemsPathProperty =
        DependencyProperty.Register(nameof(ItemsPath), typeof(string), typeof(VirtualizingTreeView));

    public static readonly DependencyProperty IsExpandablePathProperty =
        DependencyProperty.Register(nameof(IsExpandablePath), typeof(string), typeof(VirtualizingTreeView));

    public static readonly DependencyProperty ShowRootExpanderProperty =
        DependencyProperty.Register(nameof(ShowRootExpander), typeof(bool), typeof(VirtualizingTreeView),
            new FrameworkPropertyMetadata(Boxes.BooleanTrue));

    public static readonly DependencyProperty HierarchyIndentionProperty = DependencyProperty.Register(
        nameof(HierarchyIndention), typeof(double), typeof(VirtualizingTreeView), new PropertyMetadata(18.0));

    private static readonly DependencyPropertyKey IsContextMenuOpenPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly("IsContextMenuOpen", typeof(bool), typeof(VirtualizingTreeView),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    public static readonly DependencyProperty IsContextMenuOpenProperty =
        IsContextMenuOpenPropertyKey.DependencyProperty;

    internal ITreeNode? PendingFocusNode;

    private readonly Dictionary<object, List<TreeNode>> _logicalTree = new();
    private readonly ItemCollectionConverter _itemsConverter = new();
    
    private DispatcherOperation? _pendingScrollRequestCheck;
    private ScrollViewer? _scrollViewer;
    private RoutedEventHandler? _pendingLoadScrollHandler;
    private ITreeNode? _pendingLoadScrollItem;
    private TreeNode? _previousAnchorNode;
    private WeakReference? _lastFocusedNode;
    private bool _delayFocusAnchorItem;

    public IEnumerable<ITreeNode> RootNodes => RootNode.ChildNodes;

    public static bool GetIsContextMenuOpen(UIElement element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));
        return (bool)element.GetValue(IsContextMenuOpenProperty);
    }

    public string ItemsPath
    {
        get => (string)GetValue(ItemsPathProperty);
        set => SetValue(ItemsPathProperty, value);
    }

    public string IsExpandablePath
    {
        get => (string)GetValue(IsExpandablePathProperty);
        set => SetValue(IsExpandablePathProperty, value);
    }

    public bool ShowRootExpander
    {
        get => (bool)GetValue(ShowRootExpanderProperty);
        set => SetValue(ShowRootExpanderProperty, Boxes.Box(value));
    }

    public double HierarchyIndention
    {
        get => (double)GetValue(HierarchyIndentionProperty);
        set => SetValue(HierarchyIndentionProperty, value);
    }

    public IEnumerable? RootItemsSource
    {
        get => (IEnumerable)GetValue(RootItemsSourceProperty);
        set
        {
            if (value == null)
                ClearValue(RootItemsSourceProperty);
            else
                SetValue(RootItemsSourceProperty, value);
        }
    }

    public VirtualizingTreeViewItem? LastFocusedItem
    {
        get
        {
            if (_lastFocusedNode == null)
                return null;
            if (ItemContainerGenerator.ContainerFromItem(_lastFocusedNode.Target) is not VirtualizingTreeViewItem
                virtualizingTreeViewItem)
                return null;
            return !virtualizingTreeViewItem.IsConnectedToPresentationSource()
                ? null
                : virtualizingTreeViewItem;
        }
    }

    public bool UseSingleMouseSelection { get; set; }

    internal bool IsDirectlyGainingKeyboardFocus { get; private set; }

    protected IList InternalItems => Items;

    private TreeNode? AnchorNode { get; set; }

    private TreeNode RootNode { get; }

    static VirtualizingTreeView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(VirtualizingTreeView),
            new FrameworkPropertyMetadata(typeof(VirtualizingTreeView)));
    }

    public VirtualizingTreeView()
    {
        RootNode = new TreeNode(this)
        {
            IsVisible = true,
            IsExpanded = true
        };
    }

    public override void OnApplyTemplate()
    {
        _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        base.OnApplyTemplate();
    }

    public void ScrollIntoViewWhenPossiblyUnloaded(ITreeNode selectionTarget)
    {
        CancelPendingScrollRequestCheck();
        if (IsLoaded)
        {
            EnsureInView(selectionTarget);
            _pendingScrollRequestCheck = Dispatcher.BeginInvoke(() =>
            {
                if (_pendingScrollRequestCheck == null)
                    return;
                if (!IsLoaded)
                    ScrollIntoViewWhenUnloaded(selectionTarget);
                _pendingScrollRequestCheck = null;
            }, DispatcherPriority.Loaded);
        }
        else
            ScrollIntoViewWhenUnloaded(selectionTarget);
    }

    public void ChangeSelection(ITreeNode selectionTarget, ListBoxSelectionAction action, bool scrollIntoView = true)
    {
        EnsureExpanded(selectionTarget);
        if (scrollIntoView)
            ScrollIntoViewWhenPossiblyUnloaded(selectionTarget);
        switch (action)
        {
            case ListBoxSelectionAction.SingleSelection:
                SelectedItem = selectionTarget;
                UpdateAnchorItemCore(selectionTarget);
                break;
            case ListBoxSelectionAction.ExtendSelection:
                var treeNode = selectionTarget as TreeNode;
                var treeIndex1 = AnchorNode!.TreeIndex;
                var treeIndex2 = treeNode!.TreeIndex;
                var flag = treeIndex1 < treeIndex2;
                var num = flag ? 1 : -1;
                SelectedItems.Clear();
                for (var index = treeIndex1; flag ? index <= treeIndex2 : index >= treeIndex2; index += num)
                    SelectedItems.Add(InternalItems[index]);
                break;
            case ListBoxSelectionAction.ToggleSelection:
                if (SelectedItems.Contains(selectionTarget))
                {
                    if (SelectedItems.Count > 1)
                        SelectedItems.Remove(selectionTarget);
                }
                else
                    SelectedItems.Add(selectionTarget);

                UpdateAnchorItemCore(selectionTarget);
                break;
            case ListBoxSelectionAction.AddToSelection:
                if (!SelectedItems.Contains(selectionTarget))
                    SelectedItems.Add(selectionTarget);
                UpdateAnchorItemCore(selectionTarget);
                break;
            case ListBoxSelectionAction.RemoveFromSelection:
                if (SelectedItems.Count <= 1)
                    break;
                SelectedItems.Remove(selectionTarget);
                break;
            case ListBoxSelectionAction.SetAnchorItem:
                if (ReferenceEquals(selectionTarget, _previousAnchorNode))
                {
                    _previousAnchorNode = null;
                    break;
                }

                UpdateAnchorItemCore(selectionTarget);
                break;
        }
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
        FocusNode(SelectedItem as ITreeNode);
    }

    public ISet<TItem> GetDistinctSelection<TItem>() where TItem : class
    {
        var distinctSelection = new HashSet<TItem>();
        foreach (ITreeNode selectedItem in SelectedItems)
        {
            if (GetTreeNodeKeyFromItem(selectedItem.Item) is TItem treeNodeKeyFromItem)
                distinctSelection.Add(treeNodeKeyFromItem);
        }

        return distinctSelection;
    }

    public void ExpandNodeRecursively(ITreeNode node)
    {
        node.IsExpanded = true;
        foreach (var childNode in node.ChildNodes)
            ExpandNodeRecursively(childNode);
    }

    public IDisposable EnterContextMenuVisualState()
    {
        return new ContextMenuScope(this);
    }

    internal static IEnumerable<TPattern> GetItemsOfType<TPattern>(IEnumerable? sourceItems) where TPattern : class
    {
        if (sourceItems == null)
            yield break;
        foreach (var sourceItem in sourceItems)
        {
            if (sourceItem is TPattern pattern)
                yield return pattern;
        }
    }

    internal void SetAnchorItem(object? item, bool validateItemContainer)
    {
        try
        {
            if (validateItemContainer && ItemContainerGenerator.ContainerFromItem(item) == null)
                return;
            AnchorItem = item;
        }
        catch
        {
            // Ignore
        }
    }

    internal void RemoveTreeNode(TreeNode node)
    {
        RemoveNodeCore(node);
        foreach (TreeNode descendantNode in node.DescendantNodes)
            RemoveNodeCore(descendantNode);
    }

    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        if (e.OriginalSource is DependencyObject originalSource && DescendantHasContextMenu(originalSource))
            return;
        using (EnterContextMenuVisualState())
        {
            var menuProviders = GetItemsOfType<IHasContextMenu>(GetDistinctSelection<object>());
            var contextMenu = ItemsControlContextMenuProvider.Instance.Provide(menuProviders);
            if (contextMenu is null)
                return;
            e.Handled = ContextMenuPlotter.Instance.ShowContextMenu(contextMenu, GetContextMenuLocation(), null);
        }
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        if (ReferenceEquals(e.NewFocus, this))
        {
            IsDirectlyGainingKeyboardFocus = true;
            try
            {
                if (_delayFocusAnchorItem)
                {
                    _delayFocusAnchorItem = false;
                    FocusNode(AnchorNode);
                }
                else
                    FocusDefaultItem();
            }
            finally
            {
                IsDirectlyGainingKeyboardFocus = false;
            }
        }
        else if (_delayFocusAnchorItem)
        {
            _delayFocusAnchorItem = false;
            FocusNode(AnchorNode);
        }
        else
            _lastFocusedNode =
                new WeakReference(ItemContainerGenerator.ItemFromContainer((DependencyObject)e.NewFocus));

        base.OnGotKeyboardFocus(e);
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is VirtualizingTreeViewItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new VirtualizingTreeViewItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is VirtualizingTreeViewItem virtualizingTreeViewItem)
        {
            var treeNode = item as TreeNode;
            virtualizingTreeViewItem.TreeNode = treeNode!;
            virtualizingTreeViewItem.SetBinding(VirtualizingTreeViewItem.IsExpandableProperty,
                new Binding
                {
                    Source = treeNode,
                    Path = new PropertyPath("IsExpandable", Array.Empty<object>())
                });
        }

        if (item != PendingFocusNode)
            return;
        PendingFocusNode = null;
        if (element is not IInputElement inputElement)
            return;
        var previousFocus = Keyboard.FocusedElement;
        Dispatcher.BeginInvoke(() =>
        {
            if (Keyboard.FocusedElement != previousFocus)
                return;
            SetAnchorItem(item, true);
            inputElement.Focus();
        }, DispatcherPriority.Normal);
    }
    
    protected internal virtual void CommitChangesBeforeExpansionChange()
    {
    }

    protected virtual void OnRootItemsSourceChanged(DependencyPropertyChangedEventArgs args)
    {
        IEnumerable? enumerable = null;
        if (args.NewValue is IEnumerable newValue)
            enumerable = new ItemCollectionAdapter(newValue);
        RootNode.ItemsSource = enumerable;
    }

    protected virtual object? GetTreeNodeKeyFromItem(object? item)
    {
        return item;
    }

    protected bool FocusNode(ITreeNode? node)
    {
        if (!IsDirectlyGainingKeyboardFocus)
            ScrollIntoView(node!);
        if (ItemContainerGenerator.ContainerFromItem(node) is IInputElement inputElement)
        {
            SetAnchorItem(node!, false);
            inputElement.Focus();
            PendingFocusNode = null;
            return true;
        }

        PendingFocusNode = node;
        return false;
    }
    
    private static void OnRootItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        ((VirtualizingTreeView)obj).OnRootItemsSourceChanged(args);
    }

    private static void EnsureExpanded(ITreeNode selectionTarget)
    {
        for (var parent = selectionTarget.Parent; parent != null; parent = parent.Parent)
            parent.IsExpanded = true;
    }

    private void ScrollIntoViewWhenUnloaded(ITreeNode selectionTarget)
    {
        _pendingLoadScrollItem = selectionTarget;
        if (_pendingLoadScrollHandler != null)
            return;
        _pendingLoadScrollHandler = (_, _) =>
        {
            var pendingLoadScrollItem = _pendingLoadScrollItem;
            if (pendingLoadScrollItem != null)
                EnsureInView(pendingLoadScrollItem);
            Loaded -= _pendingLoadScrollHandler;
            _pendingLoadScrollHandler = null;
            _pendingLoadScrollItem = null;
        };
        Loaded += _pendingLoadScrollHandler;
    }

    private void EnsureInView(ITreeNode treeNode)
    {
        if (IsNodeInView())
            return;
        ScrollIntoView(treeNode);

        bool IsNodeInView()
        {
            try
            {
                if (ItemContainerGenerator.ContainerFromItem(treeNode) is not FrameworkElement frameworkElement)
                    return false;
                var scrollContentPresenter = frameworkElement.FindAncestor<ScrollContentPresenter>();
                if (scrollContentPresenter == null)
                    return false;
                var transformedScroll = frameworkElement.TransformToAncestor(scrollContentPresenter);
                var renderSize = frameworkElement.RenderSize;
                var width1 = renderSize.Width;
                var height1 = renderSize.Height;
                var itemRect = new Rect(0.0, 0.0, width1, height1);
                var transformedItem = transformedScroll.TransformBounds(itemRect);
                renderSize = scrollContentPresenter.RenderSize;
                var scrollWidth = renderSize.Width;
                var scrollHeight = renderSize.Height;
                var scrollRect = new Rect(0.0, 0.0, scrollWidth, scrollHeight);
                return scrollRect.Contains(transformedItem);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    private void CancelPendingScrollRequestCheck()
    {
        if (_pendingScrollRequestCheck == null)
            return;
        _pendingScrollRequestCheck.Abort();
        _pendingScrollRequestCheck = null;
    }

    private void FocusDefaultItem()
    {
        if (_lastFocusedNode?.Target is IInputElement inputElement &&
            (inputElement is not DependencyObject dependencyObject || dependencyObject.IsConnectedToPresentationSource()))
            inputElement.Focus();
        else
            FocusSelectedItem();
    }

    private void UpdateAnchorItemCore(ITreeNode anchorItem)
    {
        _previousAnchorNode = null;
        var anchorNode = AnchorNode;
        PendingFocusNode = null;
        AnchorNode = anchorItem as TreeNode;
        if (ItemContainerGenerator.ContainerFromItem(anchorItem) is not IInputElement inputElement)
            return;
        if (IsKeyboardFocusWithin)
        {
            if (inputElement.IsKeyboardFocusWithin)
                return;
            FocusNode(anchorItem);
        }
        else
        {
            _delayFocusAnchorItem = true;
            _previousAnchorNode = anchorNode;
        }
    }

    private object GetVerifiedTreeNodeKeyFromItem(object? item)
    {
        return GetTreeNodeKeyFromItem(item) ??
               throw new InvalidOperationException("Item key in the VirtualizingTreeView cannot be null");
    }

    private List<TreeNode>? GetTreeNodesCore(object item)
    {
        if (item == null)
            throw new InvalidOperationException("Item in the VirtualizingTreeView cannot be null");
        _logicalTree.TryGetValue(GetVerifiedTreeNodeKeyFromItem(item), out var treeNodesCore);
        return treeNodesCore;
    }

    private TreeNode GetOrCreateTreeNode(object item, TreeNode parentNode)
    {
        var treeNodeList = item != null
            ? GetTreeNodesCore(item)
            : throw new InvalidOperationException("Item in the VirtualizingTreeView cannot be null");
        if (treeNodeList == null)
        {
            treeNodeList = new List<TreeNode>(1);
            _logicalTree[GetVerifiedTreeNodeKeyFromItem(item)] = treeNodeList;
        }
        else
        {
            foreach (var treeNode in treeNodeList)
            {
                if (treeNode.Parent == parentNode)
                    return treeNode;
            }
        }

        var node = new TreeNode(this, item);
        node.Parent = parentNode;
        node.IsExpanded = node.IsDefaultExpanded;
        treeNodeList.Add(node);
        return node;
    }

    private Binding GetIsExpandableBinding(object? item)
    {
        return new Binding
        {
            Source = item,
            Path = new PropertyPath(IsExpandablePath, Array.Empty<object>())
        };
    }

    private void RemoveNodeCore(TreeNode node)
    {
        var treeNodeKeyFromItem = GetVerifiedTreeNodeKeyFromItem(node.Item);
        if (!_logicalTree.TryGetValue(treeNodeKeyFromItem, out var treeNodeList))
            return;
        node.Disconnect();
        treeNodeList.Remove(node);
        if (treeNodeList.Count != 0)
            return;
        _logicalTree.Remove(treeNodeKeyFromItem);
    }

    private bool DescendantHasContextMenu(DependencyObject descendant)
    {
        return descendant.FindAncestorOrSelf(GetParentWithinSubtree, ContextMenuIsEnabled) != null;
    }

    private DependencyObject? GetParentWithinSubtree(DependencyObject o)
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
        return o switch
        {
            UIElement uiElement => uiElement.IsEnabled,
            ContentElement contentElement => contentElement.IsEnabled,
            UIElement3D uiElement3D => uiElement3D.IsEnabled,
            _ => true
        };
    }

    private Point GetContextMenuLocation()
    {
        if (InputManager.Current.MostRecentInputDevice is KeyboardDevice && Keyboard.FocusedElement is UIElement focusedElement)
            return focusedElement.PointToScreen(new Point(0.0, focusedElement.RenderSize.Height));
        var messagePos = User32.GetMessagePos();
        return this.DeviceToLogicalPoint(new Point(NativeExtensions.LoWord(messagePos), NativeExtensions.HiWord(messagePos)));
    }

    private static void SetIsContextMenuOpen(UIElement element, bool value)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));
        element.SetValue(IsContextMenuOpenPropertyKey, value);
    }

    internal class TreeNode : DependencyObject, IWeakEventListener, ITreeNode, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource),
            typeof(IEnumerable), typeof(TreeNode), new PropertyMetadata(OnItemsSourceChanged));

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(nameof(IsExpanded),
            typeof(bool), typeof(TreeNode),
            new PropertyMetadata(Boxes.BooleanFalse, OnIsExpandedChanged, CoerceIsExpanded));

        private static readonly DependencyProperty IsExpandablePrivateProperty =
            DependencyProperty.Register(nameof(IsExpandablePrivate), typeof(bool), typeof(TreeNode),
                new PropertyMetadata(Boxes.BooleanFalse, OnIsExpandablePrivateChanged));

        private CollectionView? _itemsSourceView;
        private TreeNode? _parent;
        private readonly List<TreeNode> _cachedChildNodes = new();
        private int _isInCoerceIsExpandedCount;

        public IEnumerable? ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, Boxes.Box(value));
        }

        private bool IsExpandablePrivate
        {
            get => (bool)GetValue(IsExpandablePrivateProperty);
            set => SetValue(IsExpandablePrivateProperty, Boxes.Box(value));
        }

        public bool IsVirtualRoot => Parent == null;

        public int TreeIndex => IsConnected ? ParentTreeView.InternalItems.IndexOf(this) : -1;

        public bool IsConnected => ParentTreeView != null;

        public IEnumerable<ITreeNode> ChildNodes => _cachedChildNodes;

        public IEnumerable<ITreeNode> DescendantNodes
        {
            get
            {
                return _cachedChildNodes.Concat(_cachedChildNodes.SelectMany(n => n.DescendantNodes));
            }
        }

        public bool IsExpandable
        {
            get
            {
                if (IsPermanentlyExpandedRoot)
                    return true;
                if (!BindingOperations.IsDataBound(this, IsExpandablePrivateProperty))
                    BindingOperations.SetBinding(this, IsExpandablePrivateProperty, ParentTreeView.GetIsExpandableBinding(Item));
                return IsExpandablePrivate;
            }
        }

        private bool IsPermanentlyExpandedRoot
        {
            get
            {
                if (IsVirtualRoot)
                    return true;
                return Parent!.IsVirtualRoot && ParentTreeView is { ShowRootExpander: false };
            }
        }

        public bool IsDefaultExpanded
        {
            get
            {
                if (IsPermanentlyExpandedRoot)
                    return true;
                return Item is IHasChildrenVisibility hasChildrenVisibility &&
                       hasChildrenVisibility.ShowChildrenOnDefault;
            }
        }

        private bool IsExpansionVisible => IsVisible && IsExpanded;

        ITreeNode? ITreeNode.Parent
        {
            get
            {
                if (Parent == null || Parent.IsVirtualRoot)
                    return null;
                return Parent;
            }
        }

        public bool IsCollapsing { get; private set; }

        public VirtualizingTreeView ParentTreeView { get; set; }

        public object? Item { get; }

        public int Depth { get; set; }

        public bool AreChildNodesRealized { get; set; }

        public bool IsVisible { get; set; }

        public TreeNode? Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                if (_parent == null)
                    return;
                Depth = _parent.Depth + 1;
            }
        }
        public TreeNode(VirtualizingTreeView parentTreeView) : this(parentTreeView, null)
        {
        }

        public TreeNode(VirtualizingTreeView parentTreeView, object? item)
        {
            ParentTreeView = parentTreeView;
            Depth = -1;
            Item = item;
        }

        public override string? ToString()
        {
            return Item != null ? Item.ToString() : base.ToString();
        }

        public void Disconnect()
        {
            BindingOperations.ClearBinding(this, ItemsSourceProperty);
            BindingOperations.ClearBinding(this, IsExpandedProperty);
            ParentTreeView = null!;
            Parent = null;
        }

        public TreeNode? FindNearestVisibleNode()
        {
            if (IsVisible)
                return this;
            if (Parent == null)
                return null;
            if (Parent?._cachedChildNodes != null)
            {
                var num = Parent._cachedChildNodes.IndexOf(this);
                if (num >= 0)
                {
                    for (var index = num + 1; index < _parent?._cachedChildNodes.Count; ++index)
                    {
                        if (_parent._cachedChildNodes[index].IsVisible)
                            return _parent._cachedChildNodes[index];
                    }

                    for (var index = num - 1; index >= 0; --index)
                    {
                        if (_parent!._cachedChildNodes[index].IsVisible)
                            return _parent._cachedChildNodes[index].GetLastVisibleDescendantOrSelf();
                    }
                }
            }

            return Parent?.FindNearestVisibleNode();
        }

        public void RealizeChildNodes()
        {
            if (AreChildNodesRealized)
                return;
            RealizeChildNodesCore();
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!(managerType == typeof(CollectionChangedEventManager)))
                return false;
            HandleCollectionChanged((NotifyCollectionChangedEventArgs)e);
            return true;
        }

        internal void ScrollExpansionIntoView()
        {
            if (!IsExpansionVisible)
                return;
            var scrollViewer = ParentTreeView._scrollViewer;
            if (scrollViewer is not { CanContentScroll: true })
                return;
            var treeIndex = TreeIndex;
            var lastVisibleIndex = GetLastVisibleDescendantOrSelf().TreeIndex + 1;
            if (lastVisibleIndex - treeIndex > scrollViewer.ViewportHeight)
                scrollViewer.ScrollToVerticalOffset(treeIndex);
            else
            {
                if (lastVisibleIndex <= scrollViewer.VerticalOffset + scrollViewer.ViewportHeight)
                    return;
                scrollViewer.ScrollToVerticalOffset(lastVisibleIndex - scrollViewer.ViewportHeight);
            }
        }

        private static void OnIsExpandablePrivateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var treeNode = (TreeNode)o;
            treeNode.RaisePropertyChanged(nameof(IsExpandable));
            if (treeNode._isInCoerceIsExpandedCount != 0)
                return;
            treeNode.IsExpanded = treeNode.IsExpandablePrivate && treeNode.IsDefaultExpanded;
        }

        private static void OnIsExpandedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var node = (TreeNode)o;
            var newValue = (bool)e.NewValue;
            if (node.IsVisible)
            {
                if (newValue)
                    node.ExpandInternal();
                else
                    node.CollapseInternal();
            }

            if (!node.IsConnected)
                return;
            if (newValue)
                node.RealizeChildNodes();
        }

        private static object CoerceIsExpanded(DependencyObject o, object value)
        {
            var treeNode = (TreeNode)o;
            ++treeNode._isInCoerceIsExpandedCount;
            try
            {
                var flag = (bool)value;
                if (flag && !treeNode.IsExpandable)
                    return Boxes.BooleanFalse;
                return !flag && treeNode.IsPermanentlyExpandedRoot && treeNode.IsExpandable ? Boxes.BooleanTrue : value;
            }
            finally
            {
                --treeNode._isInCoerceIsExpandedCount;
            }
        }

        private static void OnItemsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var treeNode = (TreeNode)o;
            if (e.NewValue is not IEnumerable newValue)
                treeNode.ClearItemsSource();
            else
                treeNode.SetItemsSource(newValue);
        }

        private void RealizeChildNodesCore()
        {
            var binding = new Binding
            {
                Source = Item,
                Path = new PropertyPath(ParentTreeView.ItemsPath, Array.Empty<object>()),
                Converter = ParentTreeView._itemsConverter
            };
            BindingOperations.SetBinding(this, ItemsSourceProperty, binding);
            AreChildNodesRealized = true;
        }

        private void ExpandInternal()
        {
            if (!IsExpansionVisible)
                return;
            if (!AreChildNodesRealized)
            {
                RealizeChildNodesCore();
            }
            else
            {
                if (_itemsSourceView == null)
                    return;
                InsertChildItems(_itemsSourceView);
            }
        }

        private void CollapseInternal()
        {
            try
            {
                IsCollapsing = true;
                RemoveChildItems();
            }
            finally
            {
                IsCollapsing = false;
            }
        }

        private void ClearItemsSource()
        {
            if (_itemsSourceView == null)
                return;
            if (IsExpansionVisible)
                RemoveChildItems();
            if (IsConnected)
                SyncChildNodes(null);
            UnhookCollectionView(_itemsSourceView);
            _itemsSourceView = null;
            _cachedChildNodes.Clear();
        }

        private void SyncChildNodes(IEnumerable? newSource)
        {
            var treeNodeSet = new HashSet<TreeNode>(_cachedChildNodes);
            _cachedChildNodes.Clear();
            if (newSource != null)
            {
                while (true)
                {
                    using var enumerableSnapshot = new EnumerableModificationDetector(newSource);
                    _cachedChildNodes.Capacity = enumerableSnapshot.Count;
                    foreach (var obj in enumerableSnapshot)
                    {
                        var treeNode = ParentTreeView.GetOrCreateTreeNode(obj, this);
                        _cachedChildNodes.Add(treeNode);
                        treeNodeSet.Add(treeNode);
                        if (enumerableSnapshot.Modified)
                            break;
                    }

                    if (enumerableSnapshot.Modified)
                        _cachedChildNodes.Clear();
                    else
                        break;
                }

                foreach (var cachedChildNode in _cachedChildNodes)
                    treeNodeSet.Remove(cachedChildNode);
            }

            foreach (var node in treeNodeSet)
                ParentTreeView?.RemoveTreeNode(node);
        }

        private void SetItemsSource(IEnumerable newValue)
        {
            ClearItemsSource();
            _itemsSourceView = (CollectionView)CollectionViewSource.GetDefaultView(newValue);
            if (IsConnected)
                SyncChildNodes(_itemsSourceView);
            HookCollectionView(_itemsSourceView);
            if (!IsExpansionVisible)
                return;
            InsertChildItems(_itemsSourceView);
        }

        private void UnhookCollectionView(ICollectionView view)
        {
            CollectionChangedEventManager.RemoveListener(view, this);
        }

        private void HookCollectionView(ICollectionView view)
        {
            CollectionChangedEventManager.AddListener(view, this);
        }

        private void InsertChildItems(IEnumerable? childItems)
        {
            var index = 0;
            if (!IsVirtualRoot)
                index = ParentTreeView.InternalItems.IndexOf(this) + 1;
            if (childItems is not null)
                InsertItems(index, childItems);
        }

        private void InsertItems(int index, IEnumerable items)
        {
            var expandedItems = new List<TreeNode>();
            foreach (var obj in items)
            {
                var treeNode = ParentTreeView.GetOrCreateTreeNode(obj, this);
                InsertNode(index, treeNode);
                if (treeNode.IsExpanded)
                    expandedItems.Add(treeNode);
                ++index;
            }

            foreach (var treeNode in expandedItems)
                treeNode.ExpandInternal();
        }

        private void InsertNode(int index, TreeNode node)
        {
            node.IsVisible = true;
            ParentTreeView?.InternalItems.Insert(index, node);
        }

        private void RemoveChildItems()
        {
            if (ParentTreeView == null)
                return;
            if (IsVirtualRoot)
            {
                ClearNodes();
            }
            else
            {
                var num = ParentTreeView.InternalItems.IndexOf(this);
                var treeIndex = GetLastVisibleDescendantOrSelf().TreeIndex;
                for (var index = treeIndex; index > num && ParentTreeView.SelectedItems.Count > 0; --index)
                {
                    var item = ParentTreeView.InternalItems[index];
                    if (ParentTreeView.SelectionMode == SelectionMode.Single && item == ParentTreeView.SelectedItem) 
                        ParentTreeView.SelectedItem = null;
                    else
                        ParentTreeView.SelectedItems.Remove(item);
                }
                    
                if (ParentTreeView.SelectedItems.Count == 0)
                    ParentTreeView.ChangeSelection(this, ListBoxSelectionAction.SingleSelection);
                if (!IsVisible)
                    return;
                for (var index = treeIndex; index > num; --index)
                    RemoveNodeAt(index);
            }
        }

        private void ClearNodes()
        {
            foreach (TreeNode internalItem in ParentTreeView.InternalItems)
                internalItem.IsVisible = false;
            ParentTreeView.InternalItems.Clear();
        }

        private void RemoveNodeAndDescendants(TreeNode childNode)
        {
            childNode.RemoveChildItems();
            if (!childNode.IsVisible)
                return;
            RemoveNodeAt(childNode.TreeIndex);
        }

        private void RemoveNodeAt(int index)
        {
            ((TreeNode)ParentTreeView.InternalItems[index]!).IsVisible = false;
            ParentTreeView.InternalItems.RemoveAt(index);
        }

        private TreeNode GetLastVisibleDescendantOrSelf()
        {
            var last = _cachedChildNodes.FindLast(node => node.IsVisible);
            if (last == null)
                return this;
            return last.IsExpanded ? last.GetLastVisibleDescendantOrSelf() : last;
        }

        private void InsertChangedItems(int childIndex, IList items)
        {
            if (IsExpansionVisible)
                InsertItems(ConvertCollectionIndexToTreeIndex(childIndex), items);
            foreach (var obj in items)
            {
                var treeNode = ParentTreeView.GetOrCreateTreeNode(obj, this);
                _cachedChildNodes.Insert(childIndex, treeNode);
                ++childIndex;
            }
        }

        private void RemoveChangedItems(int childIndex, IList items, bool removeNodeRecord)
        {
            if (IsExpansionVisible)
            {
                var num = childIndex + items.Count;
                for (var index = childIndex; index < num; ++index)
                {
                    var cachedChildNode = _cachedChildNodes[index];
                    if (cachedChildNode.IsVisible)
                        RemoveNodeAndDescendants(cachedChildNode);
                }
            }

            var range = _cachedChildNodes.GetRange(childIndex, items.Count);
            _cachedChildNodes.RemoveRange(childIndex, items.Count);
            if (removeNodeRecord)
            {
                foreach (var node in range)
                    ParentTreeView?.RemoveTreeNode(node);
            }

            if (ParentTreeView?.SelectedItems.Count != 0)
                return;
            if (childIndex >= _cachedChildNodes.Count)
                childIndex = _cachedChildNodes.Count - 1;
            if (_cachedChildNodes.Count > 0)
            {
                ParentTreeView.ChangeSelection(_cachedChildNodes[childIndex],
                    ListBoxSelectionAction.SingleSelection, false);
            }
            else
            {
                if (IsVirtualRoot)
                    return;
                ParentTreeView.ChangeSelection(this,
                    ListBoxSelectionAction.SingleSelection, false);
            }
        }

        private void ResetChangedItems()
        {
            if (IsExpansionVisible)
                RemoveChildItems();
            SyncChildNodes(_itemsSourceView);
            if (!IsExpansionVisible)
                return;
            InsertChildItems(_itemsSourceView);
        }

        private void MoveChangedItems(int oldIndex, int newIndex, IList newItems)
        {
            RemoveChangedItems(oldIndex, newItems, false);
            InsertChangedItems(newIndex, newItems);
        }

        private void ReplaceChangedItems(int index, IList oldItems, IList newItems)
        {
            RemoveChangedItems(index, oldItems, true);
            InsertChangedItems(index, newItems);
        }

        private void HandleCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (!IsConnected)
                return;
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    InsertChangedItems(args.NewStartingIndex, args.NewItems!);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveChangedItems(args.OldStartingIndex, args.OldItems!, true);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ReplaceChangedItems(args.OldStartingIndex, args.OldItems!, args.NewItems!);
                    break;
                case NotifyCollectionChangedAction.Move:
                    MoveChangedItems(args.OldStartingIndex, args.NewStartingIndex, args.NewItems!);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetChangedItems();
                    break;
            }
        }

        private int ConvertCollectionIndexToTreeIndex(int collectionIndex)
        {
            for (var index = collectionIndex; index < _cachedChildNodes.Count; ++index)
            {
                if (_cachedChildNodes[index].IsVisible)
                    return _cachedChildNodes[index].TreeIndex;
            }

            return GetLastVisibleDescendantOrSelf().TreeIndex + 1;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged.RaiseEvent(this, propertyName);
        }
    }

    private class ItemCollectionConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is not IEnumerable source ? null : new ItemCollectionAdapter(source);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    private class ContextMenuScope : IDisposable
    {
        private readonly VirtualizingTreeView _view;

        private bool IsDisposed { get; set; }

        public ContextMenuScope(VirtualizingTreeView view)
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