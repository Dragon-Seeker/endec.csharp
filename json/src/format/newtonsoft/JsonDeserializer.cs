using System;
using System.Collections.Generic;
using System.Globalization;
using io.wispforest;
using io.wispforest.endec.impl;
using io.wispforest.endec.util;
using io.wispforest.endec.util.funny;
using Newtonsoft.Json.Linq;

namespace io.wispforest.endec.format.newtonsoft;

public class JsonDeserializer : RecursiveDeserializer<JToken>, SelfDescribedDeserializer<JToken> {
    protected JsonDeserializer(JToken serialized) : base(serialized) { }

    public static JsonDeserializer of(JToken serialized) => new (serialized);
    
    public SerializationContext setupContext(SerializationContext ctx) => ctx.withAttributes(SerializationAttributes.HUMAN_READABLE);
    
    public override byte readByte(SerializationContext ctx) => readPrimitive<byte>(convertible => convertible.ToByte(CultureInfo.CurrentCulture));

    public override short readShort(SerializationContext ctx) => readPrimitive<short>(convertible => convertible.ToInt16(CultureInfo.CurrentCulture));

    public override int readInt(SerializationContext ctx) => readPrimitive<int>(convertible => convertible.ToInt32(CultureInfo.CurrentCulture));

    public override long readLong(SerializationContext ctx) => readPrimitive<long>(convertible => convertible.ToInt64(CultureInfo.CurrentCulture));

    public override float readFloat(SerializationContext ctx) => readPrimitive<float>(convertible => convertible.ToSingle(CultureInfo.CurrentCulture));

    public override double readDouble(SerializationContext ctx) => readPrimitive<double>(convertible => convertible.ToDouble(CultureInfo.CurrentCulture));

    public override int readVarInt(SerializationContext ctx) => readInt(ctx);

    public override long readVarLong(SerializationContext ctx) => readLong(ctx);

    public override bool readBoolean(SerializationContext ctx) => readPrimitive<bool>(convertible => convertible.ToBoolean(CultureInfo.CurrentCulture));

    public override string readString(SerializationContext ctx) => readPrimitive<string>(convertible => convertible.ToString(CultureInfo.CurrentCulture));

    private T readPrimitive<T>(Func<IConvertible, T> func) where T : IConvertible => readPrimitive<T>(getValue(), func);
    
    private T readPrimitive<T>(JToken token, Func<IConvertible, T> func) where T : IConvertible {
        if (token is not JValue jValue) throw new Exception($"Unable to read JToken as it is not a JValue: {token}");
        if (token is null) throw new Exception($"Unable to read JToken as the given value is null and can not be converted to the needed type: {typeof(T)}");
        if (jValue.Value is not IConvertible t) throw new Exception($"Unable to read JValue as it is not a the correct type: [Type: {typeof(T)}, Obj Type: {jValue.Value}]");
        return func(t);
    }

    private T getValueSafe<T>() where T : JToken {
        var value = getValue();

        if (value is null) throw new NullReferenceException("Value was found to be null meaning something has gone wrong");
        if (value is not T token) throw new Exception($"Unable to cast value safely to `{typeof(T)}` as it was found to be '{value.GetType()}'");

        return token;
    }
    
    public override byte[] readBytes(SerializationContext ctx) {
        var array = getValueSafe<JArray>();

        var result = new byte[array.Count];
        for (var i = 0; i < array.Count; i++) {
            result[i] = readPrimitive<byte>(array[i], convertible => convertible.ToByte(CultureInfo.CurrentCulture));
        }

        return result;
    }

    public override Optional<V> readOptionalPacked<V>(SerializationContext ctx, Endec<V> endec) {
        var value = getValue();
        
        return !value.Type.Equals(JTokenType.Null) ? Optional.of(endec.decode(ctx, this)) : Optional.empty<V>();
    }

