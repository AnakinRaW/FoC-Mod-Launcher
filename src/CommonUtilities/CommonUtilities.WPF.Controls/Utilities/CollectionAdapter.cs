using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Utilities;

internal abstract class CollectionAdapter<TSource, TTarget> : ReadOnlyObservableCollection<TTarget>, IWeakEventListener
{
    private IEnumerable? _view;

    protected SuspendableObservableCollection<TTarget> InnerItems => (SuspendableObservableCollection<TTarget>)Items;

    protected CollectionAdapter() : base(new SuspendableObservableCollection<TTarget>())
    {
    }

    protected abstract TTarget AdaptItem(TSource item);

    protected void Initialize(IEnumerable source)
    {
        _view = source;
        while (true)
        {
            using var enumerableSnapshot = new EnumerableModificationDetector(_view);
            foreach (TSource item in enumerableSnapshot)
            {
                Items.Add(AdaptItem(item));
                if (enumerableSnapshot.Modified)
                    break;
            }
            if (enumerableSnapshot.Modified)
                Items.Clear();
            else
                break;
        }
        if (_view is not INotifyCollectionChanged view)
            return;
        CollectionChangedEventManager.AddListener(view, this);
    }

    protected virtual void InsertSourceItem(int index, TSource item)
    {
        InnerItems.Insert(index, AdaptItem(item));
    }

    protected virtual void RemoveSourceItem(int index)
    {
        InnerItems.RemoveAt(index);
    }

    protected virtual void MoveSourceItem(int sourceIndex, int targetIndex)
    {
        InnerItems.Move(sourceIndex, targetIndex);
    }

    protected virtual void ReplaceSourceItem(int index, TSource item)
    {
        InnerItems[index] = AdaptItem(item);
    }

    protected virtual void ResetSourceItems(IEnumerable? newItems)
    {
        using (InnerItems.SuspendChangeNotification())
        {
            InnerItems.Clear();
            var index = 0;
            if (newItems == null)
                return;
            foreach (TSource newItem in newItems)
            {
                InsertSourceItem(index, newItem);
                ++index;
            }
        }
    }

    private void OnInnerCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                for (var index = 0; index < e.NewItems!.Count; ++index)
                    InsertSourceItem(e.NewStartingIndex + index, (TSource)e.NewItems[index]!);
                break;
            case NotifyCollectionChangedAction.Remove:
                for (var index = 0; index < e.OldItems!.Count; ++index)
                    RemoveSourceItem(e.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                for (var index = 0; index < e.NewItems!.Count; ++index)
                    ReplaceSourceItem(e.OldStartingIndex + index, (TSource)e.NewItems[index]!);
                break;
            case NotifyCollectionChangedAction.Move:
                for (var index = 0; index < e.NewItems!.Count; ++index)
                    MoveSourceItem(e.OldStartingIndex + index, e.NewStartingIndex + index);
                break;
            case NotifyCollectionChangedAction.Reset:
                ResetSourceItems(_view);
                break;
        }
    }

    bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
    {
        if (!(managerType == typeof(CollectionChangedEventManager)))
            return false;
        OnInnerCollectionChanged((NotifyCollectionChangedEventArgs)e);
        return true;
    }
}