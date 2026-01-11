using System;
using System.Collections.Generic;

namespace io.wispforest.endec.util;

public abstract class RecursiveSerializer<T> : Serializer<T> where T : class {
    
    private readonly LinkedList<Action<T>> _frames = new ();
    private T _result;
    
    protected RecursiveSerializer(T initialResult) {
        _result = initialResult;
        _frames.AddFirst(t => _result = t);
    }
    
    /**
     * Store {@code value} into the current encoding location
     * <p>
     * This location is altered by {@link #frame(FrameAction)} and
     * initially is just the serializer's result directly
     */
    protected void consume(T value) {
        _frames.First.Value(value);
    }

    /**
     * Encode the next value down the tree by pushing a new frame
     * onto the encoding stack and invoking {@code action}
     * <p>
     * {@code action} receives {@code encoded}, which is where the next call
     * to {@link #consume(Object)} (which {@code action} must somehow cause) will
     * store the value and allow {@code action} to retrieve it using {@link EncodedValue#value()}
     * or, preferably, {@link EncodedValue#require(String)}
     */
    protected void frame(FrameAction<T> action) {
        var encoded = new EncodedValue<T>();

        _frames.AddFirst(encoded.set);
        action(encoded);
        _frames.RemoveFirst();
    }
    
    public T result() {
        return _result;
    }
    
    //--
    
    public abstract void writeByte(SerializationContext ctx, byte value);
    public abstract void writeShort(SerializationContext ctx, short value);
    public abstract void writeInt(SerializationContext ctx, int value);
    public abstract void writeLong(SerializationContext ctx, long value);
    public abstract void writeFloat(SerializationContext ctx, float value);
    public abstract void writeDouble(SerializationContext ctx, double value);
    public abstract void writeVarInt(SerializationContext ctx, int value);
    public abstract void writeVarLong(SerializationContext ctx, long value);
    public abstract void writeBoolean(SerializationContext ctx, bool value);
    public abstract void writeString(SerializationContext ctx, string value);
    public abstract void writeBytes(SerializationContext ctx, byte[] bytes);
    public abstract void writeOptional<V>(SerializationContext ctx, Endec<V> endec, V? optional);
    
    public abstract SequenceSerializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec, int size);
    public abstract MapSerializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec, int size);
    public abstract StructSerializer structed();
}

public delegate void FrameAction<T>(EncodedValue<T> encoded);

public class EncodedValue<T> {
    private T? _value = default;
    private bool _encoded = false;

    internal void set(T value) {
        _value = value;
        _encoded = true;
    }

    public T value() {
        return _value;
    }

    public bool wasEncoded() {
        return _encoded;
    }

    public T require(String name) {
        if (!_encoded) throw new Exception("Endec for " + name + " serialized nothing");
        return value();
    }
}