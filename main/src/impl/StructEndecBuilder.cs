using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using io.wispforest.endec.util.funny;

namespace io.wispforest.endec.impl;

public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, in T17, out TResult>(
    T1 arg1,
    T2 arg2,
    T3 arg3,
    T4 arg4,
    T5 arg5,
    T6 arg6,
    T7 arg7,
    T8 arg8,
    T9 arg9,
    T10 arg10,
    T11 arg11,
    T12 arg12,
    T13 arg13,
    T14 arg14,
    T15 arg15,
    T16 arg16,
    T17 arg17);

public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, in T17, in T18, out TResult>(
    T1 arg1,
    T2 arg2,
    T3 arg3,
    T4 arg4,
    T5 arg5,
    T6 arg6,
    T7 arg7,
    T8 arg8,
    T9 arg9,
    T10 arg10,
    T11 arg11,
    T12 arg12,
    T13 arg13,
    T14 arg14,
    T15 arg15,
    T16 arg16,
    T17 arg17,
    T18 arg18);

public static class StructEndecBuilder {
    public delegate StructEndec<S> Builder<S>(StructEndecBuilder<S> ctx);

    public static StructEndec<S> of<S>(Builder<S> func) => func.Invoke(new StructEndecBuilder<S>());

    public static StructEndec<S> of<S, F1>(StructField<S, F1> f1, Func<F1, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2>(StructField<S, F1> f1, StructField<S, F2> f2, Func<F1, F2, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, Func<F1, F2, F3, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, Func<F1, F2, F3, F4, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, Func<F1, F2, F3, F4, F5, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, Func<F1, F2, F3, F4, F5, F6, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, Func<F1, F2, F3, F4, F5, F6, F7, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, Func<F1, F2, F3, F4, F5, F6, F7, F8, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8, F9>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, StructField<S, F9> f9, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
            f9.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance),
            f9.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, StructField<S, F9> f9, StructField<S, F10> f10, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
            f9.encodeField(ctx, serializer, instance, value);
            f10.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance),
            f9.decodeField(ctx, deserializer, instance),
            f10.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, StructField<S, F9> f9, StructField<S, F10> f10, StructField<S, F11> f11, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
            f9.encodeField(ctx, serializer, instance, value);
            f10.encodeField(ctx, serializer, instance, value);
            f11.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance),
            f9.decodeField(ctx, deserializer, instance),
            f10.decodeField(ctx, deserializer, instance),
            f11.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, StructField<S, F9> f9, StructField<S, F10> f10, StructField<S, F11> f11, StructField<S, F12> f12, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
            f9.encodeField(ctx, serializer, instance, value);
            f10.encodeField(ctx, serializer, instance, value);
            f11.encodeField(ctx, serializer, instance, value);
            f12.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance),
            f9.decodeField(ctx, deserializer, instance),
            f10.decodeField(ctx, deserializer, instance),
            f11.decodeField(ctx, deserializer, instance),
            f12.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, StructField<S, F9> f9, StructField<S, F10> f10, StructField<S, F11> f11, StructField<S, F12> f12, StructField<S, F13> f13, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
            f9.encodeField(ctx, serializer, instance, value);
            f10.encodeField(ctx, serializer, instance, value);
            f11.encodeField(ctx, serializer, instance, value);
            f12.encodeField(ctx, serializer, instance, value);
            f13.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance),
            f9.decodeField(ctx, deserializer, instance),
            f10.decodeField(ctx, deserializer, instance),
            f11.decodeField(ctx, deserializer, instance),
            f12.decodeField(ctx, deserializer, instance),
            f13.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, StructField<S, F9> f9, StructField<S, F10> f10, StructField<S, F11> f11, StructField<S, F12> f12, StructField<S, F13> f13, StructField<S, F14> f14, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
            f9.encodeField(ctx, serializer, instance, value);
            f10.encodeField(ctx, serializer, instance, value);
            f11.encodeField(ctx, serializer, instance, value);
            f12.encodeField(ctx, serializer, instance, value);
            f13.encodeField(ctx, serializer, instance, value);
            f14.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance),
            f9.decodeField(ctx, deserializer, instance),
            f10.decodeField(ctx, deserializer, instance),
            f11.decodeField(ctx, deserializer, instance),
            f12.decodeField(ctx, deserializer, instance),
            f13.decodeField(ctx, deserializer, instance),
            f14.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, StructField<S, F9> f9, StructField<S, F10> f10, StructField<S, F11> f11, StructField<S, F12> f12, StructField<S, F13> f13, StructField<S, F14> f14, StructField<S, F15> f15, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
            f9.encodeField(ctx, serializer, instance, value);
            f10.encodeField(ctx, serializer, instance, value);
            f11.encodeField(ctx, serializer, instance, value);
            f12.encodeField(ctx, serializer, instance, value);
            f13.encodeField(ctx, serializer, instance, value);
            f14.encodeField(ctx, serializer, instance, value);
            f15.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance),
            f9.decodeField(ctx, deserializer, instance),
            f10.decodeField(ctx, deserializer, instance),
            f11.decodeField(ctx, deserializer, instance),
            f12.decodeField(ctx, deserializer, instance),
            f13.decodeField(ctx, deserializer, instance),
            f14.decodeField(ctx, deserializer, instance),
            f15.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, StructField<S, F9> f9, StructField<S, F10> f10, StructField<S, F11> f11, StructField<S, F12> f12, StructField<S, F13> f13, StructField<S, F14> f14, StructField<S, F15> f15, StructField<S, F16> f16, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
            f9.encodeField(ctx, serializer, instance, value);
            f10.encodeField(ctx, serializer, instance, value);
            f11.encodeField(ctx, serializer, instance, value);
            f12.encodeField(ctx, serializer, instance, value);
            f13.encodeField(ctx, serializer, instance, value);
            f14.encodeField(ctx, serializer, instance, value);
            f15.encodeField(ctx, serializer, instance, value);
            f16.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance),
            f9.decodeField(ctx, deserializer, instance),
            f10.decodeField(ctx, deserializer, instance),
            f11.decodeField(ctx, deserializer, instance),
            f12.decodeField(ctx, deserializer, instance),
            f13.decodeField(ctx, deserializer, instance),
            f14.decodeField(ctx, deserializer, instance),
            f15.decodeField(ctx, deserializer, instance),
            f16.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, StructField<S, F9> f9, StructField<S, F10> f10, StructField<S, F11> f11, StructField<S, F12> f12, StructField<S, F13> f13, StructField<S, F14> f14, StructField<S, F15> f15, StructField<S, F16> f16, StructField<S, F17> f17, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
            f9.encodeField(ctx, serializer, instance, value);
            f10.encodeField(ctx, serializer, instance, value);
            f11.encodeField(ctx, serializer, instance, value);
            f12.encodeField(ctx, serializer, instance, value);
            f13.encodeField(ctx, serializer, instance, value);
            f14.encodeField(ctx, serializer, instance, value);
            f15.encodeField(ctx, serializer, instance, value);
            f16.encodeField(ctx, serializer, instance, value);
            f17.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance),
            f9.decodeField(ctx, deserializer, instance),
            f10.decodeField(ctx, deserializer, instance),
            f11.decodeField(ctx, deserializer, instance),
            f12.decodeField(ctx, deserializer, instance),
            f13.decodeField(ctx, deserializer, instance),
            f14.decodeField(ctx, deserializer, instance),
            f15.decodeField(ctx, deserializer, instance),
            f16.decodeField(ctx, deserializer, instance),
            f17.decodeField(ctx, deserializer, instance));
        });
    }

    public static StructEndec<S> of<S, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17, F18>(StructField<S, F1> f1, StructField<S, F2> f2, StructField<S, F3> f3, StructField<S, F4> f4, StructField<S, F5> f5, StructField<S, F6> f6, StructField<S, F7> f7, StructField<S, F8> f8, StructField<S, F9> f9, StructField<S, F10> f10, StructField<S, F11> f11, StructField<S, F12> f12, StructField<S, F13> f13, StructField<S, F14> f14, StructField<S, F15> f15, StructField<S, F16> f16, StructField<S, F17> f17, StructField<S, F18> f18, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17, F18, S> constructor) {
        return StructEndec.of<S>((ctx, serializer, instance, value) => {
            f1.encodeField(ctx, serializer, instance, value);
            f2.encodeField(ctx, serializer, instance, value);
            f3.encodeField(ctx, serializer, instance, value);
            f4.encodeField(ctx, serializer, instance, value);
            f5.encodeField(ctx, serializer, instance, value);
            f6.encodeField(ctx, serializer, instance, value);
            f7.encodeField(ctx, serializer, instance, value);
            f8.encodeField(ctx, serializer, instance, value);
            f9.encodeField(ctx, serializer, instance, value);
            f10.encodeField(ctx, serializer, instance, value);
            f11.encodeField(ctx, serializer, instance, value);
            f12.encodeField(ctx, serializer, instance, value);
            f13.encodeField(ctx, serializer, instance, value);
            f14.encodeField(ctx, serializer, instance, value);
            f15.encodeField(ctx, serializer, instance, value);
            f16.encodeField(ctx, serializer, instance, value);
            f17.encodeField(ctx, serializer, instance, value);
            f18.encodeField(ctx, serializer, instance, value);
        }, (ctx, deserializer, instance) => {
        return constructor(
            f1.decodeField(ctx, deserializer, instance),
            f2.decodeField(ctx, deserializer, instance),
            f3.decodeField(ctx, deserializer, instance),
            f4.decodeField(ctx, deserializer, instance),
            f5.decodeField(ctx, deserializer, instance),
            f6.decodeField(ctx, deserializer, instance),
            f7.decodeField(ctx, deserializer, instance),
            f8.decodeField(ctx, deserializer, instance),
            f9.decodeField(ctx, deserializer, instance),
            f10.decodeField(ctx, deserializer, instance),
            f11.decodeField(ctx, deserializer, instance),
            f12.decodeField(ctx, deserializer, instance),
            f13.decodeField(ctx, deserializer, instance),
            f14.decodeField(ctx, deserializer, instance),
            f15.decodeField(ctx, deserializer, instance),
            f16.decodeField(ctx, deserializer, instance),
            f17.decodeField(ctx, deserializer, instance),
            f18.decodeField(ctx, deserializer, instance));
        });
    }
}

