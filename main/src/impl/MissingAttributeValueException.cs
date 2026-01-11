using System;

namespace io.wispforest.endec.impl;

public class MissingAttributeValueException : Exception {
    public MissingAttributeValueException(string message) : base(message) { }
}