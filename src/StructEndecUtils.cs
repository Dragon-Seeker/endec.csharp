using System;
using io.wispforest.impl;

namespace io.wispforest;

public class StructEndecUtils {
    /**
     * Static constructor for {@link StructEndec} for use when base use of such is desired, it is recommended that
     * you use {@link StructEndecBuilder} as encoding and decoding of data must be kept
     * in the same order with same field names used across both encoding and decoding or issues may arise for
     * formats that are not Self Describing.
     */
    public static StructEndec<T> of<T>(StructuredEncoder<T> encoder, StructuredDecoder<T> decoder) {
        return new StructEndecImpl<T>(encoder, decoder);
    }
    
    static StructEndec<T> recursive<T>(Func<StructEndec<T>, StructEndec<T>> builderFunc) {
        return new RecursiveStructEndec<T>(builderFunc);
    }
    
    public static AttributeStructEndecBuilder<T> ifAttr<T>(SerializationAttribute attribute, StructEndec<T> endec) {
        return new AttributeStructEndecBuilder<T>(endec, attribute);
    }
}