public class StructEndecBuilder<S> {
    public readonly struct StructFieldHolder<F>(StructField<S, F> field) {
        public StructField<S, F> field { get; } = field;

        public static implicit operator StructFieldHolder<F>((Endec<F> endec, string name, Func<S, F> getter) t) => new (t.endec.fieldOf(t.name, t.getter));

        public static implicit operator StructFieldHolder<F>(StructField<S, F> field) => new (field);
    }

    public StructFieldHolder<T> field<T>(Endec<T> endec, string name, Func<S, T> getter) => endec.fieldOf<S>(name, getter.Invoke);


    public StructFieldHolder<T?> nullable<T>(Endec<T> endec, string name, Func<S, T?> getter, StructMarker _ = default) where T: struct => endec.nullableFieldOf(name, getter);

    public StructFieldHolder<T?> nullable<T>((Endec<T> endec, string name, Func<S, T?> getter) t, StructMarker _ = default) where T: struct => nullable(t.endec, t.name, t.getter);

    public StructFieldHolder<T?> nullable<T>(Endec<T> endec, string name, Func<S, T?> getter, ClassMarker _ = default) where T: class => endec.nullableFieldOf(name, getter);

    public StructFieldHolder<T?> nullable<T>((Endec<T> endec, string name, Func<S, T?> getter) t, ClassMarker _ = default) where T: class => nullable(t.endec, t.name, t.getter);


