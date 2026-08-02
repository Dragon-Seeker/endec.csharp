using System;
using io.wispforest;
using io.wispforest.endec.util;
using Newtonsoft.Json.Linq;

namespace io.wispforest.endec.format.newtonsoft;

public class JsonSerializer : RecursiveSerializer<JToken>, SelfDescribedSerializer<JToken> {

    protected JToken? prefix;

    protected JsonSerializer(JToken? prefix) : base(JValue.CreateNull()) {
        this.prefix = prefix;
    }

    public static JsonSerializer of() => new (null);
    
    public SerializationContext setupContext(SerializationContext ctx) => ctx.withAttributes(SerializationAttributes.HUMAN_READABLE);
    
    //--

    public override void writeByte(SerializationContext ctx, byte value) => consume(new JValue(value));

    public override void writeShort(SerializationContext ctx, short value) => consume(new JValue(value));

    public override void writeInt(SerializationContext ctx, int value) => consume(new JValue(value));

    public override void writeLong(SerializationContext ctx, long value) => consume(new JValue(value));

    public override void writeFloat(SerializationContext ctx, float value) => consume(new JValue(value));

    public override void writeDouble(SerializationContext ctx, double value) => consume(new JValue(value));

    public override void writeVarInt(SerializationContext ctx, int value) => consume(new JValue(value));

    public override void writeVarLong(SerializationContext ctx, long value) => consume(new JValue(value));

    public override void writeBoolean(SerializationContext ctx, bool value) => consume(new JValue(value));

    public override void writeString(SerializationContext ctx, string value) => consume(new JValue(value));

    public override void writeBytes(SerializationContext ctx, byte[] bytes) {
        var result = new JArray();
        for (int i = 0; i < bytes.Length; i++) {
            result.Add(new JValue(bytes[i]));
        }
        consume(result);
    }

    public override void writeOptionalDelegate<V>(SerializationContext ctx, Endec<V> endec, bool isPresent, Func<V> getter) {
        if (isPresent) consume(JValue.CreateNull());
        else endec.encode(ctx, this, getter());
    }

    public override SequenceSerializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec, int size) => new Sequence<E>(this, ctx, elementEndec);

    public override MapSerializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec, int size) => new Map<V>(this, ctx, valueEndec);

    public override StructSerializer structed(SerializationContext ctx) => new Struct(this);
    
    //--

    protected class Struct(JsonSerializer serializer) : StructSerializer {
        private readonly JObject _result = getOrCreateMap(serializer);

        private static JObject getOrCreateMap(JsonSerializer serializer) {
            switch (serializer.prefix) {
                case null:
                    return new JObject();
                case JObject prefixObject:
                    serializer.prefix = null;
                    return prefixObject;
                default:
                    throw new Exception($"Incompatible prefix of type {serializer.prefix.GetType().Name} used for JSON map/struct");
            }
        }
        
        public StructSerializer field<F>(string name, SerializationContext ctx, Endec<F> endec, F value, bool mayOmit) {
            serializer.frame(encoded => {
                endec.encode(ctx.pushField(name), serializer, value);

                var element = encoded.require("struct field");
                
                if (mayOmit && element.Type.Equals(JTokenType.Null)) return;

                _result.Add(name, element);
            });

            return this;
        }
        
        public void end() => serializer.consume(_result);
    }

    protected class Map<V>(JsonSerializer serializer, SerializationContext ctx, Endec<V> valueEndec) : MapSerializer<V> {
        protected readonly JObject _result;

        private static JObject getOrCreateMap(JsonSerializer serializer) {
            switch (serializer.prefix) {
                case null:
                    return new JObject();
                case JObject prefixObject:
                    serializer.prefix = null;
                    return prefixObject;
                default:
                    throw new Exception($"Incompatible prefix of type {serializer.prefix.GetType().Name} used for JSON map/struct");
            }
        }
        
        public void entry(string key, V value) {
            serializer.frame(encoded => {
                valueEndec.encode(ctx.pushField(key), serializer, value);
                _result.Add(key, encoded.require("map value"));
            });
        }
        
        public void end() => serializer.consume(_result);
    }

    protected class Sequence<V>(JsonSerializer serializer, SerializationContext ctx, Endec<V> valueEndec) : SequenceSerializer<V> {
        protected readonly JArray _result = getOrCreateArray(serializer);

        private static JArray getOrCreateArray(JsonSerializer serializer) {
            switch (serializer.prefix) {
                case null:
                    return [];
                case JArray prefixArray:
                    serializer.prefix = null;
                    return prefixArray;
                default:
                    throw new Exception($"Incompatible prefix of type {serializer.prefix.GetType().Name} used for JSON sequence");
            }
        }

        public void element(V element) {
            serializer.frame(encoded => {
                valueEndec.encode(ctx.pushIndex(_result.Count), serializer, element);
                _result.Add(encoded.require("sequence element"));
            });
        }

        public void end() => serializer.consume(_result);
    }
}

