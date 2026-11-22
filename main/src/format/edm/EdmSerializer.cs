using System;
using System.Collections.Generic;
using io.wispforest.endec;
using io.wispforest.endec.format.edm;
using io.wispforest.endec.util;

namespace io.wispforest.endec.format.edm;

public class EdmSerializer : RecursiveSerializer<EdmElement>, SelfDescribedSerializer<EdmElement> {

    protected EdmSerializer() : base(EdmElements.EMPTY) { }

    public static EdmSerializer of() => new();

    // ---
    
    public override void writeByte(SerializationContext ctx, byte value) => consume(EdmElements.u8(value));

    public override void writeShort(SerializationContext ctx, short value) => consume(EdmElements.i16(value));

    public override void writeInt(SerializationContext ctx, int value) => consume(EdmElements.i32(value));

    public override void writeLong(SerializationContext ctx, long value) => consume(EdmElements.i64(value));

    public void writeSByte(SerializationContext ctx, sbyte value) => consume(EdmElements.i8(value));

    public void writeUShort(SerializationContext ctx, ushort value) => consume(EdmElements.u16(value));

    public void writeInt(SerializationContext ctx, uint value) => consume(EdmElements.u32(value));

    public void writeLong(SerializationContext ctx, ulong value) => consume(EdmElements.u64(value));

    // ---

    public override void writeFloat(SerializationContext ctx, float value) => consume(EdmElements.f32(value));

    public override void writeDouble(SerializationContext ctx, double value) => consume(EdmElements.f64(value));

    // ---

    public override void writeVarInt(SerializationContext ctx, int value) => consume(EdmElements.i32(value));

    public override void writeVarLong(SerializationContext ctx, long value) => consume(EdmElements.i64(value));

    // ---

    public override void writeBoolean(SerializationContext ctx, bool value) => consume(EdmElements.@bool(value));

    public override void writeString(SerializationContext ctx, string value) => consume(EdmElements.@string(value));

    public override void writeBytes(SerializationContext ctx, byte[] bytes) => consume(EdmElements.bytes(bytes));

    public override void writeOptional<V>(SerializationContext ctx, Endec<V> endec, bool isPresent, Func<V> getter) {
        var result = new EdmElement[1];
        frame(encoded => {
            if (isPresent) endec.encode(ctx, this, getter());
            result[0] = encoded.value();
        });
        consume(EdmElements.optional(result[0]));
    }

    // ---

    public override SequenceSerializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec, int size) => new Sequence<E>(this, elementEndec, ctx);

    public override MapSerializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec, int size) => new Map<V>(this, valueEndec, ctx);

    public override StructSerializer structed(SerializationContext ctx) => new Struct(this);

    // ---

    protected class Sequence<V>(EdmSerializer serializer, Endec<V> elementEndec, SerializationContext ctx) : SequenceSerializer<V> {
        private readonly IList<EdmElement> _result = new List<EdmElement>();

        public void element(V element) {
            serializer.frame(encoded => {
                elementEndec.encode(ctx, serializer, element);
                _result.Add(encoded.require("sequence element"));
            });
        }

        public void end() => serializer.consume(EdmElements.sequence(_result));
    }

    protected class Map<V>(EdmSerializer serializer, Endec<V> valueEndec, SerializationContext ctx) : MapSerializer<V> {
        private readonly IDictionary<string, EdmElement> _result = new Dictionary<string, EdmElement>();

        public void entry(String key, V value) {
            serializer.frame(encoded => {
                valueEndec.encode(ctx, serializer, value);
                _result[key] = encoded.require("map value");
            });
        }
        
        public void end() => serializer.consume(EdmElements.map(_result));
    }

    protected class Struct(EdmSerializer serializer) : StructSerializer {
        private readonly IDictionary<string, EdmElement> result = new Dictionary<string, EdmElement>();

        public StructSerializer field<F>(String name, SerializationContext ctx, Endec<F> endec, F value, bool mayOmit) {
            serializer.frame(encoded => {
                endec.encode(ctx, serializer, value);

                var element = encoded.require("struct field");

                if (mayOmit && ReferenceEquals(element, EdmElements.EMPTY)) return;

                result[name] = element;
            });

            return this;
        }
        
        public void end() => serializer.consume(EdmElements.map(result));
    }
}