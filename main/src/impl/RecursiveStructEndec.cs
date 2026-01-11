using System;

namespace io.wispforest.endec.impl;

public class RecursiveStructEndec<T> : StructEndec<T> {
    public readonly StructEndec<T> structEndec;

    public RecursiveStructEndec(Func<StructEndec<T>, StructEndec<T>> builder) {
        structEndec = builder(this);
    }
    
    public override void encodeStruct<E>(SerializationContext ctx, Serializer<E> serializer, StructSerializer instance, T value) where E : class {
        structEndec.encodeStruct(ctx, serializer, instance, value);
    }
    
    public override T decodeStruct<E>(SerializationContext ctx, Deserializer<E> deserializer, StructDeserializer instance) where E : class {
        return structEndec.decodeStruct(ctx, deserializer, instance);
    }
}