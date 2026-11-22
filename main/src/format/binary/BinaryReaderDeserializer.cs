using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using io.wispforest.endec.util;
using io.wispforest.endec.util.funny;

namespace io.wispforest.endec.format.binary;

public class BinaryReaderDeserializer : Deserializer<BinaryReader> {
    public static Action<string>? DEBUG_HOOK_1 = null;
    
    protected readonly BinaryReader input;

    protected BinaryReaderDeserializer(BinaryReader input) {
        this.input = input;
    }

    public static BinaryReaderDeserializer of(BinaryReader input) => new(input);

    public byte readByte(SerializationContext ctx) => input.ReadByte();

    public short readShort(SerializationContext ctx) => input.ReadInt16();

    public int readInt(SerializationContext ctx) => input.ReadInt32();

    public long readLong(SerializationContext ctx) => input.ReadInt64();

    public sbyte readSByte(SerializationContext ctx) => input.ReadSByte();

    public ushort readUShort(SerializationContext ctx) => input.ReadUInt16();

    public uint readUInt(SerializationContext ctx) => input.ReadUInt32();

    public ulong readULong(SerializationContext ctx) => input.ReadUInt64();

    public float readFloat(SerializationContext ctx) => input.ReadSingle();

    public double readDouble(SerializationContext ctx) => input.ReadDouble();

    public int readVarInt(SerializationContext ctx) => VarInts.readInt(() => readByte(ctx));

    public long readVarLong(SerializationContext ctx) => VarInts.readLong(() => readByte(ctx));

    public bool readBoolean(SerializationContext ctx) => input.ReadBoolean();

    public string readString(SerializationContext ctx) {
        var value = Encoding.UTF8.GetString(readBytes(ctx)); //input.ReadString();
        DEBUG_HOOK_1?.Invoke(value);
        return value;
    }

    public byte[] readBytes(SerializationContext ctx) => input.ReadBytes(readVarInt(ctx));

    public Optional<V> readOptionalPacked<V>(SerializationContext ctx, Endec<V> endec)
        => readBoolean(ctx)
            ? Optional.of(endec.decode(ctx, this))
            : Optional.empty<V>();

    public V tryRead<V>(Func<Deserializer<BinaryReader>, V> reader) => throw new Exception("As BinaryReader cannot be rewound, tryRead(...) cannot be supported");

    public SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec) => new Sequence<E>(this, ctx, elementEndec, readVarInt(ctx));

    public MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec) => new Map<V>(this, ctx, valueEndec, readVarInt(ctx));

    public StructDeserializer structed(SerializationContext ctx) => new Struct(this);

    protected class Sequence<V>(BinaryReaderDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, int size) : SequenceDeserializer<V> {
        private int _index = 0;

        public int estimatedSize() => size;

        public bool MoveNext() {
            if (_index >= size) {
                Current = default;
                return false;
            }
            Current = valueEndec.decode(ctx.pushIndex(_index), deserializer);
            _index++;
            return true;
        }

        public V Current { get; private set; } = default;

        public void Dispose() { /* NO-OP*/ }

        public void Reset() => throw new Exception("As BinaryReader cannot be rewound, Reset(...) cannot be supported");
    }

    protected class Struct(BinaryReaderDeserializer deserializer) : StructDeserializer {
        public F field<F>(string name, SerializationContext ctx, Endec<F> endec, Func<F>? defaultValueFactory) => endec.decode(ctx.pushField(name), deserializer);
    }

    protected class Map<V>(BinaryReaderDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, int size) : MapDeserializer<V> {
        private int _index = 0;

        public int estimatedSize() => size;

        public bool MoveNext() {
            if (_index >= size) {
                Current = default;
                return false;
            }

            Current = readPair();
            _index++;

            return true;
        }

        private KeyValuePair<string, V> readPair() {
            var key = deserializer.readString(ctx);

            return new KeyValuePair<string, V>(key, valueEndec.decode(ctx.pushField(key), deserializer));
        }

        public KeyValuePair<string, V> Current { get; private set; }

        public void Dispose() { /* NO-OP */ }

        public void Reset() => throw new Exception("As BinaryReader cannot be rewound, Reset(...) cannot be supported");
    }
}