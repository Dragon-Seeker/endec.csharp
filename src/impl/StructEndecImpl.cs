namespace io.wispforest.impl;

public class StructEndecImpl<T> : StructEndec<T> {
    private readonly StructuredEncoder<T> _encoder;
    private readonly StructuredDecoder<T> _decoder;

    public StructEndecImpl(StructuredEncoder<T> encoder, StructuredDecoder<T> decoder) {
        _encoder = encoder;
        _decoder = decoder;
    }

    public override void encodeStruct<E>(SerializationContext ctx, Serializer<E> serializer, StructSerializer instance, T value) where E : class {
        _encoder(ctx, serializer, instance, value);
    }

    public override T decodeStruct<E>(SerializationContext ctx, Deserializer<E> deserializer, StructDeserializer instance) where E : class {
        return _decoder(ctx, deserializer, instance);
    }
}