namespace io.wispforest.endec.impl.trace;

public interface EndecTraceElement {
    string valueAsString();
    string toFormatedString();
}

public sealed class FieldTraceElement(string name) : EndecTraceElement {
    public string valueAsString() => name;
    public string toFormatedString() => "." + name;
}

public sealed class IndexTraceElement(int index) : EndecTraceElement {
    public string valueAsString() => index.ToString();
    public string toFormatedString() => "[" + index + "]";
}