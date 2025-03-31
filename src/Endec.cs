using System;
using System.Collections.Generic;
using System.Linq;
using io.wispforest.impl;
using io.wispforest.util;

namespace io.wispforest;

public delegate T IntFunction<T>(int size);

public delegate void Encoder<T>(SerializationContext ctx, Serializer<dynamic> serializer, T value);

public delegate T Decoder<T>(SerializationContext ctx, Deserializer<dynamic> deserializer);

public delegate T DecoderWithError<T>(SerializationContext ctx, Deserializer<dynamic> deserializer, Exception exception);

/// <summary>
/// A combined <b>en</b>coder and <b>dec</b>oder for values of type {@code T}.
/// <para/>
/// To convert between single instances of {@code T} and their serialized form,
/// use <see cref="encodeFully{E}(io.wispforest.SerializationContext,System.Func{io.wispforest.Serializer{E}},T)"/>
/// and <see cref="decodeFully{E}(io.wispforest.SerializationContext,System.Func{E,io.wispforest.Deserializer{E}},E)"/>
/// </summary>
public abstract class Endec<T> {
    
    /// <summary>
    /// Write all data required to reconstruct <c>value</c> into <c>serializer</c>
    /// </summary>
    public abstract void encode<E>(SerializationContext ctx, Serializer<E> serializer, T value) where E : class;

    /// <summary>
    /// Decode the data specified by <see cref="encode{E}"/> and reconstruct
    /// the corresponding instance of <typeparamref name="T"/>.
    /// <para/>
    /// Endecs which intend to handle deserialization failure by decoding a different
    /// structure on error, must wrap their initial reads in a call to <see cref="Deserializer{E}.tryRead"/>
    /// to ensure that deserializer state is restored for the subsequent attempt
    /// </summary>
    public abstract T decode<E>(SerializationContext ctx, Deserializer<E> deserializer) where E : class;
    

    // ---

    /// <summary>
    /// Create a new serializer with result type <typeparamref name="E"/>, call <see cref="encode{E}"/>
    /// once for the provided <c>value</c> and return the serializer's <see cref="Serializer{E}.result"/>
    /// </summary>
    public E encodeFully<E>(SerializationContext ctx, Func<Serializer<E>> serializerConstructor, T value) where E : class {
        var serializer = serializerConstructor(); 
        encode(serializer.setupContext(ctx), serializer, value);

        return serializer.result();
    }

    public E encodeFully<E>(Func<Serializer<E>> serializerConstructor, T value) where E : class {
        return encodeFully(SerializationContext.empty(), serializerConstructor, value);
    }

    /// <summary>
    /// Create a new deserializer by calling <c>deserializerConstructor</c> with <c>value</c>
    /// and return the result of <see cref="decode{E}"/>
    /// </summary> 
    public T decodeFully<E>(SerializationContext ctx, Func<E, Deserializer<E>> deserializerConstructor, E value) where E : class {
        var deserializer = deserializerConstructor(value);
        return decode(deserializer.setupContext(ctx), deserializer);
    }
    
    public T decodeFully<E>(Func<E, Deserializer<E>> deserializerConstructor, E value) where E : class {
        return decodeFully(SerializationContext.empty(), deserializerConstructor, value);
    }

    // --- Serializer Primitives ---

    
    // --- Serializer compound types ---

    /// <summary>
    /// Create a new endec which serializes a list of elements
    /// serialized using this endec
    /// </summary> 
    public Endec<IList<T>> listOf() {
        return EndecUtils.of<IList<T>>((ctx, serializer, list) => {
            using (var sequence = serializer.sequence(ctx, this, list.Count)) {
                foreach (var entry in list) {
                    sequence.element(entry);
                }
            }
        }, (ctx, deserializer) => {
            var sequenceState = deserializer.sequence(ctx, this);

            var list = new List<T>(sequenceState.estimatedSize());
            foreach (var entry in sequenceState) {
                list.Add(entry);
            }

            return list;
        });
    }
    
    /// <summary>
    /// Create a new endec which serializes a array of elements
    /// serialized using this endec
    /// </summary> 
    public Endec<T[]> arrayOf() {
        return listOf().xmap<T[]>(list => list.ToArray<T>(), array => new List<T>(array));
    }

    /// <summary>
    /// Create a new endec which serializes a map from string
    /// keys to values serialized using this endec
    /// </summary> 
    public Endec<IDictionary<String, T>> mapOf() {
        return mapOf(EndecUtils.DefaultDictionary<String, T>());
    }
    
    public Endec<M> mapOf<M>(IntFunction<M> mapConstructor) where M : IDictionary<String, T> {
        return EndecUtils.of<M>((ctx, serializer, map) => {
            using (var mapState = serializer.map(ctx, this, map.Count)) {
                foreach (var entry in map) {
                    mapState.entry(entry.Key, entry.Value);
                }
            }
        }, (ctx, deserializer) => {
            var mapState = deserializer.map(ctx, this);

            var map = mapConstructor(mapState.estimatedSize());
            foreach (var entry in mapState) {
                map.Add(entry.Key, entry.Value);
            }

            return map;
        });
    }

    public Endec<IDictionary<K, T>> mapOf<K>(Func<K, String> keyToString, Func<String, K> stringToKey) {
        return mapOf(EndecUtils.DefaultDictionary<K, T>(), keyToString, stringToKey);
    }

