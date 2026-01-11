using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace io.wispforest.endec.util;

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
    
    // public static void PutIfAbsent<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> value) {
    //     if (!dictionary.ContainsKey(key)) dictionary[key] = value();
    // }
    
    public static void PutIfAbsent<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) {
        if (!dictionary.ContainsKey(key)) dictionary[key] = value;
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

public static class EnumerableExtensions {
    public static ISet<T> ImmutableSet<T>(this IEnumerable<T> set) {
        return new ReadOnlySet<T>(set);
    }
}

public static class CustomAttributeExtensions {
    public static bool HasCustomAttribute<T>(this Assembly element) where T : Attribute {
        return element.GetCustomAttribute<T>() is not null;
    }
    
    public static bool HasCustomAttribute<T>(this Module element) where T : Attribute {
        return element.GetCustomAttribute<T>() is not null;
    }
    
    public static bool HasCustomAttribute<T>(this MemberInfo element) where T : Attribute {
        return element.GetCustomAttribute<T>() is not null;
    }
    
    public static bool HasCustomAttribute<T>(this ParameterInfo element) where T : Attribute {
        return element.GetCustomAttribute<T>() is not null;
    }
}