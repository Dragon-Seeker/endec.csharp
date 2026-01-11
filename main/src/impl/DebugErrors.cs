using System;

namespace io.wispforest.endec.impl;

public static class DebugErrors {
    public static Action<object, Exception>? decodeErrorHook;
}