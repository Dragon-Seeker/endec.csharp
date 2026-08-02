using System;
using System.Collections.Generic;

namespace io.wispforest.endec.util.funny;

public static class Optional {
    public static Optional<V> empty<V>() {
        return new Optional<V>(false, default);
    }

    public static Optional<V> of<V>(V value) {
        return new Optional<V>(true, value);
    }

    public static Optional<V> ofNullable<V>(V? value, StructMarker _ = default) where V : struct {
        return new Optional<V>(value.HasValue, value ?? default);
    }

    public static Optional<V> ofNullable<V>(V? value, ClassMarker _ = default) where V: class {
        return new Optional<V>(value != null, value != null ? value : null);
    }
}

public static class OptionalExt {
    public static V? unpack<V>(this Optional<V> optional, StructMarker _ = default) where V : struct {
        return optional.isPresent ? optional.value : null;
    }

    public static V? unpack<V>(this Optional<V> optional, ClassMarker _ = default) where V : class {
        return optional.isPresent ? optional.value : null;
    }
}

public readonly struct Optional<T> : IEquatable<Optional<T>> {
    private readonly T _value;

    public bool isPresent { get; }

    public T value => !isPresent ? throw new NullReferenceException("Optional object was found to be not present i.e. null!") : _value;

    // Constructor handles both null classes and null structs
    internal Optional(bool isPresent, T value) {
        this._value = value;
        this.isPresent = isPresent;
    }

    // Safely get the value or a fallback default
    public T getValueOrDefault(T defaultValue = default!) => isPresent ? _value : defaultValue;

    public bool Equals(Optional<T> other) {
        if (!isPresent && !other.isPresent) return true;
        return isPresent == other.isPresent && EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

    public override int GetHashCode() => isPresent ? EqualityComparer<T>.Default.GetHashCode(_value!) : 0;

    public override string ToString() => isPresent ? _value!.ToString()! : "None";

    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);
    public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);

}