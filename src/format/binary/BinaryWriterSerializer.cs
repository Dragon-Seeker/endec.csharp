using System.IO;
using io.wispforest.util;

namespace io.wispforest.format.binary;

public class BinaryWriterSerializer : Serializer<BinaryWriter> {
    protected readonly BinaryWriter input;

    protected BinaryWriterSerializer(BinaryWriter input) {
        this.input = input;
    }

    public static BinaryWriterSerializer of(BinaryWriter input) {
        return new BinaryWriterSerializer(input);
    }

    public void writeByte(SerializationContext ctx, byte value) {
        input.Write(value);
    }

    public void writeShort(SerializationContext ctx, short value) {
        input.Write(value);
    }

    public void writeInt(SerializationContext ctx, int value) {
        input.Write(value);
    }

    public void writeLong(SerializationContext ctx, long value) {
        input.Write(value);
    }

    public void writeFloat(SerializationContext ctx, float value) {
        input.Write(value);
    }

    public void writeDouble(SerializationContext ctx, double value) {
        input.Write(value);
    }

    public void writeVarInt(SerializationContext ctx, int value) {
        VarInts.writeInt(value, b => writeInt(ctx, b));
    }

    public void writeVarLong(SerializationContext ctx, long value) {
        VarInts.writeLong(value, b => writeInt(ctx, b));
    }

    public void writeBoolean(SerializationContext ctx, bool value) {
        input.Write(value);
    }

    public void writeString(SerializationContext ctx, string value) {
        input.Write(value);
    }

    public void writeBytes(SerializationContext ctx, byte[] bytes) {
        writeVarInt(ctx, bytes.Length);
        input.Write(bytes);
    }

    #nullable enable
    public void writeOptional<V>(SerializationContext ctx, Endec<V> endec, V? optional) {
        if (optional is null) {
            writeBoolean(ctx, false);
        } else {
            writeBoolean(ctx, true);
            endec.encode(ctx, this, optional);
        }
    }
    #nullable disable

    public BinaryWriter result() {
        return input;
    }
    
    public MapSerializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec, int size) {
        return new BinaryWriterSequenceSerializer<V>(this, ctx, valueEndec);
    }

    public SequenceSerializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec, int size) {
        writeVarInt(ctx, size);
        return new BinaryWriterSequenceSerializer<E>(this, ctx, elementEndec);
    }
   
    public StructSerializer structed() {
        return new BinaryWriterStructSerializer(this);
    }
}

internal class BinaryWriterSequenceSerializer<V>(BinaryWriterSerializer serializer, SerializationContext ctx, Endec<V> valueEndec) 
    : SequenceSerializer<V>, MapSerializer<V> {
    
    public void element(V element) {
        valueEndec.encode(ctx, serializer, element);
    }

    public void entry(string key, V value) {
        serializer.writeString(ctx, key);
        valueEndec.encode(ctx, serializer, value);
    }

    public void end() {}
}

internal class BinaryWriterStructSerializer(BinaryWriterSerializer serializer) : StructSerializer {
    public StructSerializer field<T>(string name, SerializationContext ctx, Endec<T> endec, T value, bool mayOmit) {
        endec.encode(ctx, serializer, value);
        return this;
    }

    public void end() {}
}