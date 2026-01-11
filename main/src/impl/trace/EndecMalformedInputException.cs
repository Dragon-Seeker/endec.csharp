using System;

namespace io.wispforest.endec.impl.trace;

public class EndecMalformedInputException(EndecTrace location, string message) : Exception(createMessage(location, message)) {

    public readonly EndecTrace location = location;
    public readonly string message = message;

    private static string createMessage(EndecTrace location, string message) => "Malformed input at " + location.ToString() + ": " + message;
}