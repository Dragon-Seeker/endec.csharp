using System;
using System.Collections;
using System.Collections.Generic;
using io.wispforest;

namespace io.wispforest.endec;

public interface Deserializer<out T> where T : class {
    
    public SerializationContext setupContext(SerializationContext ctx) {
        return ctx;
    }

    public byte readByte(SerializationContext ctx);
    public short readShort(SerializationContext ctx);
    public int readInt(SerializationContext ctx);
    public long readLong(SerializationContext ctx);
    public float readFloat(SerializationContext ctx);
    public double readDouble(SerializationContext ctx);

    public int readVarInt(SerializationContext ctx);
    public long readVarLong(SerializationContext ctx);

    public bool readBoolean(SerializationContext ctx);
    public String readString(SerializationContext ctx);
    public byte[] readBytes(SerializationContext ctx);
    public V? readOptional<V>(SerializationContext ctx, Endec<V> endec);

    public SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec);
    public MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec);
    public StructDeserializer structed();

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

public interface StructDeserializer {

    /**
     * @deprecated Use {{@link #field(String, SerializationContext, Endec, Supplier)}}
     */ 
    public F? field<F>(string name, SerializationContext ctx, Endec<F> endec) {
        return field(name, ctx, endec, null);
    }

    /**
     * Decode the value of field {@code name} using {@code endec}. If no
     * such field exists in the serialized data, then {@code defaultValue}
     * supplier result is used as the returned value
     */
    public F? field<F>(String name, SerializationContext ctx, Endec<F> endec, Func<F>? defaultValueFactory);
}

public interface SelfDescribedDeserializer<out T> : Deserializer<T> where T : class {
    void readAny<S>(SerializationContext ctx, Serializer<S> visitor) where S : class;
}