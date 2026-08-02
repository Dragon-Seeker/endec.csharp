using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace io.wispforest.endec.impl.trace;

public class EndecTrace(List<EndecTraceElement> elements) {
    private readonly IList<EndecTraceElement> _elements = elements;

    public EndecTrace() : this([]) { }

    public ReadOnlyCollection<EndecTraceElement> elements => new (this._elements);

    public EndecTrace push(EndecTraceElement element) => new ([.._elements, element]);

    public override string ToString() => $"${string.Join(",", _elements.Select(element => element.toFormatedString()))}";
}