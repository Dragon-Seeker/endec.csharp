using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using io.wispforest.endec.format.parser;
using io.wispforest.endec.impl;
using io.wispforest.endec.util;
using io.wispforest.endec.util.funny;
using ArgumentNullException = System.ArgumentNullException;
using ArgumentOutOfRangeException = io.wispforest.endec.util.ArgumentOutOfRangeException;

namespace io.wispforest.endec.format.edm;

[Experimental("edm_binary_parsing_deserializer")]
public class EdmBinaryDeserializer(Stream stream, long size) : ParsingDeserializer<BinaryReader, EdmElementType, long>(new BinaryReader(stream.CanSeek ? stream : new LookbackBuffer(stream, size))) {
    protected override long pointer {
        get => input.BaseStream.Position;
        set => input.BaseStream.Seek(value, SeekOrigin.Begin);
    }

    protected override long add(long p1, long p2) => p1 + p2;

    protected override EdmElementType tokenAtIndex(long index) {
        pointer = index;
        return (EdmElementType) input.ReadByte();
    }

    private T readValidated<T>(EdmElementType type, Func<T> readFunc) {
        ArgumentOutOfRangeException.ThrowIfNotEqual(type, currentToken);
        return readFunc();
    }

    public override byte readByte(SerializationContext ctx) => readValidated(EdmElementType.I8, () => input.ReadByte());

    public override short readShort(SerializationContext ctx) => readValidated(EdmElementType.I16, () => input.ReadInt16());

    public override int readInt(SerializationContext ctx) => readValidated(EdmElementType.I32, () => input.ReadInt32());

    public override long readLong(SerializationContext ctx) => readValidated(EdmElementType.I64, () => input.ReadInt64());

    public override sbyte readSByte(SerializationContext ctx) => readValidated(EdmElementType.U8, () => input.ReadSByte());

    public override ushort readUShort(SerializationContext ctx) => readValidated(EdmElementType.U16, () => input.ReadUInt16());

    public override uint readUInt(SerializationContext ctx) => readValidated(EdmElementType.U32, () => input.ReadUInt32());

    public override ulong readULong(SerializationContext ctx) => readValidated(EdmElementType.U64, () => input.ReadUInt64());

    public override float readFloat(SerializationContext ctx) => readValidated(EdmElementType.F32, () => input.ReadSingle());

    public override double readDouble(SerializationContext ctx) => readValidated(EdmElementType.F64, () => input.ReadDouble());

    public override int readVarInt(SerializationContext ctx) => readInt(ctx);

    public override long readVarLong(SerializationContext ctx) => readByte(ctx);

    public override bool readBoolean(SerializationContext ctx) => readValidated(EdmElementType.BOOLEAN, () => input.ReadBoolean());

    public override string readString(SerializationContext ctx) => readValidated(EdmElementType.BOOLEAN, () => Encoding.UTF8.GetString(readBytes(ctx)));

    public override byte[] readBytes(SerializationContext ctx) => readValidated(EdmElementType.BYTES, () => input.ReadBytes(readVarInt(ctx)));

    public override Optional<V> readOptionalPacked<V>(SerializationContext ctx, Endec<V> endec)
        => readBoolean(ctx)
            ? Optional.of(endec.decode(ctx, this))
            : Optional.empty<V>();

