using System;

namespace io.wispforest.impl;

public class MissingAttributeValueException : Exception {
    public MissingAttributeValueException(string message) : base(message) { }
}