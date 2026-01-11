using System;
using System.Collections.Generic;
using System.Linq;
using io.wispforest.endec.impl;
using io.wispforest.endec.util;

namespace io.wispforest.endec;

public delegate T IntFunction<out T>(int size);

public delegate void Encoder<in T>(SerializationContext ctx, Serializer<dynamic> serializer, T value);

public delegate T Decoder<out T>(SerializationContext ctx, Deserializer<dynamic> deserializer);

public delegate T DecoderWithError<out T>(SerializationContext ctx, Deserializer<dynamic> deserializer, Exception exception);

/// <summary>
/// A combined <b>en</b>coder and <b>dec</b>oder for values of type {@code T}.
/// <para/>
/// To convert between single instances of {@code T} and their serialized form,
/// use <see cref="encodeFully{E}(io.wispforest.SerializationContext,System.Func{io.wispforest.Serializer{E}},T)"/>
/// and <see cref="decodeFully{E}(io.wispforest.SerializationContext,System.Func{E,io.wispforest.Deserializer{E}},E)"/>
/// </summary>
public abstract class Endec<T> : Endec {
    
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
        return Endec.of<IList<T>>((ctx, serializer, list) => {
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
        return mapOf(Endec.DefaultDictionary<String, T>());
    }
    
    public Endec<M> mapOf<M>(IntFunction<M> mapConstructor) where M : IDictionary<String, T> {
        return Endec.of<M>((ctx, serializer, map) => {
            using (var mapState = serializer.map(ctx, this, map.Count)) {
                foreach (var entry in map) {
                    mapState.entry(entry.Key, entry.Value);
                }
            }
        }, (ctx, deserializer) => {
            var mapState = deserializer.map(ctx, this);

            var map = mapConstructor(mapState.estimatedSize());
            foreach (var entry in mapState) {
                map[entry.Key] = entry.Value;
            }

            return map;
        });
    }

    public Endec<IDictionary<K, T>> mapOf<K>(Func<K, String> keyToString, Func<String, K> stringToKey) {
        return mapOf(Endec.DefaultDictionary<K, T>(), keyToString, stringToKey);
    }

    public Endec<M> mapOf<K, M>(IntFunction<M> mapConstructor, Func<K, String> keyToString, Func<String, K> stringToKey) where M : IDictionary<K, T> {
        return Endec.map(mapConstructor, keyToString, stringToKey, this);
    }

    /// <summary>
    /// Create a new endec which serializes an optional value
    /// serialized using this endec
    /// </summary>
    public Endec<T?> optionalOf() {
        return Endec.of(
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
        return Endec.of(
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
        return Endec.of(
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
        return Endec.of(encode, (ctx, deserializer) => {
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
        return StructEndec.of(
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

public interface Endec {
    
    internal static IntFunction<IDictionary<K, V>> DefaultDictionary<K, V>() => i => new Dictionary<K, V>(i);
    
    public static Endec<T> of<T>(Encoder<T> encoder, Decoder<T> decoder) {
        return new EndecImpl<T>(encoder, decoder);
    }
    
    public static Endec<T> recursive<T>(Func<Endec<T>, Endec<T>> builderFunc) {
        return new RecursiveEndec<T>(builderFunc);
    }
    
    public static StructEndec<T> unit<T>(T instance) {
        return unit(() => instance);
    }
    
    public static StructEndec<T> unit<T>(Func<T> instanceGetter) {
        return StructEndec.of((_, _, _, _) => {}, (_, _, _) => instanceGetter());
    }

    /// <summary>
    /// Create a new endec which serializes a map from keys serialized using
    /// <c>keyEndec</c> to values serialized using <c>valueEndec</c>.
    /// <para/>
    /// Due to the endec data model only natively supporting maps
    /// with string keys, the resulting endec's serialized representation
    /// is a list of key-value pairs
    /// </summary>
    public static Endec<IDictionary<K, V>> map<K, V>(Endec<K> keyEndec, Endec<V> valueEndec) {
        return StructEndecBuilder.of(
                keyEndec.fieldOf<KeyValuePair<K, V>>("k", s => s.Key),
                valueEndec.fieldOf<KeyValuePair<K, V>>("v", s => s.Value),
                (k, v) => new KeyValuePair<K, V>(k, v)
        ).listOf().xmap<IDictionary<K, V>>(entries => {
            var map = new Dictionary<K, V>(entries.Count);

            map.AddAll(entries);
            
            return map;
        }, kvMap => new List<KeyValuePair<K, V>>(kvMap));
    }

    public static Endec<IDictionary<K, V>> map<K, V>(Func<K, String> keyToString, Func<String, K> stringToKey, Endec<V> valueEndec) {
        return map(DefaultDictionary<K, V>(), keyToString, stringToKey, valueEndec);
    }
    
    /// <summary>
    /// Create a new endec which serializes a map from keys encoded as strings using
    /// {@code keyToString} and decoded using {@code stringToKey} to values serialized
    /// using {@code valueEndec}
   /// </summary>
    public static Endec<M> map<K, V, M>(IntFunction<M> mapConstructor, Func<K, String> keyToString, Func<String, K> stringToKey, Endec<V> valueEndec) where M : IDictionary<K, V> {
        return of((ctx, serializer, map) => {
            using (var mapState = serializer.map(ctx, valueEndec, map.Count)) {
                foreach (var entry in map) {
                    mapState.entry(keyToString(entry.Key), entry.Value);
                }
            }
        }, (ctx, deserializer) => {
            var mapState = deserializer.map(ctx, valueEndec);

            var map = mapConstructor(mapState.estimatedSize());
            
            foreach (var entry in mapState) {
                map[stringToKey(entry.Key)] = entry.Value;
            }

            return map;
        });
    }
    
    /// <summary>
    /// Create a new endec which serializes the enum constants of {@code enumClass}
    /// <para/>
    /// In a human-readable format, the endec serializes to the <see cref="Enum.GetName"/> constant's name},
    /// and to its ordinal otherwise
    /// </summary>
    #if NET7_OR_HIGHER // etc, if needed.
    public static Endec<E> forEnum<E>() where E : struct, Enum {
        return forEnum<E>(Enum.GetName);
    }
    #else
    public static Endec<E> forEnum<E>() where E : Enum {
        return forEnum<E>(arg => Enum.GetName(typeof(E), arg));
    }
    #endif

    public enum Test {
        One = 1,
        Two = 2
    }

    public static void test() {
        var endec = forEnum<Test>();
    }
    
    /// <summary>
    /// Create a new endec which serializes the enum constants of {@code enumClass}
    /// <para/>
    /// In a human-readable format, the endec serializes to the {@linkplain Enum#name() constant's name},
    /// and to its ordinal otherwise
    /// </summary>
    #if NET7_OR_HIGHER // etc, if needed.
    public static Endec<E> forEnum<E>(Func<E, string?> nameLookup) where E : struct, Enum {
        var enumValues = Enum.GetValues<E>();
        var serializedNames = new Dictionary<string, E?>();
        var entryToIndex = new Dictionary<E, int>();

        int i = 0;
        
        foreach (E enumValue in enumValues) {
            serializedNames[nameLookup(enumValue)!] = enumValue;
            entryToIndex[enumValue] = i;
            i++;
        }

        var type = typeof(E);
    
        return ifAttr(
            SerializationAttributes.HUMAN_READABLE,
            Endecs.STRING.xmap<E>(name => {
                var entry = serializedNames[name];

                if (entry is null) throw new Exception($"{type.Name} constant with the name of [{name}] could not be located!");

                return entry.Value;
            }, value => {
                var name = nameLookup(value);
                
                if (name is null) throw new Exception($"{type.Name} constant with the value of [{value}] could not be located!");

                return name;
            })
        ).orElse(
                Endecs.VAR_INT.xmap(ordinal => enumValues[ordinal], value => entryToIndex[value])
        );
    }
    #else
    public static Endec<E> forEnum<E>(Func<E, string?> nameLookup) where E : Enum {
        var enumValues = Enum.GetValues(typeof(E));
        var serializedNames = new Dictionary<string, E?>();
        var entryToIndex = new Dictionary<E, int>();

        int i = 0;
        
        foreach (E enumValue in enumValues) {
            serializedNames[nameLookup(enumValue)!] = enumValue;
            entryToIndex[enumValue] = i;
            i++;
        }

        var type = typeof(E);
    
        return ifAttr(
                SerializationAttributes.HUMAN_READABLE,
                Endecs.STRING.xmap<E>(name => {
                    var entry = serializedNames[name];
    
                    if (entry is null) throw new Exception($"{type.Name} constant with the name of [{name}] could not be located!");
    
                    return entry;
                }, value => {
                    var name = nameLookup(value);
                    
                    if (name is null) throw new Exception($"{type.Name} constant with the value of [{value}] could not be located!");

                    return name;
                })
        ).orElse(
                Endecs.VAR_INT.xmap(ordinal => (E) enumValues.GetValue(ordinal), value => entryToIndex[value])
        );
    }
    #endif
    // -- 
    
    /// <summary>
    /// Shorthand for <see cref="dispatchedStruct{T,K}(System.Func{K,io.wispforest.StructEndec{T}},System.Func{T,K},io.wispforest.Endec{K}, string)"/>
    /// which always uses <c>type</c> as the <c>variantKey</c>
    /// </summary>
    public static StructEndec<T> dispatchedStruct<T, K>(Func<K, StructEndec<T>> variantToEndec, Func<T, K> instanceToVariant, Endec<K> variantEndec) {
        return dispatchedStruct(variantToEndec, instanceToVariant, variantEndec, "type");
    }
    
    /// <summary>
    /// Create a new struct-dispatch endec which serializes variants of the struct <typeparamref name="T"/>
    /// <para/>
    /// To do this, it inserts an additional field given by <c>variantKey</c> into the beginning of the
    /// struct and writes the variant identifier obtained from <c>instanceToVariant</c> into it
    /// using <c>variantEndec</c>. When decoding, this variant identifier is read and the rest
    /// of the struct decoded with the endec obtained from <c>variantToEndec</c>
    /// <para/>
    /// For example, assume there is some interface like this
    /// <code>
    /// public interface Herbert {
    ///      Identifier id();
    ///      ... more functionality here
    /// }
    /// </code>
    ///
    /// which is implemented by <c>Harald</c> and <c>Albrecht</c>, whose endecs we have
    /// stored in a map:
    /// <code>
    /// public final class Harald implements Herbert {
    ///      public static final StructEndec&lt;Harald&gt; = StructEndecBuilder.of(...);
    ///
    ///      private final int haraldOMeter;
    ///      ...
    /// }
    ///
    /// public final class Albrecht implements Herbert {
    ///     public static final StructEndec&lt;Harald&gt; = StructEndecBuilder.of(...);
    ///
    ///     private final List&lt;String&gt; dadJokes;
    ///      ...
    /// }
    ///
    /// public static final Map&lt;Identifier, StructEndec&lt;? extends Herbert&gt;&gt; HERBERT_REGISTRY = Map.of(
    ///      new Identifier("herbert", "harald"), Harald.ENDEC,
    ///      new Identifier("herbert", "albrecht"), Albrecht.ENDEC
    /// );
    /// </code>
    ///
    /// We could then create an endec capable of serializing either <c>Harald</c> or <c>Albrecht</c> as follows:
    /// <code>
    /// Endec.dispatchedStruct(HERBERT_REGISTRY::get, Herbert::id, BuiltInEndecs.IDENTIFIER, "type")
    /// </code>
    ///
    /// If we now encode an instance of <c>Albrecht</c> to JSON using this endec, we'll get the following result:
    /// <code>
    /// {
    ///      "type": "herbert:albrecht",
    ///      "dad_jokes": [
    ///          "What does a sprinter eat before a race? Nothing, they fast!",
    ///          "Why don't eggs tell jokes? They'd crack each other up."
    ///      ]
    /// }
    /// </code>
    ///
    /// And similarly, the following data could be used for decoding an instance of {@code Harald}:
    /// <code>
    /// {
    ///      "type": "herbert:harald",
    ///      "harald_o_meter": 69
    /// }
    /// </code>
    /// </summary>
    public static StructEndec<T> dispatchedStruct<T, K>(Func<K, StructEndec<T>> variantToEndec, Func<T, K> instanceToVariant, Endec<K> variantEndec, String variantKey) {
        return StructEndec.of((ctx, serializer, instance, value) => {
            var variant = instanceToVariant(value);
            instance.field(variantKey, ctx, variantEndec, variant);
            
            variantToEndec(variant).encodeStruct(ctx, serializer,  instance, value);
        }, (ctx, deserializer, instance) => {
            var variant = instance.field(variantKey, ctx, variantEndec);
            return variantToEndec(variant).decodeStruct(ctx, deserializer, instance);
        });
    }
    
    /// <summary>
    /// Create a new dispatch endec which serializes variants of <typeparamref name="T"/>
    /// <para/>
    /// Such an endec is conceptually similar to a struct-dispatch one created through <see cref="dispatchedStruct{T,K}(System.Func{K,io.wispforest.StructEndec{T}},System.Func{T,K},io.wispforest.Endec{K}, string)"/>
    /// (check the documentation on that function for a complete usage example), but because this family of endecs does not
    /// require <typeparamref name="T"/> to be a struct, the variant identifier field cannot be merged with the rest and is encoded separately
    /// </summary>
    public static StructEndec<T> dispatched<T, K>(Func<K, Endec<T>> variantToEndec, Func<T, K> instanceToVariant, Endec<K> variantEndec) {
        return StructEndec.of((ctx, _, instance, value) => {
            var variant = instanceToVariant(value);
            instance.field("variant", ctx, variantEndec, variant);

            //noinspection unchecked
            instance.field("instance", ctx, variantToEndec(variant), value);
        }, (ctx, _, instance) => {
            var variant = instance.field("variant", ctx, variantEndec);
            return instance.field("instance", ctx, variantToEndec(variant));
        });
    }
    
    public static StructEndec<T> dispatchedFlatable<T, K>(String variantKey, String instanceKey, Func<K, Endec<T>> variantToEndec, Func<T, K> instanceToVariant, Endec<K> variantEndec) {
        return StructEndec.of((ctx, serializer, instance, value) => {
            var variant = instanceToVariant(value);
            instance.field("variant", ctx, variantEndec, variant);

            //noinspection unchecked
            var instanceEndec = variantToEndec(variant);

            if (instanceEndec is StructEndec<T> instanceStructEndec) {
                instanceStructEndec.encodeStruct(ctx, serializer, instance, value);
            } else {
                instance.field("instance", ctx, instanceEndec, value);
            }
        }, (ctx, deserializer, instance) => {
            var variant = instance.field("variant", ctx, variantEndec);
            
            var instanceEndec = variantToEndec(variant);
            
            return (instanceEndec is StructEndec<T> instanceStructEndec) 
                ? instanceStructEndec.decodeStruct(ctx, deserializer, instance) 
                : instance.field("instance", ctx, instanceEndec);
        });
    }

    // ---

    public static AttributeEndecBuilder<T> ifAttr<T>(SerializationAttribute attribute, Endec<T> endec) {
        return new AttributeEndecBuilder<T>(endec, attribute);
    }

    // ---
    
    #if NET7_0 || NET8_0 || NET9_0 // etc, if needed.
    public static Endec<N> clampedMax<N>(Endec<N> endec, N max) where N : IComparable, IMinMaxValue<N> {
        return clamped(endec, N.MinValue, max);
    }
    
    public static Endec<N> rangedMax<N>(Endec<N> endec, N max, bool throwError) where N : IComparable, IMinMaxValue<N>  {
        return ranged(endec, N.MinValue, max, throwError);
    }
    
    public static Endec<N> clampedMin<N>(Endec<N> endec, N min) where N : IComparable, IMinMaxValue<N>  {
        return clamped(endec, min, N.MaxValue);
    }
    
    public static Endec<N> rangedMin<N>(Endec<N> endec, N min, bool throwError) where N : IComparable, IMinMaxValue<N> {
        return ranged(endec, min, N.MaxValue, throwError);
    }
    
    public static Endec<N> clamped<N>(Endec<N> endec, N? min, N? max) where N : IComparable, IMinMaxValue<N> {
        return ranged(endec, min, max, false);
    }

    public static Endec<N> ranged<N>(Endec<N> endec, N? min, N? max, bool throwError) where N : IComparable, IMinMaxValue<N> {
        Func<N, N> errorChecker = n => {
            // 1st check if the given min value exist and then compare similar to: [n < min]
            // 2nd check if the given min value exist and then compare similar to: [n > max]
            if (min != null && n.CompareTo(min) < 0) {
                if(throwError) throw RangeNumberException(n, min, max);
                return min;
            } else if (max != null && n.CompareTo(max) > 0) {
                if(throwError) throw RangeNumberException(n, min, max);
                return max;
            }
            return n;
        };
    
        return endec.xmap(errorChecker, errorChecker);
    }

    private static ArgumentOutOfRangeException RangeNumberException(object number, object minValue, object maxValue) {
        return new ArgumentOutOfRangeException($"Number {number} is out of range [{minValue}, {maxValue}].");
    }
    #endif
}
