using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace io.wispforest.util;

public static class DictionaryExtensions {
    public static IDictionary<TKey, TValue> ImmutableWrap<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) {
        return new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
    
    public static void AddAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> additional) {
        foreach (var keyValuePair in additional) {
            dictionary.Add(keyValuePair);
        }
    }
    
    public static void AddAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> additional) {
        foreach (var keyValuePair in additional) {
            dictionary.Add(keyValuePair);
        }
    }
}

public static class SetExtensions {
    public static ISet<T> ImmutableWrap<T>(this ISet<T> set) {
        return new ReadOnlySet<T>(set);
    }

    public static void AddAll<T>(this ISet<T> set, IEnumerable<T> additional) {
        set.UnionWith(additional);
    }
}