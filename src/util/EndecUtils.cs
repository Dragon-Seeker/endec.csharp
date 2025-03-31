using System;
using System.Collections.Generic;
using System.Numerics;
using io.wispforest.impl;

namespace io.wispforest.util;

public class EndecUtils {
    
    public static IntFunction<IDictionary<K, V>> DefaultDictionary<K, V>() => i => new Dictionary<K, V>(i);
    
    public static Endec<T> of<T>(Encoder<T> encoder, Decoder<T> decoder) {
        return new EndecImpl<T>(encoder, decoder);
    }
    
    static Endec<T> recursive<T>(Func<Endec<T>, Endec<T>> builderFunc) {
        return new RecursiveEndec<T>(builderFunc);
    }
    
    public static StructEndec<T> unit<T>(T instance) {
        return unit(() => instance);
    }
    
    public static StructEndec<T> unit<T>(Func<T> instanceGetter) {
        return StructEndecUtils.of((_, _, _, _) => {}, (_, _, _) => instanceGetter());
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
                map.Add(stringToKey(entry.Key), entry.Value);
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
    public static Endec<E> forEnum<E>() where E : class, Enum {
        return forEnum<E>(arg => Enum.GetName(typeof(E), arg));
    }
    #endif
    
    
    
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
    public static Endec<E> forEnum<E>(Func<E, string?> nameLookup) where E : class, Enum {
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
                Endecs.VAR_INT.xmap(ordinal => enumValues.GetValue(ordinal) as E, value => entryToIndex[value])
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
        return StructEndecUtils.of((ctx, serializer, instance, value) => {
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
        return StructEndecUtils.of((ctx, _, instance, value) => {
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
        return StructEndecUtils.of((ctx, serializer, instance, value) => {
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