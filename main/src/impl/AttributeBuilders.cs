using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using io.wispforest.endec.util;

namespace io.wispforest.endec.impl;

public class AttributeEndecBuilder<T> {

    private readonly List<(SerializationAttribute, Endec<T>)> _branches = new ();

    public AttributeEndecBuilder(Endec<T> endec, SerializationAttribute attribute) {
        _branches.Add((attribute, endec));
    }

    public AttributeEndecBuilder<T> orElseIf(Endec<T> endec, SerializationAttribute attribute) {
        return orElseIf(attribute, endec);
    }

    public AttributeEndecBuilder<T> orElseIf(SerializationAttribute attribute, Endec<T> endec) {
        if (_branches.Exists(tuple => tuple.Item1.Equals(attribute))) {
            throw new ArgumentException("Cannot have more than one branch for attribute " + attribute.name);
        }

        _branches.Add((attribute, endec));
        return this;
    }

    public Endec<T> orElse(Endec<T> endec) {
        return Endec.of((ctx, serializer, value) => {
            var branchEndec = endec;

            foreach (var branch in _branches) {
                if (ctx.hasAttribute(branch.Item1)) {
                    branchEndec = branch.Item2;
                    break;
                }
            }

            branchEndec.encode(ctx, serializer, value);
        }, (ctx, deserializer) => {
            var branchEndec = endec;

            foreach (var branch in _branches) {
                if (ctx.hasAttribute(branch.Item1)) {
                    branchEndec = branch.Item2;
                    break;
                }
            }

            return branchEndec.decode(ctx, deserializer);
        });
    }
}

public class AttributeStructEndecBuilder<T> {

    private readonly List<(SerializationAttribute, StructEndec<T>)> branches = new ();

    public AttributeStructEndecBuilder(StructEndec<T> endec, SerializationAttribute attribute) {
        branches.Add((attribute, endec));
    }

    public AttributeStructEndecBuilder<T> orElseIf(StructEndec<T> endec, SerializationAttribute attribute) {
        return orElseIf(attribute, endec);
    }

    public AttributeStructEndecBuilder<T> orElseIf(SerializationAttribute attribute, StructEndec<T> endec) {
        if (branches.Exists(tuple => tuple.Item1.Equals(attribute))) {
            throw new ArgumentException("Cannot have more than one branch for attribute " + attribute.name);
        }

        branches.Add((attribute, endec));
        return this;
    }

    public StructEndec<T> orElse(StructEndec<T> endec) {
        return StructEndec.of((ctx, serializer, instance, value) => {
            var branchEndec = endec;

            foreach (var branch in branches) {
                if (ctx.hasAttribute(branch.Item1)) {
                    branchEndec = branch.Item2;
                    break;
                }
            }

            branchEndec.encodeStruct(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
            var branchEndec = endec;

            foreach (var branch in branches) {
                if (ctx.hasAttribute(branch.Item1)) {
                    branchEndec = branch.Item2;
                    break;
                }
            }

            return branchEndec.decodeStruct(ctx, deserializer, instance);
        });
    }
}