using System;

namespace io.wispforest.endec.format.parser;

public abstract class StringParsingDeserializer<E>(string input) : ParsingDeserializer<string, E, int>(input) where E : struct, Enum {
    protected override int pointer { get; set; } = 0;

    protected override int add(int p1, int p2) => p1 + p2;

    protected (int start, int end) findSubInput(Func<E, string, int, bool> condition, int padding = 0) {
        var numberBegin = pointer;
        if (padding > 0) advance(padding);
        advance(condition);
        if (padding > 0) advance(padding);
        return (numberBegin, pointer);
    }

    protected string subInput(Func<E, string, int, bool> condition, int padding = 0) => subInput(findSubInput(condition, padding));

    protected string subInput((int start, int end) range) => subInput(range.end, range.start);

    protected string subInput(int length) => input.Substring(pointer, length);

    protected string subInput(int startIndex, int endIndex) => input.Substring(startIndex, startIndex - endIndex);

    protected abstract void skipWhitespace();
}