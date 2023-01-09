using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AnakinRaW.CommonUtilities.Wpf.Utilities;

[DebuggerDisplay("#Entries={Count}")]
public class WeakValueDictionary<TKey, TValue> where TValue : class where TKey : notnull
{
    private readonly Dictionary<TKey, WeakReference?> _backingDictionary;
    private int _capacity = 10;

    public WeakValueDictionary() : this(null)
    {
    }

    public WeakValueDictionary(IEqualityComparer<TKey>? keyComparer)
    {
        _backingDictionary = new Dictionary<TKey, WeakReference?>(keyComparer);
    }

    public int Count => _backingDictionary.Count;

    public TValue? this[TKey key]
    {
        get
        {
            var backing = _backingDictionary[key];
            if (backing == null)
                return default;
            if (backing.Target is not TValue target)
            {
                Remove(key);
                throw new KeyNotFoundException();
            }
            return target;
        }
        set
        {
            if (_backingDictionary.Count == _capacity)
            {
                EnumerateAndScavenge(null);
                if (_backingDictionary.Count == _capacity)
                    _capacity = _backingDictionary.Count * 2;
            }
            _backingDictionary[key] = value == null ? null : new WeakReference(value);
        }
    }

    public bool Contains(TKey key) => TryGetValue(key, out _);

    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (_backingDictionary.TryGetValue(key, out var weakReference))
        {
            if (weakReference == null)
            {
                value = default;
                return true;
            }
            value = weakReference.Target as TValue;
            if (value != null)
                return true;
            Remove(key);
            return false;
        }
        value = default;
        return false;
    }

    public bool Remove(TKey key) => _backingDictionary.Remove(key);

    public IEnumerable<TValue?> Values
    {
        get
        {
            foreach (var weakReference in _backingDictionary.Values)
            {
                if (weakReference is null)
                    yield return null;
                else
                {
                    var target = weakReference.Target as TValue;
                    if (weakReference.IsAlive)
                        yield return target;
                }
            }
        }
    }

    public IEnumerable<TKey> Keys => _backingDictionary.Keys;

    public IEnumerable<TKey> GetAliveKeys()
    {
        var aliveKeys = new List<TKey>();
        EnumerateAndScavenge(aliveKeys);
        return aliveKeys;
    }

    public void Scavenge() => EnumerateAndScavenge(null);

    private int EnumerateAndScavenge(ICollection<TKey>? aliveKeys)
    {
        var kList = (List<TKey>?)null;
        foreach (var backing in _backingDictionary)
        {
            if (backing.Value == null)
                aliveKeys?.Add(backing.Key);
            else if (backing.Value.Target is not TValue)
            {
                kList ??= new List<TKey>();
                kList.Add(backing.Key);
            }
            else
                aliveKeys?.Add(backing.Key);
        }
        if (kList == null)
            return 0;
        foreach (var key in kList)
            _backingDictionary.Remove(key);
        return kList.Count;
    }

    public void Clear() => _backingDictionary.Clear();
}