    public Endec<M> mapOf<K, M>(IntFunction<M> mapConstructor, Func<K, String> keyToString, Func<String, K> stringToKey) where M : IDictionary<K, T> {
        return EndecUtils.map(mapConstructor, keyToString, stringToKey, this);
    }

    /// <summary>
    /// Create a new endec which serializes an optional value
    /// serialized using this endec
    /// </summary>
    public Endec<T?> optionalOf() {
        return EndecUtils.of(
                (ctx, serializer, value) => serializer.writeOptional(ctx, this, value),
                (ctx, deserializer) => deserializer.readOptional(ctx, this)
        );
    }

    // --- Constructors --

    /// <summary>
    /// Create a new endec which converts between instances of <typeparamref name="T"/> and <typeparamref name="R"/>
    /// using <c>to</c> and <c>from</c> before encoding / after decoding
    /// </summary>
    public Endec<R> xmap<R>(Func<T, R> to, Func<R, T> from) {
        return EndecUtils.of(
                (ctx, serializer, value) => encode(ctx, serializer, from(value)),
                (ctx, deserializer) => to(decode(ctx, deserializer))
        );
    }

    /// <summary>
    /// Create a new endec which converts between instances of <typeparamref name="T"/> and <typeparamref name="R"/>
    /// using <c>to</c> and <c>from</c> before encoding / after decoding, optionally using
    /// the current <c>SerializationContext</c>
    /// </summary>
    public Endec<R> xmapWithContext<R>(Func<SerializationContext, T, R> to, Func<SerializationContext, R, T> from) {
        return EndecUtils.of(
                (ctx, serializer, value) => encode(ctx, serializer, from(ctx, value)),
                (ctx, deserializer) => to(ctx, decode(ctx, deserializer))
        );
    }
    
    /// <summary>
    /// Create a new endec which runs <c>validator</c> (giving it the chance to throw on
    /// an invalid value) before encoding / after decoding
    /// </summary>
    public Endec<T> validate(Action<T> validator) {
        return xmap(t => {
            validator(t);
            return t;
        }, t => {
            validator(t);
            return t;
        });
    }
   
    
    /// <summary>
    /// Create a new endec which, if decoding using this endec's <see cref="decode{E}"/> fails,
    /// instead tries to decode using <c>decodeOnError</c>
    /// </summary>
    public Endec<T> catchErrors(DecoderWithError<T> decodeOnError) {
        return EndecUtils.of(encode, (ctx, deserializer) => {
            try {
                return deserializer.tryRead(deserializer1 => decode(ctx, deserializer1));
            } catch (Exception e) {
                return decodeOnError(ctx, deserializer, e);
            }
        });
    }
    
    /// <summary>
    /// Create a new endec which serializes a set of elements
    /// serialized using this endec as an xmapped list
    /// </summary>
    public Endec<ISet<T>> setOf() {
        return listOf().xmap<ISet<T>>(list => new HashSet<T>(list), set => new List<T>(set));
    }

    // --- Conversion ---

    /// <summary>
    /// Create a new keyed endec which (de)serializes the entry
    /// with key <c>key</c> into/from a <see cref="MapCarrier"/>,
    /// decoding to <c>defaultValue</c> if the map does not contain such an entry
    /// <para/>
    /// If <typeparamref name="T"/> is of a mutable type, you almost always want to use <see cref="keyed(string,System.Func{T})"/> instead
    /// </summary>
    public KeyedEndec<T> keyed(String key, T defaultValue) {
        return new KeyedEndec<T>(key, this, defaultValue);
    }
    
    /// <summary>
    /// Create a new keyed endec which (de)serializes the entry
    /// with key <c>key</c> into/from a <see cref="MapCarrier"/>,
    /// decoding to the result of invoking <c>defaultValueFactory</c> if the map does not contain such an entry
    /// <para/>
    /// If <typeparamref name="T"/> is of an immutable type, you almost always want to use <see cref="keyed(string,T)"/> instead
    /// </summary>
    public KeyedEndec<T> keyed(String key, Func<T> defaultValueFactory) {
        return new KeyedEndec<T>(key, this, defaultValueFactory);
    }

    // ---

    public StructEndec<T> structOf(String name) {
        return StructEndecUtils.of(
                (ctx, _, instance, value) => instance.field(name, ctx, this, value),
                (ctx, _, instance) => instance.field(name, ctx, this));
    }
    
    public StructField<S, T> fieldOf<S>(String name, Func<S, T> getter) {
        return new StructField<S, T>(name, this, getter);
    }
    
    public StructField<S, T> optionalFieldOf<S>(String name, Func<S, T?> getter) {
        return optionalFieldOf(name, getter, null);
    }
    
    public StructField<S, T> optionalFieldOf<S>(String name, Func<S, T?> getter, T? defaultValue) {
        return new StructField<S, T>(name, optionalOf(), getter, defaultValue);
    }
    
    public StructField<S, T> optionalFieldOf<S>(String name, Func<S, T?> getter, Func<T?> defaultValue) {
        if (defaultValue is null) {
            throw new NullReferenceException("Supplier was found to be null which is not permitted for optionalFieldOf");
        }
    
        return new StructField<S, T>(name, optionalOf(), getter, defaultValue);
    }
}