    public override SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec)
        => new Sequence<E>(this, ctx, elementEndec, VarInts.readInt(input.ReadByte));

    public override MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec)
        => new Map<V>(this, ctx, valueEndec, VarInts.readInt(input.ReadByte));

    public override StructDeserializer structed(SerializationContext ctx)
        => new Struct(this, VarInts.readInt(input.ReadByte));

    private void _skipElementData() => _skipElementData(currentToken);

    private void _skipElementData(EdmElementType type) {
        switch (type) {
            case EdmElementType.I8 or EdmElementType.U8 or EdmElementType.BOOLEAN:
                pointer += 1;
                return;
            case EdmElementType.I16 or EdmElementType.U16:
                pointer += 2;
                return;
            case EdmElementType.I32 or EdmElementType.U32 or EdmElementType.F32:
                pointer += 4;
                return;
            case EdmElementType.I64 or EdmElementType.U64 or EdmElementType.F64:
                pointer += 8;
                return;
            case EdmElementType.STRING:
                var stringSize = input.Read7BitEncodedIntPublic();
                pointer += stringSize;
                return;
            case EdmElementType.BYTES:
                var byteSize = VarInts.readInt(input.ReadByte);
                pointer += byteSize;
                return;
            case EdmElementType.OPTIONAL:
                if (input.ReadByte() != 0) _skipElementData();
                return;
            case EdmElementType.SEQUENCE:
                var sequenceSize = VarInts.readInt(input.ReadByte);
                for (var i = 0; i < sequenceSize; i++) _skipElementData();
                return;
            case EdmElementType.MAP:
                var mapSize = VarInts.readInt(input.ReadByte);
                for (var i = 0; i < mapSize; i++) _skipSkipKeyValue();
                return;
        }

        throw new System.ArgumentOutOfRangeException(nameof(EdmElementType), type, "type can not be handled in EdmIo.decodeElementData method");
    }

    private void _skipSkipKeyValue() {
        var keySize = input.Read7BitEncodedIntPublic();
        pointer += keySize;
        _skipElementData();
    }

    protected class Sequence<V>(EdmBinaryDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, int length) : SequenceDeserializer<V> {
        private int _index = 0;
        private readonly long _startPos = deserializer.pointer;

        public int estimatedSize() => length;

        public bool MoveNext() {
            if (_index + 1 >= length) return false;
            _index++;
            Current = valueEndec.decode(ctx.pushIndex(_index), deserializer);
            return true;
        }

        public V Current { get; private set; } = default;

        public void Reset() => deserializer.pointer = _startPos;

        public void Dispose() {
            if (_index >= length) return;
            // DECODE VALUES UNTIL WE REACH END OF LIST
            for (var i = _index; i < length; i++) deserializer._skipElementData();
        }
    }

    protected class Map<V>(EdmBinaryDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, int length) : MapDeserializer<V> {
        private int _index = 0;
        private readonly long _startPos = deserializer.pointer;

        public int estimatedSize() => length;

        public bool MoveNext() {
            if (_index + 1 >= length) return false;
            _index++;
            Current = new KeyValuePair<string, V>(
                deserializer.readString(ctx),
                valueEndec.decode(ctx.pushIndex(_index), deserializer)
            );
            return true;
        }

        public KeyValuePair<string, V> Current { get; private set; } = default;

        public void Reset() => deserializer.pointer = _startPos;

        public void Dispose() {
            if (_index >= length) return;
            // DECODE VALUES UNTIL WE REACH END OF LIST
            for (var i = _index; i < length; i++) deserializer._skipSkipKeyValue();
        }
    }

    protected class Struct(EdmBinaryDeserializer deserializer, int length) : StructDeserializer {
        private int _index = 0;
        private readonly long _startPos = deserializer.pointer;

        private readonly IDictionary<string, long> _valueIndicesByKey = new Dictionary<string, long>();

        private bool _reachedEndOfStruct = false;

        public bool MoveNext() {
            if (_index + 1 >= length) return false;
            _index++;
            return true;
        }

        public F field<F>(string name, SerializationContext ctx, Endec<F> endec, Func<F>? defaultValueFactory = null) {
            if (_valueIndicesByKey.TryGetValue(name, out var idx)) {
                return deserializer.movePointer(idx, () => endec.decode(ctx, deserializer));
            }

            while (MoveNext()) {
                var key = deserializer.readString(ctx);

                if (key == name) return endec.decode(ctx, deserializer);

                _valueIndicesByKey[key] = deserializer.pointer;
                EdmIo.decode(deserializer.input);
            }

            throw new Exception("Field '" + name + "' was missing from serialized data, but no default value was provided");
        }

        public void Dispose() {
            if (_index >= length) return;
            // DECODE VALUES UNTIL WE REACH END OF LIST
            for (var i = _index; i < length; i++) deserializer._skipSkipKeyValue();
        }
    }
}

