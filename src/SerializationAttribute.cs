namespace io.wispforest {
    public class SerializationAttributes {

        /**
         * This format is intended to be human-readable (and potentially -editable)
         * <p>
         * Endecs should use this to make decisions like representing a
         * {@link net.minecraft.util.math.BlockPos} as an integer sequence instead of packing it into a long
         */
        public static readonly SerializationAttributeMarker HUMAN_READABLE = SerializationAttribute.marker("human_readable");
    }
    
    public abstract class SerializationAttribute  {
        public readonly string name;

        protected SerializationAttribute(string name) {
            this.name = name;
        }

        public static SerializationAttributeMarker marker(string name) {
            return new SerializationAttributeMarker(name);
        }

        public static SerializationAttributeWithValue<T> withValue<T>(string name) {
            return new SerializationAttributeWithValue<T>(name);
        }
    }

    public sealed class SerializationAttributeMarker : SerializationAttribute, SerializationAttributeInstance {
        internal SerializationAttributeMarker(string name) : base(name) { }

        public SerializationAttribute attribute() {
            return this;
        }
        
        public object value() {
            return null;
        }
    }

    public sealed class SerializationAttributeWithValue<T> : SerializationAttribute {
        internal SerializationAttributeWithValue(string name) : base(name) { }

        public SerializationAttributeInstance instance(T value) {
            return new InstanceImpl(this, value);
        }
    }

    public interface SerializationAttributeInstance {
        public SerializationAttribute attribute();
        public object value();
    }

    internal class InstanceImpl : SerializationAttributeInstance {

        private readonly SerializationAttribute attribute;
        private readonly object value;

        internal InstanceImpl(SerializationAttribute attribute, object value) {
            this.attribute = attribute;
            this.value = value;
        }
        
        SerializationAttribute SerializationAttributeInstance.attribute() {
            return attribute;
        }

        object SerializationAttributeInstance.value() {
            return value;
        }
    }
}