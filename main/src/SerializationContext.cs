using System;
using System.Collections.Generic;
using System.Linq;
using io.wispforest.endec.impl;
using io.wispforest.endec.impl.trace;
using io.wispforest.endec.util;

namespace io.wispforest.endec;

public class SerializationContext {
    private static readonly SerializationContext EMPTY = new (new Dictionary<SerializationAttribute, object>(), new HashSet<SerializationAttribute>(), new EndecTrace());

    private readonly IDictionary<SerializationAttribute, object> _attributeValues;
    private readonly ISet<SerializationAttribute> _suppressedAttributes;
    private readonly EndecTrace _trace;

    private SerializationContext(IDictionary<SerializationAttribute, object> attributeValues, ISet<SerializationAttribute> suppressedAttributes, EndecTrace trace) {
        _attributeValues = attributeValues.immutableWrap();
        _suppressedAttributes = suppressedAttributes.immutableWrap();
        _trace = trace;
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
        foreach (var entry in this._attributeValues) {
            if (!newAttributes.ContainsKey(entry.Key)) newAttributes.Add(entry);
        }
        return new SerializationContext(newAttributes, this._suppressedAttributes, new EndecTrace());
    }

    public SerializationContext withoutAttributes(params SerializationAttribute[] attributes) {
        var newAttributes = new Dictionary<SerializationAttribute, object>(_attributeValues);
        foreach (var attribute in attributes) newAttributes.Remove(attribute);
        return new SerializationContext(newAttributes, this._suppressedAttributes, new EndecTrace());
    }

    public SerializationContext withSuppressed(params SerializationAttribute[] attributes) {
        var newSuppressed = new HashSet<SerializationAttribute>(_suppressedAttributes);
        newSuppressed.addAll(attributes);
        return new SerializationContext(_attributeValues, newSuppressed, new EndecTrace());
    }

    public SerializationContext withoutSuppressed(params SerializationAttribute[] attributes) {
        var newSuppressed = new HashSet<SerializationAttribute>(_suppressedAttributes);
        foreach (var attribute in attributes) newSuppressed.Remove(attribute);
        return new SerializationContext(_attributeValues, newSuppressed, new EndecTrace());
    }

    public SerializationContext and(SerializationContext other) {
        var newAttributeValues = new Dictionary<SerializationAttribute, object>(_attributeValues);
        newAttributeValues.addAll(other._attributeValues);

        var newSuppressed = new HashSet<SerializationAttribute>(_suppressedAttributes);
        newSuppressed.addAll(other._suppressedAttributes);

        return new SerializationContext(newAttributeValues, newSuppressed, new EndecTrace());
    }

    public bool hasAttribute(SerializationAttribute attribute) => _attributeValues.ContainsKey(attribute) && !_suppressedAttributes.Contains(attribute);
    
    public A getAttributeValue<A>(SerializationAttributeWithValue<A> attribute) => (A) _attributeValues[attribute];

    public A requireAttributeValue<A>(SerializationAttributeWithValue<A> attribute) =>
        hasAttribute(attribute)
            ? getAttributeValue(attribute)
            : throw new MissingAttributeValueException("Context did not provide a value for attribute '" + attribute.name + "'");

    private static IDictionary<SerializationAttribute, object> unpackAttributes(params SerializationAttributeInstance[] attributes)
        => attributes.ToDictionary(instance => instance.attribute(), instance => instance.value());
    
    //--

    public EndecTrace trace() => _trace;
    
    public SerializationContext pushField(string fieldName) => new (_attributeValues, _suppressedAttributes, _trace.push(new FieldTraceElement(fieldName)));

    public SerializationContext pushIndex(int index) => new (_attributeValues, _suppressedAttributes, _trace.push(new IndexTraceElement(index)));

    public void throwMalformedInput(string message) => throw new EndecMalformedInputException(_trace, message);

    public E exceptionWithTrace<E>(Func<EndecTrace, E> exceptionFactory) where E : Exception => exceptionFactory(_trace);
    
    //--
    
    public override string ToString() => $"SerializationContext[Attributes: {_attributeValues}, SuppressedAttributes: {_suppressedAttributes}, CurrentTrace: {_trace}]";
}