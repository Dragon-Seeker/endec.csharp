using System;
using io.wispforest.endec.util.funny;

namespace io.wispforest.endec.format.parser;

public abstract class ParsingDeserializer<T, E, P>(T input) : Deserializer<T> where T : class where E : struct, Enum where P : struct {

    protected readonly T input = input;

    private E? _tokenCache;

    protected E currentToken => _tokenCache ??= tokenAtIndex(pointer);

    protected abstract E tokenAtIndex(P index);

    protected abstract P pointer { set; get; }

    protected void movePointer(P to, P? offset = null) {
        pointer = offset != null ? add(to, offset.Value) : to;
        _tokenCache = null;
    }

    protected abstract P add(P p1, P p2);

    protected V movePointer<V>(P to, Func<V> action) {
        var prevPointer = pointer;
        movePointer(to);
        var result = action();
        movePointer(prevPointer);
        return result;
    }

    protected V frame<V>(Func<Deserializer<T>, V> func) {
        var prevPointer = pointer;
        var result = func(this);
        movePointer(prevPointer);
        return result;
    }

    protected void advance(P? offset = null) => movePointer(pointer, offset);

    protected void advance(Func<E, T, P, bool> condition) {
        while (condition(currentToken, input, pointer)) advance();
    }

    public abstract sbyte readSByte(SerializationContext ctx);
    public abstract ushort readUShort(SerializationContext ctx);
    public abstract uint readUInt(SerializationContext ctx);
    public abstract ulong readULong(SerializationContext ctx);

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
    public abstract Optional<V> readOptionalPacked<V>(SerializationContext ctx, Endec<V> endec);
    public abstract SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec);
    public abstract MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec);
    public abstract StructDeserializer structed(SerializationContext ctx);

    public virtual V tryRead<V>(Func<Deserializer<T>, V> reader) {
        var prevPointer = pointer;
        try {
            return reader(this);
        } catch (Exception) {
            movePointer(prevPointer);
            throw;
        }
    }
}