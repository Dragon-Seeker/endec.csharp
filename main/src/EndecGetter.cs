using System;
using System.Reflection;

namespace io.wispforest.endec;

public sealed class EndecGetter {
    public static Endec<T> endec<T>() where T : EndecGetter<T> {
    #if NET7_OR_HIGHER // etc, if needed.
            return T.endec();
    # else
        return EndecGetter<T>.endec();
    #endif
    }
}

public interface EndecGetter<T> where T : EndecGetter<T> {
    #if NET7_OR_HIGHER // etc, if needed.
    public static abstract Endec<T> endec();
    #else
    public static Endec<T> endec() {
        var type = typeof(T);
        var methodCall = type.GetMethod("endec", BindingFlags.Public | BindingFlags.Static)
            ?? type.GetMethod("Endec", BindingFlags.Public | BindingFlags.Static);

        var possibleEndec = methodCall?.Invoke(null, null);

        return possibleEndec as Endec<T> ?? throw new InvalidCastException("Unable to get Endec due to it being the incorrect type!");
    }
    #endif
}

public sealed class StructEndecGetter {
    public static StructEndec<T> endec<T>() where T : StructEndecGetter<T> {
        #if NET7_OR_HIGHER // etc, if needed.
            return T.Endec();
        # else
        return StructEndecGetter<T>.endec();
        #endif
    }
}


public interface StructEndecGetter<T> : EndecGetter<T> where T : StructEndecGetter<T> {
    #if NET7_OR_HIGHER // etc, if needed.
    public new static abstract StructEndec<T> endec();
    #else
    static StructEndec<T> endec() {
        return EndecGetter.endec<T>() as StructEndec<T> ?? throw new InvalidCastException("Unable to get StructEndec due to it being the incorrect type!");
    }
    #endif
}