    public StructFieldHolder<T> defaulted<T>(Endec<T> endec, string name, Func<S, T> getter, T defaultValue = default, StructMarker _ = default) where T: struct => endec.defaultFieldOf(name, getter, defaultValue);

    public StructFieldHolder<T> defaulted<T>((Endec<T> endec, string name, Func<S, T> getter, T defaultValue) t, StructMarker _ = default) where T: struct => defaulted(t.endec, t.name, t.getter, t.defaultValue);

    public StructFieldHolder<T> defaulted<T>(Endec<T> endec, string name, Func<S, T> getter, T defaultValue, ClassMarker _ = default) where T: class => endec.defaultFieldOf(name, getter, defaultValue);

    public StructFieldHolder<T> defaulted<T>((Endec<T> endec, string name, Func<S, T> getter, T defaultValue) t, ClassMarker _ = default) where T: class => defaulted(t.endec, t.name, t.getter, t.defaultValue);


    public StructEndec<S> of<F1>(StructFieldHolder<F1> f1, Func<F1, S> constructor)
        => StructEndecBuilder.of(f1.field, constructor);

    public StructEndec<S> of<F1, F2>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, Func<F1, F2, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, constructor);

    public StructEndec<S> of<F1, F2, F3>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, Func<F1, F2, F3, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, Func<F1, F2, F3, F4, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, Func<F1, F2, F3, F4, F5, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, Func<F1, F2, F3, F4, F5, F6, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, Func<F1, F2, F3, F4, F5, F6, F7, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, Func<F1, F2, F3, F4, F5, F6, F7, F8, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8, F9>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, StructFieldHolder<F9> f9, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, f9.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, StructFieldHolder<F9> f9, StructFieldHolder<F10> f10, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, f9.field, f10.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, StructFieldHolder<F9> f9, StructFieldHolder<F10> f10, StructFieldHolder<F11> f11, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, f9.field, f10.field, f11.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, StructFieldHolder<F9> f9, StructFieldHolder<F10> f10, StructFieldHolder<F11> f11, StructFieldHolder<F12> f12, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, f9.field, f10.field, f11.field, f12.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, StructFieldHolder<F9> f9, StructFieldHolder<F10> f10, StructFieldHolder<F11> f11, StructFieldHolder<F12> f12, StructFieldHolder<F13> f13, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, f9.field, f10.field, f11.field, f12.field, f13.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, StructFieldHolder<F9> f9, StructFieldHolder<F10> f10, StructFieldHolder<F11> f11, StructFieldHolder<F12> f12, StructFieldHolder<F13> f13, StructFieldHolder<F14> f14, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, f9.field, f10.field, f11.field, f12.field, f13.field, f14.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, StructFieldHolder<F9> f9, StructFieldHolder<F10> f10, StructFieldHolder<F11> f11, StructFieldHolder<F12> f12, StructFieldHolder<F13> f13, StructFieldHolder<F14> f14, StructFieldHolder<F15> f15, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, f9.field, f10.field, f11.field, f12.field, f13.field, f14.field, f15.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, StructFieldHolder<F9> f9, StructFieldHolder<F10> f10, StructFieldHolder<F11> f11, StructFieldHolder<F12> f12, StructFieldHolder<F13> f13, StructFieldHolder<F14> f14, StructFieldHolder<F15> f15, StructFieldHolder<F16> f16, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, f9.field, f10.field, f11.field, f12.field, f13.field, f14.field, f15.field, f16.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, StructFieldHolder<F9> f9, StructFieldHolder<F10> f10, StructFieldHolder<F11> f11, StructFieldHolder<F12> f12, StructFieldHolder<F13> f13, StructFieldHolder<F14> f14, StructFieldHolder<F15> f15, StructFieldHolder<F16> f16, StructFieldHolder<F17> f17, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, f9.field, f10.field, f11.field, f12.field, f13.field, f14.field, f15.field, f16.field, f17.field, constructor);

    public StructEndec<S> of<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17, F18>(StructFieldHolder<F1> f1, StructFieldHolder<F2> f2, StructFieldHolder<F3> f3, StructFieldHolder<F4> f4, StructFieldHolder<F5> f5, StructFieldHolder<F6> f6, StructFieldHolder<F7> f7, StructFieldHolder<F8> f8, StructFieldHolder<F9> f9, StructFieldHolder<F10> f10, StructFieldHolder<F11> f11, StructFieldHolder<F12> f12, StructFieldHolder<F13> f13, StructFieldHolder<F14> f14, StructFieldHolder<F15> f15, StructFieldHolder<F16> f16, StructFieldHolder<F17> f17, StructFieldHolder<F18> f18, Func<F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17, F18, S> constructor)
        => StructEndecBuilder.of(f1.field, f2.field, f3.field, f4.field, f5.field, f6.field, f7.field, f8.field, f9.field, f10.field, f11.field, f12.field, f13.field, f14.field, f15.field, f16.field, f17.field, f18.field, constructor);

    //     static void Main(String[] args) {
