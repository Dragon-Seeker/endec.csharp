using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace io.wispforest.endec.util;

public static class ObjectExtensions {
    public static T apply<T>(this T obj, Action<T> action) {
        action(obj);
        return obj;
    }
}

public static class DictionaryExtensions {
    extension<TKey, TValue>(IDictionary<TKey, TValue> dictionary) {
        public IDictionary<TKey, TValue> immutableWrap() {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

        public void addAll(IDictionary<TKey, TValue> additional) {
            foreach (var keyValuePair in additional) {
                dictionary.Add(keyValuePair);
            }
        }

        public void addAll(IEnumerable<KeyValuePair<TKey, TValue>> additional) {
            foreach (var keyValuePair in additional) {
                dictionary.Add(keyValuePair);
            }
        }

        public void putIfAbsent(TKey key, Func<TValue> value) {
            if (!dictionary.ContainsKey(key)) dictionary[key] = value();
        }

        public void putIfAbsent(TKey key, TValue value) {
            if (!dictionary.ContainsKey(key)) dictionary[key] = value;
        }
    }
}

public static class SetExtensions {
    extension<T>(ISet<T> set) {
        public ISet<T> immutableWrap() {
            return new ReadOnlySet<T>(set);
        }

        public void addAll(IEnumerable<T> additional) {
            set.UnionWith(additional);
        }
    }
}

public static class EnumerableExtensions {
    public static ISet<T> ImmutableSet<T>(this IEnumerable<T> set) {
        return new ReadOnlySet<T>(set);
    }
}

public static class CustomAttributeExtensions {
    public static bool HasCustomAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute {
        return provider switch {
            Assembly t => t.GetCustomAttribute<T>() is not null,
            Module t => t.GetCustomAttribute<T>() is not null,
            MemberInfo t => t.GetCustomAttribute<T>() is not null,
            ParameterInfo t => t.GetCustomAttribute<T>() is not null,
            _ => throw new ArgumentException($"Unable to handle the given ICustomAttributeProvider type: {provider.GetType()}")
        };
    }
}

internal static class ArgumentNullException {
    [Intrinsic]
    public static void ThrowIfNull<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) where T : class {
        if (argument is null) Throw(paramName);
    }

    [Intrinsic]
    public static void ThrowIfNull<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) where T : struct {
        if (!argument.HasValue) Throw(paramName);
    }

    [CLSCompliant(false)]
    public static unsafe void ThrowIfNull([NotNull] void* argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) {
        if (argument is null) Throw(paramName);
    }

    [DoesNotReturn]
    private static void Throw(string? paramName) => throw new System.ArgumentNullException(paramName);
}

internal static class ArgumentOutOfRangeException {
    private static string ArgumentOutOfRange_Generic_MustBeLessOrEqual = "{0} ('{1}') must be less than or equal to '{2}'.";
    private static string ArgumentOutOfRange_Generic_MustBeLess = "{0} ('{1}') must be less than '{2}'.";
    private static string ArgumentOutOfRange_Generic_MustBeGreaterOrEqual = "{0} ('{1}') must be greater than or equal to '{2}'.";
    private static string ArgumentOutOfRange_Generic_MustBeGreater = "{0} ('{1}') must be greater than '{2}'.";
    private static string ArgumentOutOfRange_Generic_MustBeEqual = "{0} ('{1}') must be equal to '{2}'.";
    private static string ArgumentOutOfRange_Generic_MustBeNotEqual = "{0} ('{1}') must not be equal to '{2}'.";

    [DoesNotReturn]
    private static void ThrowGreater<T>(T value, T other, string? paramName) => throw new System.ArgumentOutOfRangeException(paramName, value, string.Format(ArgumentOutOfRange_Generic_MustBeLessOrEqual, paramName, value, other));

    [DoesNotReturn]
    private static void ThrowGreaterEqual<T>(T value, T other, string? paramName) => throw new System.ArgumentOutOfRangeException(paramName, value, string.Format(ArgumentOutOfRange_Generic_MustBeLess, paramName, value, other));

    [DoesNotReturn]
    private static void ThrowLess<T>(T value, T other, string? paramName) => throw new System.ArgumentOutOfRangeException(paramName, value, string.Format(ArgumentOutOfRange_Generic_MustBeGreaterOrEqual, paramName, value, other));

    [DoesNotReturn]
    private static void ThrowLessEqual<T>(T value, T other, string? paramName) => throw new System.ArgumentOutOfRangeException(paramName, value, string.Format(ArgumentOutOfRange_Generic_MustBeGreater, paramName, value, other));

    [DoesNotReturn]
    private static void ThrowEqual<T>(T value, T other, string? paramName) => throw new System.ArgumentOutOfRangeException(paramName, value, string.Format(ArgumentOutOfRange_Generic_MustBeNotEqual, paramName, (object?)value ?? "null", (object?)other ?? "null"));

    [DoesNotReturn]
    private static void ThrowNotEqual<T>(T value, T other, string? paramName) => throw new System.ArgumentOutOfRangeException(paramName, value, string.Format(ArgumentOutOfRange_Generic_MustBeEqual, paramName, (object?)value ?? "null", (object?)other ?? "null"));

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as not equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) { if (EqualityComparer<T>.Default.Equals(value, other)) ThrowEqual(value, other, paramName); }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) { if (!EqualityComparer<T>.Default.Equals(value, other)) ThrowNotEqual(value, other, paramName); }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as less or equal than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T> { if (value.CompareTo(other) > 0) ThrowGreater(value, other, paramName); }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as less than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T> { if (value.CompareTo(other) >= 0) ThrowGreaterEqual(value, other, paramName); }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as greater than or equal than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T> { if (value.CompareTo(other) < 0) ThrowLess(value, other, paramName); }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as greater than than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
    public static void ThrowIfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T> { if (value.CompareTo(other) <= 0) ThrowLessEqual(value, other, paramName); }
}