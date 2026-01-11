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

        if (bl) index++;

        return bl;
    }
    
    public int index { get; private set; }
    
    public T Current => enumerator.Current;

    public void Reset() => enumerator.Reset();

    object? IEnumerator.Current => Current;
    
    public void Dispose() => enumerator.Dispose();
}