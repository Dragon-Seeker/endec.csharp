using System;

namespace io.wispforest.impl;

public class StructField<S, F> {
    protected readonly string _name;
    protected readonly Endec<F> _endec;
    protected readonly Func<S, F> _getter;
    protected readonly Func<F>? _defaultValueFactory;

    public StructField(String name, Endec<F> endec, Func<S, F> getter, Func<F> defaultValueFactory) {
        this._name = name;
        this._endec = endec;
        this._getter = getter;
        this._defaultValueFactory = defaultValueFactory;
    }

    public StructField(String name, Endec<F> endec, Func<S, F> getter, F? defaultValue) : this(name, endec, getter, () => defaultValue) { }

    public StructField(String name, Endec<F> endec, Func<S, F> getter) : this(name, endec, getter, null) { }

    public virtual void encodeField(SerializationContext ctx, Serializer<dynamic> serializer, StructSerializer instance, S obj) {
        try {
            instance.field(_name, ctx, _endec, _getter(obj), _defaultValueFactory != null);
        } catch (Exception e) {
            throw new StructFieldException("Exception occurred when encoding a given StructField: [Field: " + _name + "]", e);
        }
    }

    public virtual F decodeField(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance) {
        try {
            return instance.field(_name, ctx, _endec, _defaultValueFactory);
        } catch (Exception e) {
            throw new StructFieldException("Exception occurred when decoding a given StructField: [Field: " + _name + "]", e);
        }
    }
}

public sealed class FlatStructField<S, F> : StructField<S, F> {

    public FlatStructField(StructEndec<F> endec, Func<S, F> getter) : base("", endec, getter, null){ }

    public override void encodeField(SerializationContext ctx, Serializer<dynamic> serializer, StructSerializer instance, S obj) {
        (_endec as StructEndec<F>).encodeStruct(ctx, serializer, instance, _getter(obj));
    }

    public override F decodeField(SerializationContext ctx, Deserializer<dynamic> deserializer, StructDeserializer instance) {
        return (_endec as StructEndec<F>).decodeStruct(ctx, deserializer, instance);
    }
}

public class StructFieldException : Exception {
    public StructFieldException(String message, Exception cause) : base(message, cause) { }
}