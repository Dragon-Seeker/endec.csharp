using System;

namespace io.wispforest.endec.impl;

public abstract class StructFieldBase<S, F> {
    public abstract void encodeField(SerializationContext ctx, Serializer<dynamic> serializer, StructSerializer instance, S obj);

    public abstract F decodeField(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance);
}

public abstract class StructField<S, F> : StructFieldBase<S, F>;

public sealed class StructFieldImpl<S, F>(string name, Endec<F> endec, Func<S, F> getter) : StructField<S, F> {
    public override void encodeField(SerializationContext ctx, Serializer<dynamic> serializer, StructSerializer instance, S obj) {
        StructFieldUtils.catchEncode(ctx, name, () => instance.field(name, ctx, endec, getter(obj), false));
    }

    public override F decodeField(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance) {
        return StructFieldUtils.catchDecode(ctx, name, () => instance.field(name, ctx, endec));
    }
}

public sealed class NullStructStructField<S, F>(string name, Endec<F?> endec, Func<S, F?> getter) : StructField<S, F?> where F: struct {
    public override void encodeField(SerializationContext ctx, Serializer<dynamic> serializer, StructSerializer instance, S obj) {
        StructFieldUtils.catchEncode(ctx, name, () => instance.field(name, ctx, endec, getter(obj), true));
    }

    public override F? decodeField(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance) {
        return StructFieldUtils.catchDecode(ctx, name, () => instance.field(name, ctx, endec));
    }
}

public sealed class NullClassStructField<S, F>(string name, Endec<F?> endec, Func<S, F?> getter) : StructField<S, F?> where F: class {
    public override void encodeField(SerializationContext ctx, Serializer<dynamic> serializer, StructSerializer instance, S obj) {
        StructFieldUtils.catchEncode(ctx, name, () => instance.field(name, ctx, endec, getter(obj), true));
    }

    public override F? decodeField(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance) {
        return StructFieldUtils.catchDecode(ctx, name, () => instance.field(name, ctx, endec, () => null));
    }
}

public sealed class DefaultedStructStructField<S, F>(string name, Endec<F?> endec, Func<S, F> getter, Func<F>? defaultValueGetter = null) : StructField<S, F> where F : struct {
    private readonly Func<F> _defaultValueGetter = defaultValueGetter ?? (() => default);

    public override void encodeField(SerializationContext ctx, Serializer<dynamic> serializer, StructSerializer instance, S obj) {
        StructFieldUtils.catchEncode(ctx, name, () => instance.field(name, ctx, endec, getter(obj), true));
    }

    public override F decodeField(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance) {
        return StructFieldUtils.catchDecode(ctx, name, () => instance.field(name, ctx, endec, (() => null))) ?? _defaultValueGetter();
    }
}

public sealed class DefaultedClassStructField<S, F>(string name, Endec<F?> endec, Func<S, F> getter, Func<F> defaultValueGetter) : StructField<S, F> where F: class {
    public override void encodeField(SerializationContext ctx, Serializer<dynamic> serializer, StructSerializer instance, S obj) {
        StructFieldUtils.catchEncode(ctx, name, () => instance.field(name, ctx, endec, getter(obj), true));
    }

    public override F decodeField(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance) {
        return StructFieldUtils.catchDecode(ctx, name, () => instance.field(name, ctx, endec, defaultValueGetter))!;
    }
}

public sealed class FlatStructField<S, F>(StructEndec<F> endec, Func<S, F> getter) : StructField<S, F> {
    public override void encodeField(SerializationContext ctx, Serializer<dynamic> serializer, StructSerializer instance, S obj) {
        endec.encodeStruct(ctx, serializer, instance, getter(obj));
    }

    public override F decodeField(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance) {
        return endec.decodeStruct(ctx, deserializer, instance);
    }
}

public class StructFieldException(string message, Exception cause) : Exception(message, cause);

internal static class StructFieldUtils {
    public static void catchEncode(SerializationContext ctx, String name, Action action) {
        try {
            action();
        } catch (Exception e) {
            throw ctx.pushField(name).exceptionWithTrace<StructFieldException>((trace) => {
                throw new StructFieldException("Exception occurred when encoding a given StructField: " + trace.ToString(), e);
            });
        }
    }
    public static F catchDecode<F>(SerializationContext ctx, String name, Func<F> func) {
        try {
            return func();
        } catch (Exception e) {
            throw ctx.pushField(name).exceptionWithTrace<StructFieldException>((trace) => {
                throw new StructFieldException("Exception occurred when decoding a given StructField: " + trace.ToString(), e);
            });
        }
    }
}