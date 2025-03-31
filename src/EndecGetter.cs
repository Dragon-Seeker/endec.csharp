using System;
using System.Reflection;

namespace io.wispforest;

public sealed class EndecGetter {
    public static Endec<T> Endec<T>() where T : EndecGetter<T> {
        #if NET7_OR_HIGHER // etc, if needed.
            return T.Endec();
        # else
            return EndecGetter<T>.Endec();
        #endif
    }
}

public interface EndecGetter<T> where T : EndecGetter<T> {
    #if NET7_OR_HIGHER // etc, if needed.
    public static abstract Endec<T> Endec();
    #else
    public static Endec<T> Endec() {
        var methodCall = typeof(T).GetMethod("Endec", BindingFlags.Public | BindingFlags.Static);
        
        var possibleEndec = methodCall.Invoke(null, null);

        if (possibleEndec is not Endec<T> endec) {
            throw new InvalidCastException("Unable to get Endec due to it being the incorrect type!");
        }
        
        return endec;
    }
    #endif
}

#if NET7_OR_HIGHER
public interface StructEndecGetter<T> : EndecGetter<T> where T : StructEndecGetter<T> {
    public new static abstract StructEndec<T> Endec();
    
    static Endec<T> EndecGetter<T>.Endec() {
        return T.Endec();
    }
}
#endif