namespace io.wispforest.endec.impl;

public class EndecImpl<T> : Endec<T> {

    private readonly Encoder<T> _encoder;
    private readonly Decoder<T> _decoder;

    public EndecImpl(Encoder<T> encoder, Decoder<T> decoder) {
        this._encoder = encoder;
        this._decoder = decoder;
    }
    
    public override void encode<E>(SerializationContext ctx, Serializer<E> serializer, T value) where E : class {
        _encoder(ctx, serializer, value);
    }
            
    public override T decode<E>(SerializationContext ctx, Deserializer<E> deserializer) where E : class {
        return _decoder(ctx, deserializer);
    }
    
    
}