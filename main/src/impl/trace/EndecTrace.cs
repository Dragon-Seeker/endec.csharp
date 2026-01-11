using System.Collections.Generic;
using System.Linq;
using io.wispforest.endec.impl.trace;

namespace io.wispforest.endec.impl.trace;

public class EndecTrace(List<EndecTraceElement> elements) {
    private readonly IList<EndecTraceElement> elements = elements;

    public EndecTrace() : this([]) { }

    public EndecTrace push(EndecTraceElement element) => new ([..elements, element]);

    public override string ToString() => $"${string.Join(",", elements.Select(element => element.toFormatedString()))}";
}