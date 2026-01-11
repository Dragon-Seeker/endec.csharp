namespace io.wispforest.endec.impl.trace;

public interface EndecTraceElement {
    string toFormatedString();
}

public sealed class FieldTraceElement(string name) : EndecTraceElement {
    public string toFormatedString() => "." + name;
}

public sealed class IndexTraceElement(int index) : EndecTraceElement {
    public string toFormatedString() => "[" + index + "]";
}