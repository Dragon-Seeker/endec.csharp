using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using io.wispforest.endec.impl;
using io.wispforest.endec.util;

namespace io.wispforest.endec.format.edm;

public abstract class EdmElement {

    public EdmElementType type { get; }
    
    internal EdmElement(EdmElementType type) {
        this.type = type;
    }
    
    public abstract V cast<V>();
    
    public abstract object unwrap();
    
    public override string ToString() => format(new BlockWriter()).buildResult();
    
    private BlockWriter format(BlockWriter formatter) {
        return type switch {
            EdmElementType.BYTES => formatter.writeBlock("bytes(", ")", false, blockWriter => {
                blockWriter.write(Convert.ToBase64String(cast<byte[]>()));
            }),
            EdmElementType.MAP => formatter.writeBlock("map({", "})", blockWriter => {
                var map = cast<IDictionary<String, EdmElement>>();

                int idx = 0;

                foreach (var entry in map) {
                    blockWriter.write("\"" + entry.Key + "\": ");
                    entry.Value.format(blockWriter);

                    if (idx < map.Count - 1) blockWriter.writeln(",");

                    idx++;
                }
            }),
            EdmElementType.SEQUENCE => formatter.writeBlock("sequence([", "])", blockWriter => {
                var list = cast<IList<EdmElement>>();

                for (var idx = 0; idx < list.Count; idx++) {
                    list[idx].format(blockWriter);
                    if (idx < list.Count - 1) blockWriter.writeln(",");
                }
            }),
            EdmElementType.OPTIONAL => formatter.writeBlock("optional(", ")", false, blockWriter => {
                var optional = cast<EdmElement<dynamic>?>();

                if (optional != null) {
                    optional.format(blockWriter);
                } else {
                    blockWriter.write("");
                }
            }),
            EdmElementType.STRING => formatter.writeBlock("string(\"", "\")", false, blockWriter => {
                blockWriter.write(cast<string>());
            }),
            _ => formatter.writeBlock(type.formatName() + "(", ")", false, blockWriter => {
                blockWriter.write(unwrap().ToString());
            })
        };
    }
}

public class EdmElement<T> : EdmElement {

    private T value { get; }

    internal EdmElement(T value, EdmElementType type) : base(type) {
        this.value = value;
    }
    
    public override V cast<V>() {
        return value is V v ? v : throw new InvalidCastException($"Unable to cast [{value?.GetType()}] to [{typeof(V)}");
    }
    
    public override object? unwrap() {
        return value switch {
            null => null,
            EdmElement inner => inner.unwrap(),                                    // OPTIONAL
            IList<EdmElement> list => list.Select(e => e.unwrap()).ToList(),
            IDictionary<string, EdmElement> map => map.ToDictionary(e => e.Key, e => e.Value.unwrap()),
            _ => value
        };
    }

    /**
     * Create a copy of this EDM element as an {@link EdmMap}, which
     * : the {@link io.wispforest.endec.util.MapCarrier} interface
     */
    public EdmMap asMap() {
        return type != EdmElementType.MAP
            ? throw new InvalidCastException("Cannot cast EDM element of type " + type + " to MAP")
            : new EdmMap(new Dictionary<string, EdmElement>(cast<Dictionary<string, EdmElement>>()));
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not EdmElement other || type != other.type) return false;
        return deepEquals(unwrap(), other.unwrap());   // element-wise for IList / IDictionary, Equals otherwise
    }

    private static bool deepEquals(object? a, object? b) {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;

        switch (a) {
            case byte[] bytesA when b is byte[] bytesB:
                return bytesA.AsSpan().SequenceEqual(bytesB);

            case IList<object?> listA when b is IList<object?> listB:
                if (listA.Count != listB.Count) return false;
                for (var i = 0; i < listA.Count; i++) {
                    if (!deepEquals(listA[i], listB[i])) return false;
                }
                return true;

            case IDictionary<string, object?> mapA when b is IDictionary<string, object?> mapB:
                if (mapA.Count != mapB.Count) return false;
                foreach (var entry in mapA) {
                    if (!mapB.TryGetValue(entry.Key, out var other) || !deepEquals(entry.Value, other)) return false;
                }
                return true;

            default:
                return a.Equals(b);
        }
    }

    public override int GetHashCode() {
        return 31 * (value?.GetHashCode() ?? 0) + type.GetHashCode();
    }
    
    // TODO: IMPL
    public static EdmElement<Dictionary<string, EdmElement>> consumeMap(Dictionary<string, EdmElement> value) {
        return new EdmElement<Dictionary<string, EdmElement>>(value, EdmElementType.MAP); // Hangry
    }
}

