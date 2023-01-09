using System;
using System.Collections;
using System.Collections.Generic;

namespace AnakinRaW.CommonUtilities.Wpf.Utilities;

// From Stephen Cleary https://github.com/StephenCleary/Mvvm/blob/master/src/Nito.Mvvm.Core/WeakCollection.cs
/// <summary>
/// A collection of weak references to objects.
/// </summary>
/// <typeparam name="T">The type of object to hold weak references to.</typeparam>
public sealed class WeakCollection<T> : IEnumerable<T> where T : class
{
    /// <summary>
    /// The actual collection of strongly-typed weak references.
    /// </summary>
    private readonly List<WeakReference<T>> _list = new();
    
    /// <summary>
    /// Adds a weak reference to an object to the collection. Does not cause a purge.
    /// </summary>
    /// <param name="item">The object to add a weak reference to.</param>
    public void Add(T item)
    {
        _list.Add(new WeakReference<T>(item));
    }

    /// <summary>
    /// Removes a weak reference to an object from the collection. Does not cause a purge.
    /// </summary>
    /// <param name="item">The object to remove a weak reference to.</param>
    /// <returns>True if the object was found and removed; false if the object was not found.</returns>
    public bool Remove(T item)
    {
        for (var i = 0; i != _list.Count; ++i)
        {
            var weakReference = _list[i];
            if (weakReference.TryGetTarget(out var entry) && entry == item)
            {
                _list.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public void Clear()
    {
        _list.Clear();
    }

    /// <summary>
    /// Gets a list of live objects from this collection, causing a purge.
    /// </summary>
    /// <returns></returns>
    public List<T> ToList()
    {
        var ret = new List<T>(_list.Count); 
        
        // This implementation uses logic similar to List<T>.RemoveAll, which always has O(n) time.
        //  Some other implementations seen in the wild have O(n*m) time, where m is the number of dead entries.
        //  As m approaches n (e.g., mass object extinctions), their running time approaches O(n^2).
        var writeIndex = 0;
        for (var readIndex = 0; readIndex != _list.Count; ++readIndex)
        {
            var weakReference = _list[readIndex];
            if (weakReference.TryGetTarget(out var item))
            {
                ret.Add(item);

                if (readIndex != writeIndex)
                    _list[writeIndex] = _list[readIndex];

                ++writeIndex;
            }
        }
        _list.RemoveRange(writeIndex, _list.Count - writeIndex);
        return ret;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}