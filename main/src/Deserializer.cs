using System;
using System.Collections;
using System.Collections.Generic;
using io.wispforest;
using io.wispforest.endec.util.funny;

namespace io.wispforest.endec;

public interface Deserializer<out T> where T : class {
    
    public SerializationContext setupContext(SerializationContext ctx) {
        return ctx;
    }

    public byte readByte(SerializationContext ctx);
    public short readShort(SerializationContext ctx);
    public int readInt(SerializationContext ctx);
    public long readLong(SerializationContext ctx);

    public sbyte readSByte(SerializationContext ctx) => unchecked((sbyte) readByte(ctx));
    public ushort readUShort(SerializationContext ctx) => (ushort) readByte(ctx);
    public uint readUInt(SerializationContext ctx) => (uint) readByte(ctx);
    public ulong readULong(SerializationContext ctx) => (ulong) readByte(ctx);

    public float readFloat(SerializationContext ctx);
    public double readDouble(SerializationContext ctx);

    public int readVarInt(SerializationContext ctx);
    public long readVarLong(SerializationContext ctx);

    public bool readBoolean(SerializationContext ctx);
    public string readString(SerializationContext ctx);
    public byte[] readBytes(SerializationContext ctx);

    public V? readOptional<V>(SerializationContext ctx, Endec<V> endec, StructMarker _ = default) where V : struct => readOptionalPacked(ctx, endec).unpack();

    public V? readOptional<V>(SerializationContext ctx, Endec<V> endec, ClassMarker _ = default) where V : class => readOptionalPacked(ctx, endec).unpack();

    public Optional<V> readOptionalPacked<V>(SerializationContext ctx, Endec<V> endec);

    public SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec);
    public MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec);
    public StructDeserializer structed(SerializationContext ctx);

    public V tryRead<V>(Func<Deserializer<T>, V> reader);
}

public interface SequenceDeserializer<E> : IEnumerator<E>, IEnumerable<E> { 
    IEnumerator<E> IEnumerable<E>.GetEnumerator() {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this;
    }

    object? IEnumerator.Current => this.Current;

    public int estimatedSize();
}

public interface MapDeserializer<E> : IEnumerator<KeyValuePair<string, E>>, IEnumerable<KeyValuePair<string, E>> {
    IEnumerator<KeyValuePair<string, E>> IEnumerable<KeyValuePair<string, E>>.GetEnumerator() {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this;
    }

    object? IEnumerator.Current => this.Current;

    public int estimatedSize();
}

public interface StructDeserializer: IDisposable {
    /**
     * Decode the value of field {@code name} using {@code endec}. If no
     * such field exists in the serialized data, then {@code defaultValue}
     * supplier result is used as the returned value
     */
    public F field<F>(String name, SerializationContext ctx, Endec<F> endec, Func<F>? defaultValueFactory = null);

    void IDisposable.Dispose() {
        // NO-OP
    }
}

public interface SelfDescribedDeserializer<out T> : Deserializer<T> where T : class {
    void readAny<S>(SerializationContext ctx, Serializer<S> visitor) where S : class;
}