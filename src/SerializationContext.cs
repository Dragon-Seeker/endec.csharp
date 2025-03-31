using System.Collections.Generic;
using io.wispforest.impl;
using io.wispforest.util;

namespace io.wispforest;

public class SerializationContext {
    private static readonly SerializationContext EMPTY = new (new Dictionary<SerializationAttribute, object>(), new HashSet<SerializationAttribute>());

    private readonly IDictionary<SerializationAttribute, object> attributeValues;
    private readonly ISet<SerializationAttribute> suppressedAttributes;

    private SerializationContext(IDictionary<SerializationAttribute, object> attributeValues, ISet<SerializationAttribute> suppressedAttributes) {
        this.attributeValues = attributeValues.ImmutableWrap();
        this.suppressedAttributes = suppressedAttributes.ImmutableWrap();
    }

    public static SerializationContext empty() {
        return EMPTY;
    }

    public static SerializationContext attributes(params SerializationAttributeInstance[] attributes) {
        if (attributes.Length == 0) return EMPTY;
        return new SerializationContext(unpackAttributes(attributes), new HashSet<SerializationAttribute>());
    }

    public static SerializationContext suppressed(params SerializationAttribute[] attributes) {
        if (attributes.Length == 0) return EMPTY;
        return new SerializationContext(new Dictionary<SerializationAttribute, object>(), new HashSet<SerializationAttribute>(attributes));
    }

    public SerializationContext withAttributes(params SerializationAttributeInstance[] attributes) {
        var newAttributes = unpackAttributes(attributes);
        foreach (var entry in this.attributeValues) {
            if (!newAttributes.ContainsKey(entry.Key)) {
                newAttributes.Add(entry);
            }
        }

        return new SerializationContext(newAttributes, this.suppressedAttributes);
    }

    public SerializationContext withoutAttributes(params SerializationAttribute[] attributes) {
        var newAttributes = new Dictionary<SerializationAttribute, object>(attributeValues);
        foreach (var attribute in attributes) {
            newAttributes.Remove(attribute);
        }

        return new SerializationContext(newAttributes, this.suppressedAttributes);
    }

    public SerializationContext withSuppressed(params SerializationAttribute[] attributes) {
        var newSuppressed = new HashSet<SerializationAttribute>(suppressedAttributes);
        newSuppressed.AddAll(attributes);

        return new SerializationContext(attributeValues, newSuppressed);
    }

    public SerializationContext withoutSuppressed(params SerializationAttribute[] attributes) {
        var newSuppressed = new HashSet<SerializationAttribute>(suppressedAttributes);
        foreach (var attribute in attributes) {
            newSuppressed.Remove(attribute);
        }

        return new SerializationContext(attributeValues, newSuppressed);
    }

    public SerializationContext and(SerializationContext other) {
        var newAttributeValues = new Dictionary<SerializationAttribute, object>(attributeValues);
        newAttributeValues.AddAll(other.attributeValues);

        var newSuppressed = new HashSet<SerializationAttribute>(suppressedAttributes);
        newSuppressed.AddAll(other.suppressedAttributes);

        return new SerializationContext(newAttributeValues, newSuppressed);
    }

    public bool hasAttribute(SerializationAttribute attribute) {
        return attributeValues.ContainsKey(attribute) && !suppressedAttributes.Contains(attribute);
    }
    
    public A getAttributeValue<A>(SerializationAttributeWithValue<A> attribute) {
        return (A) attributeValues[attribute];
    }

    public A requireAttributeValue<A>(SerializationAttributeWithValue<A> attribute) {
        if (!hasAttribute(attribute)) {
            throw new MissingAttributeValueException("Context did not provide a value for attribute '" + attribute.name + "'");
        }

        return getAttributeValue(attribute);
    }

    private static IDictionary<SerializationAttribute, object> unpackAttributes(params SerializationAttributeInstance[] attributes) {
        var attributeValues = new Dictionary<SerializationAttribute, object>();
        foreach (var instance in attributes) {
            attributeValues.Add(instance.attribute(), instance.value());
        }

        return attributeValues;
    }
}