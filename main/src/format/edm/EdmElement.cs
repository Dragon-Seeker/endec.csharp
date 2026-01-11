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
        return this.type switch {
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
        if (value is V v) return v;

        throw new InvalidCastException($"Unable to cast [{value?.GetType()}] to [{typeof(V)}");
    }
    
    public override object unwrap() {
        if (this.value is List<object> list) {
            return list.Select(o => (o as EdmElement<object>).unwrap()).ToList();
        } else if (this.value is Dictionary<string, object> map) {
            var dict = new Dictionary<string, object>();
            
            foreach (var entry in map) {
                dict[entry.Key] = (entry.Value as EdmElement<object>).unwrap();
            }

            return dict;
        } else if (Nullable.GetUnderlyingType(value.GetType()) != null) {
            return value is not null 
                    ? (value! as EdmElement<object>).unwrap() 
                    : null;
        } else {
            return this.value;
        }
    }

    /**
     * Create a copy of this EDM element as an {@link EdmMap}, which
     * : the {@link io.wispforest.endec.util.MapCarrier} interface
     */
    public EdmMap asMap() {
        if(this.type != EdmElementType.MAP) {
            throw new InvalidCastException("Cannot cast EDM element of type " + this.type + " to MAP");
        }
        
        return new EdmMap(new Dictionary<string, EdmElement>(this.cast<Dictionary<string, EdmElement>>()));
    }

    public override bool Equals(object? obj) {
        if (this == obj) return true;
        if (!(obj is EdmElement<object> that)) return false;
        if ((this.value is null && that.value is null) || (this.value?.Equals(that.value) ?? false)) {
            return this.type == that.type;
        }

        return false;
    }

    public override int GetHashCode() {
        int result = this.value.GetHashCode();
        result = 31 * result + this.type.GetHashCode();
        return result;
    }
    
    // TODO: IMPL
    public static EdmElement<Dictionary<string, EdmElement>> consumeMap(Dictionary<string, EdmElement> value) {
        return new EdmElement<Dictionary<string, EdmElement>>(value, EdmElementType.MAP); // Hangry
    }
}

public class EdmElements {
    
    public static readonly EdmElement<EdmElement?> EMPTY = new (null, EdmElementType.OPTIONAL);
    
    public static EdmElement<sbyte> i8(sbyte value) {
        return new EdmElement<sbyte>(value, EdmElementType.I8);
    }

    public static EdmElement<byte> u8(byte value) {
        return new EdmElement<byte>(value, EdmElementType.U8);
    }

    public static EdmElement<short> i16(short value) {
        return new EdmElement<short>(value, EdmElementType.I16);
    }

    public static EdmElement<ushort> u16(ushort value) {
        return new EdmElement<ushort>(value, EdmElementType.U16);
    }

    public static EdmElement<int> i32(int value) {
        return new EdmElement<int>(value, EdmElementType.I32);
    }

    public static EdmElement<uint> u32(uint value) {
        return new EdmElement<uint>(value, EdmElementType.U32);
    }

    public static EdmElement<long> i64(long value) {
        return new EdmElement<long>(value, EdmElementType.I64);
    }

    public static EdmElement<ulong> u64(ulong value) {
        return new EdmElement<ulong>(value, EdmElementType.U64);
    }

    public static EdmElement<float> f32(float value) {
        return new EdmElement<float>(value, EdmElementType.F32);
    }

    public static EdmElement<double> f64(double value) {
        return new EdmElement<double>(value, EdmElementType.F64);
    }

    public static EdmElement<bool> @bool(bool value) {
        return new EdmElement<bool>(value, EdmElementType.BOOLEAN);
    }

    public static EdmElement<string> @string(string value) {
        return new EdmElement<string>(value, EdmElementType.STRING);
    }

    public static EdmElement<byte[]> bytes(byte[] value) {
        return new EdmElement<byte[]>(value, EdmElementType.BYTES);
    }

    public static EdmElement<EdmElement?> optional(EdmElement? value) {
        return value is null ? EdmElements.EMPTY : new EdmElement<EdmElement?>(value, EdmElementType.OPTIONAL);
    }
    public static EdmElement<IList<EdmElement>> sequence(IList<EdmElement> value) {
        return new EdmElement<IList<EdmElement>>(value.ToList(), EdmElementType.SEQUENCE);
    }

    public static EdmElement<IDictionary<string, EdmElement>> map(IDictionary<string, EdmElement> value) {
        return new EdmElement<IDictionary<string, EdmElement>>(value, EdmElementType.MAP);
    }
    
    internal static EdmElement<object> cast<V>(object value) {
        if (value is EdmElement<object> element) return element;

        throw new InvalidCastException($"Unable to cast [{value?.GetType()}] to [{typeof(V)}");
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
    public static String formatName(this EdmElementType type){
        return Enum.GetName(typeof(EdmElementType), type)!.ToLower();
    }
}

public class EdmMap : EdmElement<Dictionary<string, EdmElement>>, MapCarrier {

    private readonly Dictionary<string, EdmElement> map;

    internal EdmMap(Dictionary<string, EdmElement> map) : base(map, EdmElementType.MAP) {
        this.map = map;
    }
    
    public T getWithErrors<T>(SerializationContext ctx, KeyedEndec<T> key) {
        return this.has(key) ? key.endec.decodeFully(ctx.pushField(key.key), EdmDeserializer.of, this.map[key.key]) : key.defaultValue();
    }
            
    public void put<T>(SerializationContext ctx, KeyedEndec<T> key, T value) {
        this.map[key.key] = key.endec.encodeFully(ctx.pushField(key.key), EdmSerializer.of, value);
    }
            
    public void delete<T>(KeyedEndec<T> key) {
        this.map.Remove(key.key);
    }
            
    public bool has<T>(KeyedEndec<T> key) {
        return this.map.ContainsKey(key.key);
    }
}