using io.wispforest.endec.util;

namespace io.wispforest.endec;

public interface Serializer<out T> where T : class {
    
    public SerializationContext setupContext(SerializationContext ctx) {
        return ctx;
    }

    public void writeByte(SerializationContext ctx, byte value);
    public void writeShort(SerializationContext ctx, short value);
    public void writeInt(SerializationContext ctx, int value);
    public void writeLong(SerializationContext ctx, long value);
    public void writeFloat(SerializationContext ctx, float value);
    public void writeDouble(SerializationContext ctx, double value);

    public void writeVarInt(SerializationContext ctx, int value);
    public void writeVarLong(SerializationContext ctx, long value);

    public void writeBoolean(SerializationContext ctx, bool value);
    public void writeString(SerializationContext ctx, string value);
    public void writeBytes(SerializationContext ctx, byte[] bytes);
    
    public void writeOptional<V>(SerializationContext ctx, Endec<V> endec, V? optional);
    
    public SequenceSerializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec, int size); 
    public MapSerializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec, int size);
    public StructSerializer structed();

    public T result();
}

public interface SequenceSerializer<E> : Endable {
    void element(E element);
}

public interface MapSerializer<V> : Endable {
    void entry(string key, V value);
}

public interface StructSerializer : Endable {
    public StructSerializer field<F>(string name, SerializationContext ctx, Endec<F> endec, F value) {
        return field(name, ctx, endec, value, false);
    }

    public StructSerializer field<F>(string name, SerializationContext ctx, Endec<F> endec, F value, bool mayOmit);
}

public interface SelfDescribedSerializer<T> : Serializer<T> where T : class;