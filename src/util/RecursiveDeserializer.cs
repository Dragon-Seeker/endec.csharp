using System;
using System.Collections.Generic;

namespace io.wispforest.util;

public abstract class RecursiveDeserializer<T> : Deserializer<T>  where T : class  {
    
    private readonly LinkedList<Func<T>> frames = new ();
    private readonly T serialized;

    protected RecursiveDeserializer(T serialized) {
        this.serialized = serialized;
        frames.AddFirst(() => this.serialized);
    }

    /**
     * Get the value currently to be decoded
     * <p>
     * This value is altered by {@link #frame(Supplier, Supplier)} and
     * initially returns the entire serialized input
     */
    protected T getValue() {
        return frames.First.Value();
    }

    /**
     * Decode the next value down the tree, given by {@code nextValue}, by pushing that frame
     * onto the decoding stack, invoking {@code action}, and popping the frame again. Consequently,
     * all decoding of {@code nextValue} must happen inside {@code action}
     * <p>
     * If {@code nextValue} is reading the field of a struct, {@code isStructField} must be set
     */
    protected V frame<V>(Func<T> nextValue, Func<V> action) {
        try {
            frames.AddFirst(nextValue);
            return action();
        } finally {
            frames.RemoveFirst();
        }
    }
    
    public V tryRead<V>(Func<Deserializer<T>, V> reader) {
        var framesBackup = new LinkedList<Func<T>>(frames);

        try {
            return reader(this);
        } catch (Exception e) {
            frames.Clear();
            
            foreach (var frame1 in framesBackup) {
                frames.AddFirst(frame1);
            }

            throw e;
        }
    }
    //--
    
    public abstract byte readByte(SerializationContext ctx);
    public abstract short readShort(SerializationContext ctx);
    public abstract int readInt(SerializationContext ctx);
    public abstract long readLong(SerializationContext ctx);
    public abstract float readFloat(SerializationContext ctx);
    public abstract double readDouble(SerializationContext ctx);
    public abstract int readVarInt(SerializationContext ctx);
    public abstract long readVarLong(SerializationContext ctx);
    public abstract bool readBoolean(SerializationContext ctx);
    public abstract string readString(SerializationContext ctx);
    public abstract byte[] readBytes(SerializationContext ctx);
    public abstract V? readOptional<V>(SerializationContext ctx, Endec<V> endec);

    public abstract SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec);
    public abstract MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec);
    public abstract StructDeserializer structed();
}