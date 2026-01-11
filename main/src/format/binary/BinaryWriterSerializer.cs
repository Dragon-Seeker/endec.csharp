using System;
using System.IO;
using System.Text;
using io.wispforest.endec.util;

namespace io.wispforest.endec.format.binary;

public class BinaryWriterSerializer : Serializer<BinaryWriter> {
    public static Action<string> DEBUG_HOOK_1 = (s) => { };
    public static Action<long, long> DEBUG_HOOK_2 = (sizeBefore, sizeAfter) => { };
    
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
        VarInts.writeInt(value, b => writeByte(ctx, b));
    }

    public void writeVarLong(SerializationContext ctx, long value) {
        VarInts.writeLong(value, b => writeByte(ctx, b));
    }

    public void writeBoolean(SerializationContext ctx, bool value) {
        input.Write(value);
    }

    public void writeString(SerializationContext ctx, string value) {
        DEBUG_HOOK_1(value);

        long prevLength = -1;
        
        if (input.BaseStream is MemoryStream memoryStream1) {
            prevLength = memoryStream1.Length;
        }
        
        //input.Write(value);
        
        writeBytes(ctx, Encoding.UTF8.GetBytes(value));
        
        long newLength = -1;
        
        if (input.BaseStream is MemoryStream memoryStream2) {
            newLength = memoryStream2.Length;
        }

        DEBUG_HOOK_2(prevLength, newLength);
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
        writeVarInt(ctx, size);
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

    private int index = 0;
    
    public void element(V element) {
        valueEndec.encode(ctx.pushIndex(index), serializer, element);
        index++;
    }

    public void entry(string key, V value) {
        serializer.writeString(ctx, key);
        valueEndec.encode(ctx.pushField(key), serializer, value);
    }

    public void end() {}
}

internal class BinaryWriterStructSerializer(BinaryWriterSerializer serializer) : StructSerializer {
    public StructSerializer field<T>(string name, SerializationContext ctx, Endec<T> endec, T value, bool mayOmit) {
        endec.encode(ctx.pushField(name), serializer, value);
        return this;
    }

    public void end() {}
}