using System;
using System.Collections.Generic;
using System.Linq;
using io.wispforest.endec.impl;
using io.wispforest.endec.util;
using io.wispforest.endec.util.funny;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace io.wispforest.endec.format.edm;

public class EdmDeserializer : RecursiveDeserializer<EdmElement>, SelfDescribedDeserializer<EdmElement> {

    protected EdmDeserializer(EdmElement serialized) : base(serialized) { }

    public static EdmDeserializer of(EdmElement serialized) => new (serialized);

    // ---

    public override byte readByte(SerializationContext ctx) => getValue().cast<byte>();

    public override short readShort(SerializationContext ctx) => getValue().cast<short>();

    public override int readInt(SerializationContext ctx) => getValue().cast<int>();

    public override long readLong(SerializationContext ctx) => getValue().cast<long>();

    public sbyte readSByte(SerializationContext ctx) => getValue().cast<sbyte>();

    public ushort readUShort(SerializationContext ctx) => getValue().cast<ushort>();

    public uint readUInt(SerializationContext ctx) => getValue().cast<uint>();

    public ulong readULong(SerializationContext ctx) => getValue().cast<ulong>();

    // ---

    public override float readFloat(SerializationContext ctx) => getValue().cast<float>();

    public override double readDouble(SerializationContext ctx) => getValue().cast<double>();

    // ---

    public override int readVarInt(SerializationContext ctx) => readInt(ctx);

    public override long readVarLong(SerializationContext ctx) => readLong(ctx);

    // ---

    public override bool readBoolean(SerializationContext ctx) => getValue().cast<bool>();

    public override string readString(SerializationContext ctx) => getValue().cast<string>();

    public override byte[] readBytes(SerializationContext ctx) => getValue().cast<byte[]>();

    public override Optional<V> readOptionalPacked<V>(SerializationContext ctx, Endec<V> endec) {
        var optional = getValue().cast<EdmElement?>();
        return optional is not null
            ? Optional.of(frame(() => optional!, () => endec.decode(ctx, this)))
            : Optional.empty<V>();
    }

    // ---

    public override SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec)
        => new Sequence<E>(this, ctx, elementEndec, getValue().cast<IList<EdmElement>>());

    public override MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec)
        => new Map<V>(this, ctx, valueEndec, getValue().cast<IDictionary<string, EdmElement>>());

    public override StructDeserializer structed(SerializationContext ctx)
        => new Struct(this, getValue().cast<IDictionary<string, EdmElement>>());

    // ---

    public void readAny<S>(SerializationContext ctx, Serializer<S> visitor) where S : class => visit(ctx, visitor, getValue());
    
    private static void visit<S>(SerializationContext ctx, Serializer<S> visitor, EdmElement value) where S : class {
        var type = value.type;

        switch (type) {
            case EdmElementType.I8: visitor.writeSByte(ctx, value.cast<sbyte>()); break;
            case EdmElementType.U8: visitor.writeByte(ctx, value.cast<byte>()); break;
            case EdmElementType.I16: visitor.writeShort(ctx, value.cast<short>()); break;
            case EdmElementType.U16: visitor.writeUShort(ctx, value.cast<ushort>()); break;
            case EdmElementType.I32: visitor.writeInt(ctx, value.cast<int>()); break;
            case EdmElementType.U32: visitor.writeUInt(ctx, value.cast<uint>()); break;
            case EdmElementType.I64: visitor.writeLong(ctx, value.cast<long>()); break;
            case EdmElementType.U64: visitor.writeULong(ctx, value.cast<ulong>()); break;
            case EdmElementType.F32: visitor.writeFloat(ctx, value.cast<float>()); break;
            case EdmElementType.F64: visitor.writeDouble(ctx, value.cast<double>()); break;
            case EdmElementType.BOOLEAN: visitor.writeBoolean(ctx, value.cast<bool>()); break;
            case EdmElementType.STRING: visitor.writeString(ctx, value.cast<string>()); break;
            case EdmElementType.BYTES: visitor.writeBytes(ctx, value.cast<byte[]>()); break;
            case EdmElementType.OPTIONAL: visitor.writeOptional(ctx, Endec.of<EdmElement>(visit, (_, _) => null), value.cast<EdmElement?>()); break;
            case EdmElementType.SEQUENCE: {
                var edmList = value.cast<List<EdmElement>>();
                using var sequence = visitor.sequence(ctx, Endec.of<EdmElement>(visit, (_, _) => null), edmList.Count);
                edmList.ForEach(sequence.element);
                break;
            }
            case EdmElementType.MAP: {
                var edmElements = value.cast<IDictionary<string, EdmElement>>();
                using var map = visitor.map(ctx, Endec.of<EdmElement>(visit, (_, _) => null), edmElements.Count);
                foreach (var entry in edmElements) map.entry(entry.Key, entry.Value);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException($"{nameof(value.type)} type can not be handled in EdmDeserializer.visit method");
        }
    }

    // ---

    protected class Sequence<V>(EdmDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, IList<EdmElement> elements)
        : SequenceDeserializer<V> {
        private readonly IndexedEnumerator<EdmElement> _elements = elements.GetIndexedEnumerator();
        private readonly int _size = elements.Count();

        public int estimatedSize() => _size;

        public bool MoveNext() => _elements.MoveNext();

        public V Current {
            get {
                var element = _elements.Current;
                return deserializer.frame(
                        () => element,
                        () => valueEndec.decode(ctx.pushIndex(_elements.index), deserializer)
                );
            }
        }
        
        public void Reset() => _elements.Reset();

        public void Dispose() => _elements.Dispose();
    }

    protected class Map<V>(EdmDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, IDictionary<string, EdmElement> entries)
        : MapDeserializer<V> {
        private readonly IEnumerator<KeyValuePair<string, EdmElement>> _entries = entries.GetEnumerator();
        private readonly int _size = entries.Count;

        public int estimatedSize() => _size;

        public bool MoveNext() => _entries.MoveNext();

        public KeyValuePair<string, V> Current {
            get {
                var entry = _entries.Current;
                return deserializer.frame(
                        () => entry.Value,
                        () => new KeyValuePair<string, V>(entry.Key, valueEndec.decode(ctx, deserializer))
                );
            }
        }
        
        public void Reset() => _entries.Reset();

        public void Dispose() => _entries.Dispose();
    }

    protected class Struct(EdmDeserializer deserializer, IDictionary<string, EdmElement> map) : StructDeserializer {
        public F field<F>(string name, SerializationContext ctx, Endec<F> endec, Func<F>? defaultValueFactory) {
            if (!map.TryGetValue(name, out var element)) {
                return defaultValueFactory == null ? throw new Exception("Field '" + name + "' was missing from serialized data, but no default value was provided") : defaultValueFactory();
            }

            try {
                return deserializer.frame(
                    () => element,
                    () => endec.decode(ctx, deserializer)
                );
            } catch (Exception e) {
                DebugErrors.decodeErrorHook?.Invoke(element, e);
                throw e;
            }
        }
    }
    
}


