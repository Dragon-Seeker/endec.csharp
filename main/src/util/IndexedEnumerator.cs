using System.Collections;
using System.Collections.Generic;

namespace io.wispforest.endec.util;

public interface IndexedEnumerator<out T> : IEnumerator<T> {
    int index { get; }
}

public static class IEnumeratorExtensions {
    public static IndexedEnumerator<T> GetIndexedEnumerator<T>(this IList<T> list) => new WrappedIEnumerator<T>(list.GetEnumerator());
}

public class WrappedIEnumerator<T>(IEnumerator<T> enumerator) : IndexedEnumerator<T> {
    
    public bool MoveNext() {
        var bl = enumerator.MoveNext();

        if (bl) _nextIndex++;

        return bl;
    }

    private int _nextIndex = 0;

    public int index => _nextIndex - 1;
    
    public T Current => enumerator.Current;

    public void Reset() => enumerator.Reset();

    object? IEnumerator.Current => Current;
    
    public void Dispose() => enumerator.Dispose();
}