public class LookbackBuffer(Stream underlyingStream, long length) : Stream, IDisposable {
    private readonly Stream _underlyingStream = underlyingStream ?? throw new ArgumentNullException(nameof(underlyingStream));
    private readonly MemoryStream _cache = new ();
    private long _virtualPosition = 0;

    public override void Flush() {
        _cache.Flush();
        _underlyingStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count) {
        int bytesReadFromCache = 0;

        if (_virtualPosition < _cache.Length) {
            _cache.Position = _virtualPosition;
            var readableSpace = _cache.Length - _virtualPosition;
            bytesReadFromCache = _cache.Read(buffer, offset, readableSpace < count ? (int) readableSpace : count);
            _virtualPosition += bytesReadFromCache;

            // If we satisfied the full request from cache, return
            if (bytesReadFromCache == count) return bytesReadFromCache;

            // Otherwise, adjust offset and count to read the remainder from the live stream
            offset += bytesReadFromCache;
            count -= bytesReadFromCache;
        }

        // 2. Read new data from the underlying live stream
        return bytesReadFromCache + readAndCopyFromUnderlyingStream(count, offset, buffer);
    }

    private int readAndCopyFromUnderlyingStream(int count, int offset = 0, byte[]? buffer = null) {
        if (buffer == null) buffer = new byte[count];
        int bytesReadFromStream = _underlyingStream.Read(buffer, offset, count);
        if (bytesReadFromStream > 0) {
            // Append this new data to the end of our cache so we can backtrack to it later
            _cache.Position = _cache.Length;
            _cache.Write(buffer, offset, bytesReadFromStream);
            _virtualPosition += bytesReadFromStream;
        }
        return bytesReadFromStream;
    }

    public override long Seek(long offset, SeekOrigin origin) {
        offset = origin switch {
            SeekOrigin.Current => _virtualPosition + offset,
            SeekOrigin.End => length + offset,
            _ => offset
        };

        if (offset > _cache.Length) readAndCopyFromUnderlyingStream((int) (offset - _cache.Length));

        return _virtualPosition = offset;
    }

    public override int ReadByte() {
        var readFromCache = _virtualPosition < _cache.Length;
        var value = readFromCache ? _cache.ReadByte() : _underlyingStream.ReadByte();

        if (value == -1) return -1;

        var byteValue = unchecked((byte) value);

        if (!readFromCache) _cache.WriteByte(byteValue);

        _virtualPosition++;

        return byteValue;
    }

    public override void SetLength(long value) => throw new NotSupportedException("Unable to use 'SetLength' as it is not allowed!");
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("Unable to use 'Write' as it is not allowed!");

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length { get; } = length;

    public override long Position {
        get => _virtualPosition;
        set => _virtualPosition = value;
    }

    public void Dispose() => _cache.Dispose();
}

internal static class BinaryReaderExt {
    private const int MaxBytesWithoutOverflow = 4;
    extension(BinaryReader reader) {
        public int Read7BitEncodedIntPublic() {
            uint result = 0;
            byte byteReadJustNow;

            for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7) {
                // ReadByte handles end of stream cases for us.
                byteReadJustNow = reader.ReadByte();
                result |= (byteReadJustNow & 0x7Fu) << shift;

                if (byteReadJustNow <= 0x7Fu) return (int)result; // early exit
            }

            byteReadJustNow = reader.ReadByte();
            if (byteReadJustNow > 0b_1111u) throw new FormatException("Too many bytes in what should have been a 7-bit encoded integer.");

            result |= (uint)byteReadJustNow << (MaxBytesWithoutOverflow * 7);
            return (int)result;
        }
    }
}