//         Func<int, String> encodeCall = (fieldArgName) => $"f{fieldArgName}.encodeField(ctx, serializer, instance, value);\n";
//         Func<int, String> encodeCalls = (index) => {
//             return Enumerable.Range(1, index).Select(fieldArgName => encodeCall(fieldArgName)).Aggregate((s1, s2) => s1 + s2);
//         };
//
//         Func<int, String> decodeCall = (index) => $"f{index}.decodeField(ctx, deserializer, instance)";
//         Func<int, String> decodeCalls = (index) => {
//             return Enumerable.Range(1, index).Select(fieldArgName => decodeCall(fieldArgName)).Aggregate((s1, s2) => $"{s1},\n{s2}");
//         };
//
//         Func<int, String> structFieldArg = (index) => $"StructField<S, F{index}> f{index}";
//         Func<int, String> structFieldArgs = (index) => {
//             return Enumerable.Range(1, index).Select(structFieldArg).Aggregate((s1, s2) => $"{s1}, {s2}");
//         };
//
//         Func<int, String> structFieldType = (index) => $"F{index}";
//         Func<int, String> structFieldTypes = (index) => {
//             return Enumerable.Range(1, index).Select(structFieldType).Aggregate((s1, s2) => $"{s1}, {s2}");
//         };
//
//         Func<int, String> staticConstructorMethod = (maxFields) => {
//             var value =
//             $"""
//             public static StructEndec<S> of<{structFieldTypes(maxFields)}>({structFieldArgs(maxFields)}, Func<{structFieldTypes(maxFields)}, S> constructor) [
//                 return StructEndec.of((ctx, serializer, instance, value) => [
//                     {encodeCalls(maxFields)}
//                 ], (ctx, deserializer, instance) => [
//                     return constructor({decodeCalls(maxFields)});
//                 ]);
//             ]
//             """;
//
//             return value;
//         };
//
//         File.WriteAllText("Test.txt", Enumerable.Range(1, 18).Select(staticConstructorMethod).Aggregate((s1, s2) => $"{s1}\n{s2}"));
//     }
}