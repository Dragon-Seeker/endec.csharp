using System;
using System.IO;
using System.Text;
using io.wispforest.endec.util;

namespace io.wispforest.endec.format.binary;

public class BinaryWriterSerializer : Serializer<BinaryWriter> {
    public static Action<string>? DEBUG_HOOK_1 = null;
    public static Action<long, long>? DEBUG_HOOK_2 = null;
    
    protected readonly BinaryWriter input;

    protected BinaryWriterSerializer(BinaryWriter input) {
        this.input = input;
    }

    public static BinaryWriterSerializer of(BinaryWriter input) => new (input);

    public void writeByte(SerializationContext ctx, byte value) => input.Write(value);

    public void writeShort(SerializationContext ctx, short value) => input.Write(value);

    public void writeInt(SerializationContext ctx, int value) => input.Write(value);

    public void writeLong(SerializationContext ctx, long value) => input.Write(value);

    public void writeFloat(SerializationContext ctx, float value) => input.Write(value);

    public void writeDouble(SerializationContext ctx, double value) => input.Write(value);

    public void writeVarInt(SerializationContext ctx, int value) => VarInts.writeInt(value, b => writeByte(ctx, b));

    public void writeVarLong(SerializationContext ctx, long value) => VarInts.writeLong(value, b => writeByte(ctx, b));

    public void writeBoolean(SerializationContext ctx, bool value) => input.Write(value);

    public void writeString(SerializationContext ctx, string value) {
        //input.Write(value);

        DEBUG_HOOK_1?.Invoke(value);

        if (DEBUG_HOOK_2 == null) {
            writeBytes(ctx, Encoding.UTF8.GetBytes(value));
        } else {
            long prevLength = -1;

            if (input.BaseStream is MemoryStream memoryStream1) prevLength = memoryStream1.Length;

            writeBytes(ctx, Encoding.UTF8.GetBytes(value));

            long newLength = -1;

            if (input.BaseStream is MemoryStream memoryStream2) newLength = memoryStream2.Length;

            DEBUG_HOOK_2.Invoke(prevLength, newLength);
        }
    }

    public void writeBytes(SerializationContext ctx, byte[] bytes) {
        writeVarInt(ctx, bytes.Length);
        input.Write(bytes);
    }

    public void writeOptionalDelegate<V>(SerializationContext ctx, Endec<V> endec, bool isPresent, Func<V> getter) {
        writeBoolean(ctx, isPresent);
        if (isPresent) endec.encode(ctx, this, getter());
    }

    public BinaryWriter result() => input;
    
    public MapSerializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec, int size) {
        writeVarInt(ctx, size);
        return new Sequence<V>(this, ctx, valueEndec);
    }

    public SequenceSerializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec, int size) {
        writeVarInt(ctx, size);
        return new Sequence<E>(this, ctx, elementEndec);
    }
   
    public StructSerializer structed(SerializationContext ctx) => new Struct(this);

    protected class Sequence<V>(BinaryWriterSerializer serializer, SerializationContext ctx, Endec<V> valueEndec) : SequenceSerializer<V>, MapSerializer<V> {

        private int _index = 0;

        public void element(V element) {
            valueEndec.encode(ctx.pushIndex(_index), serializer, element);
            _index++;
        }

        public void entry(string key, V value) {
            serializer.writeString(ctx, key);
            valueEndec.encode(ctx.pushField(key), serializer, value);
        }

        public void end() {}
    }

    protected class Struct(BinaryWriterSerializer serializer) : StructSerializer {
        public StructSerializer field<T>(string name, SerializationContext ctx, Endec<T> endec, T value, bool mayOmit) {
            endec.encode(ctx.pushField(name), serializer, value);
            return this;
        }

        public void end() {}
    }
}