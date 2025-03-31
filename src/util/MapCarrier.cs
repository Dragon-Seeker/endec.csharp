using System;
using io.wispforest.impl;

namespace io.wispforest.util;

public interface MapCarrier {
    /**
     * Get the value stored under {@code key} in this object's associated map.
     * If no such value exists, the default value of {@code key} is returned
     * <p>
     * Any exceptions thrown during decoding are propagated to the caller
     */
    public T getWithErrors<T>(SerializationContext ctx, KeyedEndec<T> key);

    public T getWithErrors<T>(KeyedEndec<T> key) {
        return getWithErrors(SerializationContext.empty(), key);
    }

    /**
     * Store {@code value} under {@code key} in this object's associated map
     */
    public void put<T>(SerializationContext ctx, KeyedEndec<T> key, T value);

    public void put<T>(KeyedEndec<T> key, T value) {
        put(SerializationContext.empty(), key, value);
    }

    /**
     * Delete the value stored under {@code key} from this object's associated map,
     * if it is present
     */
    public void delete<T>(KeyedEndec<T> key);

    /**
     * Test whether there is a value stored under {@code key} in this object's associated map
     */
    public bool has<T>(KeyedEndec<T> key);

    // ---

    /**
     * Get the value stored under {@code key} in this object's associated map.
     * If no such value exists <i>or</i> an exception is thrown during decoding,
     * the default value of {@code key} is returned
     */
    public T get<T>(SerializationContext ctx, KeyedEndec<T> key) {
        try {
            return getWithErrors(ctx, key);
        } catch (Exception e) {
            return key.defaultValue();
        }
    }

    public T get<T>(KeyedEndec<T> key) {
        return get(SerializationContext.empty(), key);
    }


    public void putIfNotNull<T>(KeyedEndec<T> key, T? value) {
        putIfNotNull(SerializationContext.empty(), key, value);
    }

    /**
     * If {@code value} is not {@code null}, store it under {@code key} in this
     * object's associated map
     */
    public void putIfNotNull<T>(SerializationContext ctx, KeyedEndec<T> key, T? value) {
        if (value is null) return;
        put(ctx, key, value);
    }

    public void copy<T>(KeyedEndec<T> key, MapCarrier other) {
        copy(SerializationContext.empty(), key, other);
    }

    /**
     * Store the value associated with {@code key} in this object's associated map
     * into the associated map of {@code other} under {@code key}
     * <p>
     * Importantly, this does not copy the value itself - be careful with mutable types
     */
    public void copy<T>(SerializationContext ctx, KeyedEndec<T> key, MapCarrier other) {
        other.put(ctx, key, get(ctx, key));
    }

    public void copyIfPresent<T>(KeyedEndec<T> key, MapCarrier other) {
        copyIfPresent(SerializationContext.empty(), key, other);
    }

    /**
     * Like {@link #copy(SerializationContext, KeyedEndec, MapCarrier)}, but only if this object's associated map
     * has a value stored under {@code key}
     */
    public void copyIfPresent<T>(SerializationContext ctx, KeyedEndec<T> key, MapCarrier other) {
        if (!has(key)) return;
        copy(ctx, key, other);
    }

    public void mutate<T>(KeyedEndec<T> key, Func<T, T> mutator) {
        mutate(SerializationContext.empty(), key, mutator);
    }

    /**
     * Get the value stored under {@code key} in this object's associated map, apply
     * {@code mutator} to it and store the result under {@code key}
     */
    public void mutate<T>(SerializationContext ctx, KeyedEndec<T> key, Func<T, T> mutator) {
        put(ctx, key, mutator(get(ctx, key)));
    }
}