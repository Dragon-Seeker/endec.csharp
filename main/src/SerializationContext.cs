using System;
using System.Collections.Generic;
using io.wispforest.endec.impl;
using io.wispforest.endec.impl.trace;
using io.wispforest.endec.util;

namespace io.wispforest.endec;

public class SerializationContext {
    private static readonly SerializationContext EMPTY = new (new Dictionary<SerializationAttribute, object>(), new HashSet<SerializationAttribute>(), new EndecTrace());

    private readonly IDictionary<SerializationAttribute, object> attributeValues;
    private readonly ISet<SerializationAttribute> suppressedAttributes;
    private readonly EndecTrace trace;

    private SerializationContext(IDictionary<SerializationAttribute, object> attributeValues, ISet<SerializationAttribute> suppressedAttributes, EndecTrace trace) {
        this.attributeValues = attributeValues.ImmutableWrap();
        this.suppressedAttributes = suppressedAttributes.ImmutableWrap();
        this.trace = trace;
    }

    public static SerializationContext empty() => EMPTY;

    public static SerializationContext attributes(params SerializationAttributeInstance[] attributes) {
        return attributes.Length == 0 
                ? EMPTY 
                : new SerializationContext(unpackAttributes(attributes), new HashSet<SerializationAttribute>(), new EndecTrace());
    }

    public static SerializationContext suppressed(params SerializationAttribute[] attributes) {
        return attributes.Length == 0 
                ? EMPTY 
                : new SerializationContext(new Dictionary<SerializationAttribute, object>(), new HashSet<SerializationAttribute>(attributes), new EndecTrace());
    }

    public SerializationContext withAttributes(params SerializationAttributeInstance[] attributes) {
        var newAttributes = unpackAttributes(attributes);
        foreach (var entry in this.attributeValues) {
            if (!newAttributes.ContainsKey(entry.Key)) newAttributes.Add(entry);
        }

        return new SerializationContext(newAttributes, this.suppressedAttributes, new EndecTrace());
    }

    public SerializationContext withoutAttributes(params SerializationAttribute[] attributes) {
        var newAttributes = new Dictionary<SerializationAttribute, object>(attributeValues);
        foreach (var attribute in attributes) {
            newAttributes.Remove(attribute);
        }

        return new SerializationContext(newAttributes, this.suppressedAttributes, new EndecTrace());
    }

    public SerializationContext withSuppressed(params SerializationAttribute[] attributes) {
        var newSuppressed = new HashSet<SerializationAttribute>(suppressedAttributes);
        newSuppressed.AddAll(attributes);

        return new SerializationContext(attributeValues, newSuppressed, new EndecTrace());
    }

    public SerializationContext withoutSuppressed(params SerializationAttribute[] attributes) {
        var newSuppressed = new HashSet<SerializationAttribute>(suppressedAttributes);
        foreach (var attribute in attributes) {
            newSuppressed.Remove(attribute);
        }

        return new SerializationContext(attributeValues, newSuppressed, new EndecTrace());
    }

    public SerializationContext and(SerializationContext other) {
        var newAttributeValues = new Dictionary<SerializationAttribute, object>(attributeValues);
        newAttributeValues.AddAll(other.attributeValues);

        var newSuppressed = new HashSet<SerializationAttribute>(suppressedAttributes);
        newSuppressed.AddAll(other.suppressedAttributes);

        return new SerializationContext(newAttributeValues, newSuppressed, new EndecTrace());
    }

    public bool hasAttribute(SerializationAttribute attribute) => attributeValues.ContainsKey(attribute) && !suppressedAttributes.Contains(attribute);
    
    public A getAttributeValue<A>(SerializationAttributeWithValue<A> attribute) => (A) attributeValues[attribute];

    public A requireAttributeValue<A>(SerializationAttributeWithValue<A> attribute) {
        if (hasAttribute(attribute)) return getAttributeValue(attribute);
        
        throw new MissingAttributeValueException("Context did not provide a value for attribute '" + attribute.name + "'");
    }

    private static IDictionary<SerializationAttribute, object> unpackAttributes(params SerializationAttributeInstance[] attributes) {
        var attributeValues = new Dictionary<SerializationAttribute, object>();
        foreach (var instance in attributes) {
            attributeValues.Add(instance.attribute(), instance.value());
        }

        return attributeValues;
    }
    
    //--
    
    public SerializationContext pushField(string fieldName) {
        return new SerializationContext(this.attributeValues, this.suppressedAttributes, this.trace.push(new FieldTraceElement(fieldName)));
    }

    public SerializationContext pushIndex(int index) {
        return new SerializationContext(this.attributeValues, this.suppressedAttributes, this.trace.push(new IndexTraceElement(index)));
    }

    public void throwMalformedInput(string message) => throw new EndecMalformedInputException(trace, message);

    public E exceptionWithTrace<E>(Func<EndecTrace, E> exceptionFactory) where E : Exception => exceptionFactory(trace);
    
    //--
    
    
    public override string ToString() {
        return $"SerializationContext[Attributes: {attributeValues}, SuppressedAttributes: {suppressedAttributes}, CurrentTrace: {trace}]";
    }
}