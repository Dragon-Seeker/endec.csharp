using System;
using System.Collections.Generic;
using System.IO;
using io.wispforest.util;

namespace io.wispforest.format.binary;

public class BinaryReaderDeserializer : Deserializer<BinaryReader> {
    protected readonly BinaryReader input;

    protected BinaryReaderDeserializer(BinaryReader input) {
        this.input = input;
    }

    public static BinaryReaderDeserializer of(BinaryReader input) {
        return new BinaryReaderDeserializer(input);
    }

    public byte readByte(SerializationContext ctx) {
        return input.ReadByte();
    }

    public short readShort(SerializationContext ctx) {
        return input.ReadInt16();
    }

    public int readInt(SerializationContext ctx) {
        return input.ReadInt32();
    }

    public long readLong(SerializationContext ctx) {
        return input.ReadInt64();
    }

    public float readFloat(SerializationContext ctx) {
        return input.ReadSingle();
    }

    public double readDouble(SerializationContext ctx) {
        return input.ReadDouble();
    }

    public int readVarInt(SerializationContext ctx) {
        return VarInts.readInt(() => readByte(ctx));
    }

    public long readVarLong(SerializationContext ctx) {
        return VarInts.readLong(() => readByte(ctx));
    }

    public bool readBoolean(SerializationContext ctx) {
        return input.ReadBoolean();
    }

    public string readString(SerializationContext ctx) {
        return input.ReadString();
    }

    public byte[] readBytes(SerializationContext ctx) {
        return input.ReadBytes(readVarInt(ctx));
    }

    public V? readOptional<V>(SerializationContext ctx, Endec<V> endec) {
        var bl = readBoolean(ctx);

        return (bl) ? endec.decode(ctx, this) : default(V);
    }

    public V tryRead<V>(Func<Deserializer<BinaryReader>, V> reader) {
        throw new Exception("As BinaryReader cannot be rewound, tryRead(...) cannot be supported");
    }

    public SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec) {
        return new BinaryReaderSequenceDeserializer<E>(this, ctx, elementEndec, readVarInt(ctx));
    }

    public MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec) {
        return new BinaryReaderMapDeserializer<V>(this, ctx, valueEndec, readVarInt(ctx));
    }

    public StructDeserializer structed() {
        return new BinaryReaderStructDeserializer(this);
    }
}

internal class BinaryReaderSequenceDeserializer<V> : SequenceDeserializer<V> {

    private readonly BinaryReaderDeserializer deserializer;
    
    private readonly SerializationContext ctx;
    private readonly Endec<V> valueEndec;
    private readonly int size;

    private int index = 0;

    public BinaryReaderSequenceDeserializer(BinaryReaderDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, int size) {
        this.deserializer = deserializer;
        this.ctx = ctx;
        this.valueEndec = valueEndec;
        this.size = size;
    }

    public int estimatedSize() {
        return size;
    }

    public bool MoveNext() {
        return index < size;
    }

    public V Current => next();

    public V next() {
        index++;
        return valueEndec.decode(ctx, deserializer);
    }
    
    public void Dispose() {
        // NO-OP
    }

    public void Reset() {
        throw new Exception("As BinaryReader cannot be rewound, Reset(...) cannot be supported");
    }
}

internal class BinaryReaderStructDeserializer : StructDeserializer {

    private readonly BinaryReaderDeserializer deserializer;

    public BinaryReaderStructDeserializer(BinaryReaderDeserializer deserializer) {
        this.deserializer = deserializer;
    }

    public T? field<T>(String name, SerializationContext ctx, Endec<T> endec, Func<T>? defaultValueFactory) {
        return endec.decode(ctx, deserializer);
    }
}

internal class BinaryReaderMapDeserializer<V> : MapDeserializer<V> {

    private readonly BinaryReaderDeserializer deserializer;
    
    private readonly SerializationContext ctx;
    private readonly Endec<V> valueEndec;
    private readonly int size;

    private int index = 0;

    public BinaryReaderMapDeserializer(BinaryReaderDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, int size) {
        this.deserializer = deserializer;
        this.ctx = ctx;
        this.valueEndec = valueEndec;
        this.size = size;
    }

    public int estimatedSize() {
        return size;
    }

    public bool MoveNext() {
        return index < size;
    }

    public KeyValuePair<String, V> next() {
        index++;
        return new KeyValuePair<String, V>(
            deserializer.readString(ctx),
            valueEndec.decode(ctx, deserializer)
        );
    }

    public KeyValuePair<string, V> Current => next();
    
    public void Dispose() {
        // NO-OP
    }

    public void Reset() {
        throw new Exception("As BinaryReader cannot be rewound, Reset(...) cannot be supported");
    }
}