public static class EdmElements {
    
    public static readonly EdmElement<EdmElement?> EMPTY = new (null, EdmElementType.OPTIONAL);
    
    public static EdmElement<sbyte> i8(sbyte value) => new (value, EdmElementType.I8);

    public static EdmElement<byte> u8(byte value) => new (value, EdmElementType.U8);

    public static EdmElement<short> i16(short value) => new (value, EdmElementType.I16);

    public static EdmElement<ushort> u16(ushort value) => new (value, EdmElementType.U16);

    public static EdmElement<int> i32(int value) => new (value, EdmElementType.I32);

    public static EdmElement<uint> u32(uint value) => new (value, EdmElementType.U32);

    public static EdmElement<long> i64(long value) => new (value, EdmElementType.I64);

    public static EdmElement<ulong> u64(ulong value) => new (value, EdmElementType.U64);

    public static EdmElement<float> f32(float value) => new (value, EdmElementType.F32);

    public static EdmElement<double> f64(double value) => new (value, EdmElementType.F64);

    public static EdmElement<bool> @bool(bool value) => new (value, EdmElementType.BOOLEAN);

    public static EdmElement<string> @string(string value) => new (value, EdmElementType.STRING);

    public static EdmElement<byte[]> bytes(byte[] value) => new (value, EdmElementType.BYTES);

    public static EdmElement<EdmElement?> optional(EdmElement? value) => value is null ? EMPTY : new EdmElement<EdmElement?>(value, EdmElementType.OPTIONAL);

    public static EdmElement<IList<EdmElement>> sequence(IList<EdmElement> value) => new (value.ToList(), EdmElementType.SEQUENCE);

    public static EdmElement<IDictionary<string, EdmElement>> map(IDictionary<string, EdmElement> value) => new (value, EdmElementType.MAP);
    
    internal static EdmElement<object> cast<V>(object value) {
        return value as EdmElement<object> ?? throw new InvalidCastException($"Unable to cast [{value?.GetType()}] to [{typeof(V)}");
    }
}
        
public enum EdmElementType {
    I8,
    U8,
    I16,
    U16,
    I32,
    U32,
    I64,
    U64,
    F32,
    F64,

    BOOLEAN,
    STRING,
    BYTES,
    OPTIONAL,

    SEQUENCE,
    MAP
}

public static class EdmElementTypeUtils {
    public static string formatName(this EdmElementType type){
        return Enum.GetName(typeof(EdmElementType), type)!.ToLower();
    }
}

public class EdmMap : EdmElement<Dictionary<string, EdmElement>>, MapCarrier {

    private readonly Dictionary<string, EdmElement> map;

    internal EdmMap(Dictionary<string, EdmElement> map) : base(map, EdmElementType.MAP) {
        this.map = map;
    }
    
    public T getWithErrors<T>(SerializationContext ctx, KeyedEndec<T> key) {
        return has(key) ? key.endec.decodeFully(ctx.pushField(key.key), EdmDeserializer.of, map[key.key]) : key.defaultValue();
    }
            
    public void put<T>(SerializationContext ctx, KeyedEndec<T> key, T value) {
        map[key.key] = key.endec.encodeFully(ctx.pushField(key.key), EdmSerializer.of, value);
    }
            
    public void delete<T>(KeyedEndec<T> key) {
        map.Remove(key.key);
    }
            
    public bool has<T>(KeyedEndec<T> key) {
        return map.ContainsKey(key.key);
    }
}