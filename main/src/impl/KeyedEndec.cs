using System;

namespace io.wispforest.endec.impl;

public class KeyedEndec<F> {
    public string key { get; }
    public Endec<F> endec { get; }
    public Func<F> defaultValueFactory { get; }
    
    public KeyedEndec(string key, Endec<F> endec, Func<F> defaultValueFactory) {
        this.key = key;
        this.endec = endec;
        this.defaultValueFactory = defaultValueFactory;
    }
    
    public KeyedEndec(String key, Endec<F> endec, F defaultValue) : this(key, endec, () => defaultValue) { }
    
    public F defaultValue() {
        return this.defaultValueFactory();
    }
}