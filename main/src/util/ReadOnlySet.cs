using System;
using System.Collections;
using System.Collections.Generic;

namespace io.wispforest.endec.util;

public class ReadOnlySet<T> : ISet<T> {
    
    public int Count => _set.Count;
    public bool IsReadOnly => true;
    
    private readonly ISet<T> _set;

    public ReadOnlySet(ISet<T> set) => this._set = set;
    
    public ReadOnlySet(IEnumerable<T> enumerable) => this._set = new HashSet<T>(enumerable);
    
    public ReadOnlySet(params T[] entries) => this._set = new HashSet<T>(entries);

    public void Add(T item) {
        throw new NotSupportedException("Adding an item is not supported due to being read-only.");
    }

    public void ExceptWith(IEnumerable<T> other) {
        throw new NotSupportedException("Adding an item is not supported due to being read-only.");
    }

    public void IntersectWith(IEnumerable<T> other) {
        throw new NotSupportedException("Adding an item is not supported due to being read-only.");
    }

    public bool IsProperSubsetOf(IEnumerable<T> other) {
        return _set.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other) {
        return _set.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<T> other) {
        return _set.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<T> other) {
        return _set.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<T> other) {
        return _set.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<T> other) {
        return _set.SetEquals(other);
    }

    public void SymmetricExceptWith(IEnumerable<T> other) {
        throw new NotSupportedException("Adding an item is not supported due to being read-only.");
    }

    public void UnionWith(IEnumerable<T> other) {
        throw new NotSupportedException("Adding an item is not supported due to being read-only.");
    }

    bool ISet<T>.Add(T item) {
        throw new NotSupportedException("Adding an item is not supported due to being read-only.");
    }

    public void Clear() {
        throw new NotSupportedException("Adding an item is not supported due to being read-only.");
    }

    public bool Contains(T item) {
        return _set.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        _set.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item) {
        throw new NotSupportedException("Adding an item is not supported due to being read-only.");
    }

    public IEnumerator<T> GetEnumerator() {
        return _set.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}