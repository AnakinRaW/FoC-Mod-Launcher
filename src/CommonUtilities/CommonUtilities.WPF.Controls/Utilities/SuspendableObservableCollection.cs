using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.Utilities;

internal class SuspendableObservableCollection<T> : ObservableCollection<T>
{
    private int _suspendChangesCount;
    private bool _skippedNotification;

    public IDisposable SuspendChangeNotification() => new SuspendChangesScope(this);

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (AreNotificationsSuspended)
            _skippedNotification = true;
        else
            base.OnCollectionChanged(e);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (AreNotificationsSuspended)
            _skippedNotification = true;
        else
            base.OnPropertyChanged(e);
    }

    private bool AreNotificationsSuspended => _suspendChangesCount > 0;

    private class SuspendChangesScope : DisposableObject
    {
        private readonly SuspendableObservableCollection<T> _collection;

        public SuspendChangesScope(SuspendableObservableCollection<T> collection)
        {
            _collection = collection;
            if (_collection._suspendChangesCount == 0)
                _collection._skippedNotification = false;
            ++_collection._suspendChangesCount;
        }

        protected override void DisposeManagedResources()
        {
            --_collection._suspendChangesCount;
            if (_collection._suspendChangesCount != 0 || !_collection._skippedNotification)
                return;
            _collection.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}