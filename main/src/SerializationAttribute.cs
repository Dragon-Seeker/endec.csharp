namespace io.wispforest.endec;

public static class SerializationAttributes {

    /**
     * This format is intended to be human-readable (and potentially -editable)
     * <p>
     * Endecs should use this to make decisions like representing a
     * {@link net.minecraft.util.math.BlockPos} as an integer sequence instead of packing it into a long
     */
    public static readonly SerializationAttributeMarker HUMAN_READABLE = SerializationAttribute.marker("human_readable");
}

public abstract class SerializationAttribute(string name) {
    public readonly string name = name;

    public static SerializationAttributeMarker marker(string name) => new (name);

    public static SerializationAttributeWithValue<T> withValue<T>(string name) => new (name);
}

public sealed class SerializationAttributeMarker : SerializationAttribute, SerializationAttributeInstance {
    internal SerializationAttributeMarker(string name) : base(name) { }

    public SerializationAttribute attribute() => this;
    
    public object value() => default;
}

public sealed class SerializationAttributeWithValue<T> : SerializationAttribute {
    internal SerializationAttributeWithValue(string name) : base(name) { }

    public SerializationAttributeInstance instance(T value) => new InstanceImpl(this, value);
}

public interface SerializationAttributeInstance {
    public SerializationAttribute attribute();
    public object value();
}

internal class InstanceImpl(SerializationAttribute attribute, object value) : SerializationAttributeInstance {
    
    SerializationAttribute SerializationAttributeInstance.attribute() => attribute;

    object SerializationAttributeInstance.value() => value;
}
