using System;

namespace io.wispforest.endec.util;

public interface Endable : IDisposable {

    void end();
    
    void IDisposable.Dispose() {
        end();
    }
}