using System;

namespace io.wispforest.util;

public interface Endable : IDisposable {

    void end();
    
    void IDisposable.Dispose() {
        end();
    }
}