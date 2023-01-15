using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Sklavenwalker.CommonUtilities;

namespace AnakinRaW.CommonUtilities.Wpf.Utilities;

internal sealed class EnumerableModificationDetector : DisposableObject, IEnumerable
{
    private readonly IList _stableList;
    private INotifyCollectionChanged? _changeSource;

    public int Count => _stableList.Count;

    public bool Modified { get; private set; }

    private INotifyCollectionChanged? ChangeSource
    {
        set
        {
            if (_changeSource == value)
                return;
            if (_changeSource != null)
                _changeSource.CollectionChanged -= OnCollectionChanged!;
            _changeSource = value;
            if (_changeSource == null)
                return;
            _changeSource.CollectionChanged += OnCollectionChanged!;
        }
    }

    public EnumerableModificationDetector(IEnumerable source)
    {
        _stableList = new List<object>(source.Cast<object>());
        ChangeSource = source as INotifyCollectionChanged;
    }

    public IEnumerator GetEnumerator()
    {
        return _stableList.GetEnumerator();
    }

    private void OnCollectionChanged(object sender, EventArgs e)
    {
        Modified = true;
        ChangeSource = null;
    }

    protected override void DisposeManagedResources()
    {
        ChangeSource = null;
    }
}