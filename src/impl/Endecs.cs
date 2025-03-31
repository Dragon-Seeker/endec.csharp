using System;
using System.Collections.Generic;
using System.Linq;
using io.wispforest.util;

namespace io.wispforest.impl;

public class Endecs { 
    public static readonly StructEndec<object> VOID = EndecUtils.unit<object>(null);
        
    public static readonly Endec<bool> BOOLEAN = EndecUtils.of<bool>((ctx, serializer, value) => serializer.writeBoolean(ctx, value), (ctx, deserializer) => deserializer.readBoolean(ctx)); 
    public static readonly Endec<byte> BYTE = EndecUtils.of<byte>((ctx, serializer, value) => serializer.writeByte(ctx, value), (ctx, deserializer) => deserializer.readByte(ctx));
    public static readonly Endec<short> SHORT = EndecUtils.of<short>((ctx, serializer, value) => serializer.writeShort(ctx, value), (ctx, deserializer) => deserializer.readShort(ctx));
    public static readonly Endec<int> INT = EndecUtils.of<int>((ctx, serializer, value) => serializer.writeInt(ctx, value), (ctx, deserializer) => deserializer.readInt(ctx));
    public static readonly Endec<int> VAR_INT = EndecUtils.of<int>((ctx, serializer, value) => serializer.writeVarInt(ctx, value), (ctx, deserializer) => deserializer.readVarInt(ctx));
    public static readonly Endec<long> LONG = EndecUtils.of<long>((ctx, serializer, value) => serializer.writeLong(ctx, value), (ctx, deserializer) => deserializer.readLong(ctx));
    public static readonly Endec<long> VAR_LONG = EndecUtils.of<long>((ctx, serializer, value) => serializer.writeVarLong(ctx, value), (ctx, deserializer) => deserializer.readVarLong(ctx));
    public static readonly Endec<float> FLOAT = EndecUtils.of<float>((ctx, serializer, value) => serializer.writeFloat(ctx, value), (ctx, deserializer) => deserializer.readFloat(ctx));
    public static readonly Endec<double> DOUBLE = EndecUtils.of<double>((ctx, serializer, value) => serializer.writeDouble(ctx, value), (ctx, deserializer) => deserializer.readDouble(ctx));
    public static readonly Endec<string> STRING = EndecUtils.of<string>((ctx, serializer, value) => serializer.writeString(ctx, value), (ctx, deserializer) => deserializer.readString(ctx));
    public static readonly Endec<byte[]> BYTES = EndecUtils.of<byte[]>((ctx, serializer, value) => serializer.writeBytes(ctx, value), (ctx, deserializer) => deserializer.readBytes(ctx));
    
    public static readonly Endec<int[]> INT_ARRAY = INT.listOf().xmap<int[]>((list) => list.ToArray(), (ints) => new List<int>(ints));
    public static readonly Endec<long[]> LONG_ARRAY = LONG.listOf().xmap((list) => list.ToArray(), (longs) => new List<long>(longs));
    
    public static readonly Endec<Guid> GUID = EndecUtils
        .ifAttr(SerializationAttributes.HUMAN_READABLE, STRING.xmap(Guid.Parse, guid => guid.ToString()))
        .orElse(BYTES.xmap(bytes => new Guid(bytes), guid => guid.ToByteArray()));
    
    public static Endec<V> vectorEndec<C, V>(String name, Endec<C> componentEndec, Func<C, C, V> constructor, Func<V, C> xGetter, Func<V, C> yGetter) { 
        return componentEndec.listOf()
            .validate(validateSize<C>(name, 2))
            .xmap<V>(
                components => constructor(components[0], components[1]),
                vector => [xGetter(vector), yGetter(vector)]
            );
    }

    public static Endec<V> vectorEndec<C, V>(String name, Endec<C> componentEndec, Func<C, C, C, V> constructor, Func<V, C> xGetter, Func<V, C> yGetter, Func<V, C> zGetter) {
        return componentEndec.listOf()
            .validate(validateSize<C>(name, 3))
            .xmap(
                components => constructor(components[0], components[1], components[2]),
                vector => [xGetter(vector), yGetter(vector), zGetter(vector)]
            );
    }

    public static Endec<V> vectorEndec<C, V>(String name, Endec<C> componentEndec, Func<C, C, C, C, V> constructor, Func<V, C> xGetter, Func<V, C> yGetter, Func<V, C> zGetter, Func<V, C> wGetter) {
        return componentEndec.listOf()
            .validate(validateSize<C>(name, 4))
            .xmap(
                components => constructor(components[0], components[1], components[2], components[3]),
                vector => [xGetter(vector), yGetter(vector), zGetter(vector), wGetter(vector)]
            );
    }
    
    private static Action<IList<C>> validateSize<C>(String name, int requiredSize) { 
        return collection => { if (collection.Count() != 4) throw new ArgumentException(name + "collection must have " + requiredSize + " elements"); };
    }
}