    public override SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec)
        => new Sequence<E>(this, ctx, elementEndec,getValueSafe<JArray>() );

    public override MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec)
        => new Map<V>(this, ctx, valueEndec, getValueSafe<JObject>());

    public override StructDeserializer structed(SerializationContext ctx)
        => new Struct(this, getValueSafe<JObject>());

    public void readAny<S>(SerializationContext ctx, Serializer<S> visitor) where S : class => decodeValue(ctx, visitor, getValue());
    
    private void decodeValue<S>(SerializationContext ctx, Serializer<S> visitor, JToken element) where S : class {
        var handledToken = true;

        if (element.Type.Equals(JTokenType.Null)) {
            visitor.writeOptional(ctx, JsonEndec.INSTANCE, null);
            return;
        }

        switch (element) {
            case JValue { Value: string str }: visitor.writeString(ctx, str); break;
            case JValue { Value: bool bl }: visitor.writeBoolean(ctx, bl); break;
            case JValue { Value: IConvertible value }:
                try {
                    var asLong = value.ToInt64(CultureInfo.CurrentCulture);

                    if ((byte) asLong == asLong) visitor.writeByte(ctx, value.ToByte(CultureInfo.CurrentCulture));
                    else if ((short) asLong == asLong) visitor.writeShort(ctx, value.ToInt16(CultureInfo.CurrentCulture));
                    else if ((int) asLong == asLong) visitor.writeInt(ctx, value.ToInt32(CultureInfo.CurrentCulture));
                    else visitor.writeLong(ctx, asLong);
                } catch (Exception bruh) when (bruh is FormatException or OverflowException) {
                    var asDouble = value.ToDouble(CultureInfo.CurrentCulture);
                    if ((float) asDouble == asDouble) visitor.writeFloat(ctx, value.ToSingle(CultureInfo.CurrentCulture));
                    else visitor.writeDouble(ctx, asDouble);
                }
                break;
            case JValue: handledToken = false; break;
            case JArray array: {
                using var sequence = visitor.sequence(ctx, Endec.of<JToken>(decodeValue, (_, _) => JValue.CreateNull()), array.Count);
                foreach (var jToken in array) {
                    sequence.element(jToken);
                }
                break;
            }
            case JObject obj: {
                using var map = visitor.map(ctx, Endec.of<JToken>(decodeValue, (_, _) => JValue.CreateNull()), obj.Count);
                foreach (var entry in obj) {
                    map.entry(entry.Key, entry.Value!);
                }
                break;
            }
            default: handledToken = false; break;
        }
        
        if (!handledToken) throw new Exception($"Non-standard, unrecognized JsonElement implementation cannot be decoded: {element}");
    }
    
    protected class Sequence<V>(JsonDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, JArray elements) : SequenceDeserializer<V> {
        private readonly IndexedEnumerator<JToken> _elements = elements.GetIndexedEnumerator();
        private readonly int _size = elements.Count;

        public int estimatedSize() => _size;

        public bool MoveNext() => _elements.MoveNext();

        public V Current {
            get {
                var element = _elements.Current;
                return deserializer.frame(
                        () => element,
                        () => valueEndec.decode(ctx.pushIndex(this._elements.index), deserializer)
                );
            }
        }

        public void Reset() => _elements.Reset();

        public void Dispose() => _elements.Dispose();
    }
    
    protected class Map<V>(JsonDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, JObject entries) : MapDeserializer<V> {
        private readonly IEnumerator<KeyValuePair<string, JToken?>> _entries = entries.GetEnumerator();
        private readonly int _size = entries.Count;

        public int estimatedSize() => _size;

        public bool MoveNext() => _entries.MoveNext();

        public KeyValuePair<string, V> Current => next();
    
        private KeyValuePair<string, V> next() {
            var entry = _entries.Current;
            return deserializer.frame(
                    () => entry.Value!,
                    () => new KeyValuePair<string, V>(entry.Key, valueEndec.decode(ctx.pushField(entry.Key), deserializer))
            );
        }

        public void Reset() => _entries.Reset();

        public void Dispose() => _entries.Dispose();
    }
    
    protected class Struct(JsonDeserializer deserializer, JObject obj) : StructDeserializer {
        public F field<F>(string name, SerializationContext ctx, Endec<F> endec, Func<F>? defaultValueFactory) {
            var element = obj[name];
            if (element is null) {
                return defaultValueFactory is null
                    ? throw new Exception($"Field '{name}' was missing from serialized data, but no default value was provided")
                    : defaultValueFactory();
            }

            try {
                return deserializer.frame(() => element, () => endec.decode(ctx.pushField(name), deserializer));
            } catch (Exception e) {
                DebugErrors.decodeErrorHook?.Invoke(obj, e);
                throw e;
            }
        }
    }
}