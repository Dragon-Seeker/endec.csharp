using System;

namespace io.wispforest.endec.impl;

public class RecursiveEndec<T> : Endec<T> {
    public readonly Endec<T> endec;

    public RecursiveEndec(Func<Endec<T>, Endec<T>> builder) {
        endec = builder(this);
    }

    public override void encode<E>(SerializationContext ctx, Serializer<E> serializer, T value) where E : class {
        endec.encode(ctx, serializer, value);
    }
    
    public override T decode<E>(SerializationContext ctx, Deserializer<E> deserializer) where E : class {
        return endec.decode(ctx, deserializer);
    }
}