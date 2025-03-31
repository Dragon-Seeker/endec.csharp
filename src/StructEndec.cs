using System;
using io.wispforest.impl;

namespace io.wispforest;

public delegate void StructuredEncoder<T>(SerializationContext ctx, Serializer<dynamic> serializer, StructSerializer instance, T value);

public delegate T StructuredDecoder<T>(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance);

public delegate T StructuredDecoderWithError<T>(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance, Exception exception);

/**
 * Marker and template interface for all endecs which serialize structs
 * <p>
 * Every such endec should extend this interface to profit from the implementation of {@link #mapCodec(SerializationAttribute...)}
 * and composability which allows {@link Endec#dispatchedStruct(Function, Function, Endec, String)} to work
 */
public abstract class StructEndec<T> : Endec<T> {

    public abstract void encodeStruct<E>(SerializationContext ctx, Serializer<E> serializer, StructSerializer instance, T value) where E : class;

    public abstract T decodeStruct<E>(SerializationContext ctx, Deserializer<E> deserializer, StructDeserializer instance) where E : class;
    
    public override void encode<E>(SerializationContext ctx, Serializer<E> serializer, T value) where E : class {
        using (var instance = serializer.structed()) {
            encodeStruct(ctx, serializer, instance, value);
        }
    }
    
    public override T decode<E>(SerializationContext ctx, Deserializer<E> deserializer) where E : class {
        return decodeStruct(ctx, deserializer, deserializer.structed());
    }

    public StructField<S, T> flatFieldOf<S>(Func<S, T> getter) {
        return new FlatStructField<S, T>(this, getter);
    }
    
    public StructField<M, T> flatInheritedFieldOf<M>() where M : T {
        return new FlatStructField<M, T>(this, m => m);
    }
    
    public StructEndec<R> xmap<R>(Func<T, R> to, Func<R, T> from) {
        return StructEndecUtils.of(
                (ctx, serializer, instance, value) => encodeStruct(ctx, serializer, instance, from(value)),
                (ctx, deserializer, instance) => to(decodeStruct(ctx, deserializer, instance))
        );
    }
    
    public StructEndec<R> xmapWithContext<R>(Func<SerializationContext, T, R> to, Func<SerializationContext, R, T> from) {
        return StructEndecUtils.of(
                (ctx, serializer, instance, value) => encodeStruct(ctx, serializer, instance, from(ctx, value)),
                (ctx, deserializer, instance) => to(ctx, decodeStruct(ctx, deserializer, instance))
        );
    }

    public StructEndec<T> structuredCatchErrors(StructuredDecoderWithError<T> decodeOnError) {
        return StructEndecUtils.of(encodeStruct, (ctx, deserializer, instance) => {
            try {
                return deserializer.tryRead(deserializer1 => decodeStruct(ctx, deserializer1, instance));
            } catch (Exception e) {
                return decodeOnError(ctx, deserializer, instance, e);
            }
        });
    }
    
    public StructEndec<T> validate(Action<T> validator) {
        return this.xmap(t => {
            validator(t);
            return t;
        }, t => {
            validator(t);
            return t;
        });
    }
}