using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace io.wispforest.endec.util;

public class BlockWriter {
    private readonly StringWriter result = new StringWriter();
    private readonly Stack<(bool incrementIndentation, string value)> blocks = new ();

    private int indentLevel = 0;

    public BlockWriter() {}

    public BlockWriter write(String value) {
        result.Write(value);

        return this;
    }

    public BlockWriter writeln() {
        writeln("");

        return this;
    }

    public BlockWriter writeln(String value) {
        
        result.Write(value + "\n" + ("  ".repeat(Math.Max(0, indentLevel))));

        return this;
    }

    public BlockWriter startBlock(String startDelimiter, String endDelimiter) {
        return startBlock(startDelimiter, endDelimiter, true);
    }

    public BlockWriter startBlock(String startDelimiter, String endDelimiter, bool incrementIndentation) {
        if (incrementIndentation) {
            indentLevel++;
            writeln(startDelimiter);
        } else {
            write(startDelimiter);
        }
        
        blocks.Push(new (incrementIndentation, endDelimiter));

        return this;
    }

    public BlockWriter writeBlock(String startDelimiter, String endDelimiter, Action<BlockWriter> consumer) {
        return writeBlock(startDelimiter, endDelimiter, true, consumer);
    }

    public BlockWriter writeBlock(String startDelimiter, String endDelimiter, bool incrementIndentation, Action<BlockWriter> consumer) {
        startBlock(startDelimiter, endDelimiter, incrementIndentation);

        consumer(this);

        endBlock();

        return this;
    }

    public BlockWriter endBlock() {
        var endBlockData = blocks.Pop();

        if (endBlockData.incrementIndentation) {
            indentLevel--;
            writeln();
        }

        write(endBlockData.value);

        return this;
    }

    public String buildResult() {
        return result.ToString();
    }
}

public static class StringExtension {
    public static string repeat(this string str, int amount) => new StringBuilder().Insert(0, str, 3).ToString();
}