using System;
using System.Collections;
using System.Collections.Generic;

namespace io.wispforest.util;

public class ReadOnlySet<T> : ISet<T> {
    
    public int Count => set.Count;
    public bool IsReadOnly => true;
    
    private readonly ISet<T> set;

    public ReadOnlySet(ISet<T> set) {
        this.set = set;
    }

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
        return set.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other) {
        return set.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<T> other) {
        return set.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<T> other) {
        return set.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<T> other) {
        return set.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<T> other) {
        return set.SetEquals(other);
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
        return set.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        set.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item) {
        throw new NotSupportedException("Adding an item is not supported due to being read-only.");
    }

    public IEnumerator<T> GetEnumerator() {
        return set.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}