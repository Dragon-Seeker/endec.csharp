using System;
using System.Collections.Generic;
using System.Linq;
using io.wispforest.endec.impl;
using io.wispforest.endec.util;

namespace io.wispforest.endec.format.edm;

public class EdmDeserializer : RecursiveDeserializer<EdmElement>, SelfDescribedDeserializer<EdmElement> {

    protected EdmDeserializer(EdmElement serialized) : base(serialized) { }

    public static EdmDeserializer of(EdmElement serialized) {
        return new EdmDeserializer(serialized);
    }

    // ---


    public override byte readByte(SerializationContext ctx) {
        return this.getValue().cast<byte>();
    }

    public override short readShort(SerializationContext ctx) {
        return this.getValue().cast<short>();
    }

    public override int readInt(SerializationContext ctx) {
        return this.getValue().cast<int>();
    }

    public override long readLong(SerializationContext ctx) {
        return this.getValue().cast<long>();
    }

    // ---

    public override float readFloat(SerializationContext ctx) {
        return this.getValue().cast<float>();
    }

    public override double readDouble(SerializationContext ctx) {
        return this.getValue().cast<double>();
    }

    // ---

    public override int readVarInt(SerializationContext ctx) {
        return this.readInt(ctx);
    }

    public override long readVarLong(SerializationContext ctx) {
        return this.readLong(ctx);
    }

    // ---

    public override bool readBoolean(SerializationContext ctx) {
        return this.getValue().cast<bool>();
    }

    public override string readString(SerializationContext ctx) {
        return this.getValue().cast<string>();
    }

    public override byte[] readBytes(SerializationContext ctx) {
        return this.getValue().cast<byte[]>();
    }

    public override V? readOptional<V>(SerializationContext ctx, Endec<V> endec) where V : default {
        var optional = this.getValue().cast<EdmElement?>();
        return optional is not null 
                ? this.frame(() => optional!, () => endec.decode(ctx, this)) 
                : default;
    }

    // ---

    public override SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec) {
        return new Sequence<E>(this, ctx, elementEndec, this.getValue().cast<IList<EdmElement>>());
    }

    public override MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec) {
        return new Map<V>(this, ctx, valueEndec, this.getValue().cast<IDictionary<string, EdmElement>>());
    }

    public override StructDeserializer structed() {
        return new Struct(this, this.getValue().cast<IDictionary<string, EdmElement>>());
    }

    // ---

    public void readAny<S>(SerializationContext ctx, Serializer<S> visitor) where S : class {
        visit(ctx, visitor, getValue());
    }
    
    private void visit<S>(SerializationContext ctx, Serializer<S> visitor, EdmElement value) where S : class {
        var type = value.type;
        
        if (type == EdmElementType.I8) visitor.writeByte(ctx, value.cast<byte>());
        else if (type == EdmElementType.I16) visitor.writeShort(ctx, value.cast<short>());
        else if (type == EdmElementType.I32) visitor.writeInt(ctx, value.cast<int>());
        else if (type == EdmElementType.I64) visitor.writeLong(ctx, value.cast<long>());
        else if (type == EdmElementType.F32) visitor.writeFloat(ctx, value.cast<float>());
        else if (type == EdmElementType.F64) visitor.writeDouble(ctx, value.cast<double>());
        else if (type == EdmElementType.BOOLEAN) visitor.writeBoolean(ctx, value.cast<bool>());
        else if (type == EdmElementType.STRING) visitor.writeString(ctx, value.cast<string>());
        else if (type == EdmElementType.BYTES) visitor.writeBytes(ctx, value.cast<byte[]>());
        else if (type == EdmElementType.OPTIONAL) {
            visitor.writeOptional(ctx, Endec.of<EdmElement>(visit, (_, _) => null), value.cast<EdmElement?>());
        } 
        else if (type == EdmElementType.SEQUENCE) {
            var edmList = value.cast<List<EdmElement>>();
            
            using var sequence = visitor.sequence(ctx, Endec.of<EdmElement>(visit, (_, _) => null), edmList.Count);
            
            edmList.ForEach(sequence.element);
        } 
        else if (type == EdmElementType.MAP) {
            var edmElements = value.cast<IDictionary<string, EdmElement>>();
            
            using var map = visitor.map(ctx, Endec.of<EdmElement>(visit, (_, _) => null), edmElements.Count);
            
            foreach (var entry in edmElements) map.entry(entry.Key, entry.Value);
        }
        else {
            throw new ArgumentOutOfRangeException($"{nameof(value.type)} type can not be handled in EdmDeserializer.visit method");
        }
    }
    
    

    // ---

    internal class Sequence<V> : SequenceDeserializer<V> {

        private readonly EdmDeserializer deserializer;
        private readonly SerializationContext ctx;
        private readonly Endec<V> valueEndec;
        private readonly IndexedEnumerator<EdmElement> elements;
        private readonly int size;
    
        public Sequence(EdmDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, IList<EdmElement> elements) {
            this.deserializer = deserializer;
            this.ctx = ctx;
            this.valueEndec = valueEndec;
    
            this.elements = elements.GetIndexedEnumerator();
            this.size = elements.Count();
        }
        
        public int estimatedSize() {
            return this.size;
        }

        public bool MoveNext() {
            return this.elements.MoveNext();
        }

        public V Current {
            get {
                var element = this.elements.Current;
                return deserializer.frame(
                        () => element,
                        () => this.valueEndec.decode(this.ctx.pushIndex(this.elements.index), deserializer)
                );
            }
        }
        
        public void Reset() {
            this.elements.Reset();
        }

        public void Dispose() {
            this.elements.Dispose();
        }
    }

    internal class Map<V> : MapDeserializer<V> {

        private readonly EdmDeserializer deserializer;
        private readonly SerializationContext ctx;
        private readonly Endec<V> valueEndec;
        private readonly IEnumerator<KeyValuePair<string, EdmElement>> entries;
        private readonly int size;
        
        public Map(EdmDeserializer deserializer, SerializationContext ctx, Endec<V> valueEndec, IDictionary<string, EdmElement> entries) {
            this.deserializer = deserializer;
            this.ctx = ctx;
            this.valueEndec = valueEndec;

            this.entries = entries.GetEnumerator();
            this.size = entries.Count;
        }
        
        public int estimatedSize() {
            return this.size;
        }

        public bool MoveNext() {
            return this.entries.MoveNext();
        }

        public KeyValuePair<string, V> Current {
            get {
                var entry = this.entries.Current;
                return deserializer.frame(
                        () => entry.Value,
                        () => new KeyValuePair<string, V>(entry.Key, this.valueEndec.decode(this.ctx, deserializer))
                );
            }
        }
        
        public void Reset() {
            this.entries.Reset();
        }

        public void Dispose() {
            this.entries.Dispose();
        }
    }

    internal class Struct : StructDeserializer {
    
        private readonly EdmDeserializer deserializer;
        private readonly IDictionary<string, EdmElement> map;
        
        public Struct(EdmDeserializer deserializer, IDictionary<string, EdmElement> map) {
            this.deserializer = deserializer;
            this.map = map;
        }
        
        public F field<F>(String name, SerializationContext ctx, Endec<F> endec, Func<F>? defaultValueFactory) {
            var element = this.map[name];
            if (element == null) {
                if(defaultValueFactory == null) {
                    throw new Exception("Field '" + name + "' was missing from serialized data, but no default value was provided");
                }
    
                return defaultValueFactory();
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


