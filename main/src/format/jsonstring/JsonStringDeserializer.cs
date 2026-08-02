using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using io.wispforest.endec.impl;
using io.wispforest.endec.util.funny;
using System.Text.Json;
using io.wispforest.endec.format.parser;

namespace io.wispforest.endec.format.jsonstring;

[Experimental("json_parsing_deserializer")]
public class JsonStringDeserializer : StringParsingDeserializer<TokenType> {

  private JsonStringDeserializer(string input) : base(input) { }

  protected override TokenType tokenAtIndex(int index) => TokenTypeUtils.forChar(input[index]);

  protected override void skipWhitespace() => advance((token, _, _) => token == TokenType.whitespace);

  protected dynamic parseNumber() {
    skipWhitespace();
    return TokenTypeUtils.NUMBER_INITIAL.contains(currentToken)
        ? JsonUtils.jsonDecode(subInput((token, _, _) => TokenTypeUtils.NUMBER.contains(token)))
        : throw new InvalidOperationException($"Expected a number, found: {currentToken}");
  }

  protected bool parseBool() {
    skipWhitespace();
    if (!TokenTypeUtils.BOOL.contains(currentToken)) throw new InvalidOperationException($"Expected 'true' or 'false', found: {currentToken}");
    var boolean = currentToken == TokenType.literalTrue;
    var requiredInput = boolean ? "true" : "false";
    return subInput(requiredInput.Length) == requiredInput
      ? boolean
      : throw new InvalidOperationException($"Encountered unexpected char while parsing boolean {requiredInput}");
  }

  protected string parseString() {
    skipWhitespace();
    return currentToken == TokenType.quote
      ? (string)JsonUtils.jsonDecode(subInput((token, input, pointer) => token != TokenType.quote || input[pointer - 1] == '\\'))
      : throw new InvalidOperationException($"Expected string, found: {currentToken}");
  }

  public override sbyte readSByte(SerializationContext ctx) => parseNumber();
  public override byte readByte(SerializationContext ctx) => parseNumber();

  public override short readShort(SerializationContext ctx) => parseNumber();
  public override ushort readUShort(SerializationContext ctx) => parseNumber();

  public override int readVarInt(SerializationContext ctx) => readInt(ctx);
  public override long readVarLong(SerializationContext ctx) => readLong(ctx);

  public override int readInt(SerializationContext ctx) => parseNumber();
  public override uint readUInt(SerializationContext ctx) => parseNumber();

  public override long readLong(SerializationContext ctx) => parseNumber();
  public override ulong readULong(SerializationContext ctx) => parseNumber();

  public override float readFloat(SerializationContext ctx) => parseNumber();
  public override double readDouble(SerializationContext ctx) => parseNumber();

  public override bool readBoolean(SerializationContext ctx) => parseBool();
  public override string readString(SerializationContext ctx) => parseString();
  public override byte[] readBytes(SerializationContext ctx) => Endec.BYTES.decode(ctx, this);
  public override Optional<V> readOptionalPacked<V>(SerializationContext ctx, Endec<V> endec) {
    skipWhitespace();
    if (currentToken == TokenType.literalNull) {
      if (subInput(4) != "null") throw new InvalidOperationException("Encountered unexpected char while parsing 'null'");
      advance(4);
      return Optional.empty<V>();
    } else {
      return Optional.of(endec.decode(ctx, this));
    }
  }

  protected void validateAndAdvance(TokenType token, string type) {
    if (currentToken != token) throw new InvalidOperationException($"Expected {type}, found: {currentToken}");
    advance();
    skipWhitespace();
  }

  public override SequenceDeserializer<E> sequence<E>(SerializationContext ctx, Endec<E> elementEndec) {
    validateAndAdvance(TokenType.arrayBegin, "array");
    return new Sequence<E>(this, ctx, elementEndec);
  }

  public override MapDeserializer<V> map<V>(SerializationContext ctx, Endec<V> valueEndec) {
    validateAndAdvance(TokenType.objectBegin, "object");
    return new Map<V>(this, ctx, valueEndec);
  }

  public override StructDeserializer structed(SerializationContext ctx) {
    validateAndAdvance(TokenType.objectBegin, "object");
    return new Struct(this);
  }

  protected string parseKey() {
    var key = parseString();
    skipWhitespace();
    if (currentToken != TokenType.keyValueSeparator) throw new InvalidOperationException($"Expected ':' to separate key from value, found {currentToken}");
    advance();
    skipWhitespace();
    return key;
  }

  protected void skipAfterValue((TokenType end, TokenType seperator) parts) {
    skipWhitespace();
    if (!parts.contains(currentToken)) {
      throw new InvalidOperationException($"Expected '{parts.end}' for next entry or '{parts.seperator}' to close object, found {currentToken}");
    }
    if (currentToken == parts.seperator) advance();
    skipWhitespace();
  }

  protected IEnumerable<T> sequenceParser<V, T>(SerializationContext ctx, Endec<V> elementEndec, (TokenType end, TokenType seperator) parts, Func<Func<V>, T> valueHandler) {
    var decoder = () => elementEndec.decode(ctx, this);
    while (currentToken != parts.end) {
      yield return valueHandler(decoder);
      skipAfterValue(parts);
    }
    advance();
  }

  protected class Sequence<V>(JsonStringDeserializer deserializer, SerializationContext ctx, Endec<V> elementEndec) : SequenceDeserializer<V> {
    private readonly IEnumerator<V> _elements = deserializer.sequenceParser(ctx, elementEndec, TokenTypeUtils.ARRAY_PARTS, getter => getter()).GetEnumerator();

    public V Current => _elements.Current;
    public bool MoveNext() => _elements.MoveNext();
    public void Reset() => _elements.Reset();
    public void Dispose() => _elements.Dispose();
    public int estimatedSize() => 16; // TODO: FIND SOME WAY TO CALCULATE OR GIVE A BETTER SIZE?
  }

  protected class Map<V>(JsonStringDeserializer deserializer, SerializationContext ctx, Endec<V> elementEndec) : MapDeserializer<V> {
    private readonly IEnumerator<KeyValuePair<string, V>> _elements = deserializer.sequenceParser(ctx, elementEndec, TokenTypeUtils.OBJECT_PARTS,
      (getter) => new KeyValuePair<string, V>(deserializer.parseKey(), getter())).GetEnumerator();

    public KeyValuePair<string, V> Current => _elements.Current;
    public bool MoveNext() => _elements.MoveNext();
    public void Reset() => _elements.Reset();
    object? IEnumerator.Current => Current;
    public void Dispose() => _elements.Dispose();
    public int estimatedSize() => 16; // TODO: FIND SOME WAY TO CALCULATE OR GIVE A BETTER SIZE?
  }

  protected class Struct(JsonStringDeserializer deserializer) : StructDeserializer {
    private readonly IDictionary<string, int> _valueIndicesByKey = new Dictionary<string, int>();

    private bool _reachedEndOfStruct = false;

    public F field<F>(string name, SerializationContext ctx, Endec<F> endec, Func<F>? defaultValueFactory = null) {
      if (_valueIndicesByKey.TryGetValue(name, out var idx)) {
        return deserializer.movePointer(idx, () => {
          var result = endec.decode(ctx, deserializer);
          deserializer.skipAfterValue(TokenTypeUtils.OBJECT_PARTS);
          // TODO: TECHNICALLY NOT REQUIRED BUT DOSE HELP VALIDATE THAT WE ARE STILL CORRECT FORMAT
          return result;
        });
      }

      while (true) {
        if (_reachedEndOfStruct) throw new Exception("Field '" + name + "' was missing from serialized data, but no default value was provided");
        var key = deserializer.parseKey();

        if (key == name) {
          var result = endec.decode(ctx, deserializer);
          deserializer.skipAfterValue(TokenTypeUtils.OBJECT_PARTS);
          return result;
        }

        _valueIndicesByKey[key] = deserializer.pointer;
        _skipValue();
      }
    }

    private void _skipValue(bool breakOnSeparators = true) {
      int arrayDepth = 0, objectDepth = 0;
      deserializer.advance((token, _, _) => {
        if (token == TokenType.objectBegin) {
          objectDepth++;
        } else if (token == TokenType.arrayBegin) {
          arrayDepth++;
        } else if (token == TokenType.objectEnd) {
          if (objectDepth == 0) {
            _reachedEndOfStruct = true;
            return false;
          }
          objectDepth--;
        } else if (token == TokenType.arrayEnd) {
          arrayDepth--;
        } else if (token == TokenType.elementSeparator) {
          if(objectDepth == 0 && arrayDepth == 0 && breakOnSeparators) return false;
        }
        return true;
      });
    }

    public void Dispose() {
      if (_reachedEndOfStruct) return;
      while (!_reachedEndOfStruct) {
        deserializer.parseKey();
        _skipValue();
      }
    }
  }

  private static class Tests {
    private static void main(List<String> args) {
      //   final result = Endec.i32.listOf().mapOf().mapOf().decode(
      //         SerializationContext.empty,
      //         JsonStringDeserializer(r'''
      // {
      //   "a": {"_": [1]},
      //   "b": {"_": [2, 3], "-": [3, 2]},
      //   "c": {"_": [4, 5, 6]}
      // }
      // '''),
      //       );

      var endec = StructEndecBuilder.of<(int, string, IList<IDictionary<double, bool>>)>(ctx => {
        return ctx.of(
          (Endec.INT, "int", (tuple) => tuple.Item1),
          (Endec.STRING, "string", (tuple) => tuple.Item2),
          (Endec.map((d) => d.ToString(CultureInfo.InvariantCulture), Double.Parse, Endec.BOOLEAN).listOf(), "bruh", (tuple) => tuple.Item3),
          (int p0, string p1, IList<IDictionary<double, bool>> p2) => new (p0, p1, p2)
        );
      });

      var result = endec.decodeFully(
          (str) => new JsonStringDeserializer(str),
          """
          {
            "string": "bruh",
            "bruh": [
              {"1.5": false, "3.8678": true},
              {"-1.5": true, "2.1e5": false}
            ],
            "int": 69
          }
          """
      );
      Console.WriteLine(result);
    }

    private static void main1(IList<string> args) {
      var jsonString = """7""";
      var json = JsonUtils.jsonDecode(jsonString);
      Console.WriteLine(jsonString);
      Console.WriteLine(json);
      Console.WriteLine(json.GetType());

      // print(double.parse(source))
    }
  }
}

public enum TokenType {
  elementSeparator,
  objectBegin,
  objectEnd,
  arrayBegin,
  arrayEnd,
  quote,
  digit,
  plus,
  minus,
  e,
  keyValueSeparator,
  dot,
  literalTrue,
  literalFalse,
  literalNull,
  whitespace,
  other
}

public static class TokenTypeUtils {
  public static readonly (TokenType end, TokenType seperator) OBJECT_PARTS = (TokenType.objectEnd, TokenType.elementSeparator);

  public static readonly (TokenType end, TokenType seperator) ARRAY_PARTS = (TokenType.arrayEnd, TokenType.elementSeparator);

  public static readonly (TokenType, TokenType) NUMBER_INITIAL = (TokenType.digit, TokenType.minus);
  public static readonly (TokenType, TokenType, TokenType, TokenType, TokenType) NUMBER = (TokenType.digit, TokenType.minus, TokenType.dot, TokenType.e, TokenType.plus);
  public static readonly (TokenType, TokenType) BOOL = (TokenType.literalTrue, TokenType.literalFalse);

  public static TokenType forChar(char character) => character switch {
    ',' => TokenType.elementSeparator,
    '{' => TokenType.objectBegin,
    '}' => TokenType.objectEnd,
    '[' => TokenType.arrayBegin,
    ']' => TokenType.arrayEnd,
    '"' => TokenType.quote,
    '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or '0' => TokenType.digit,
    '+' => TokenType.plus,
    '-' => TokenType.minus,
    'e' or 'E' => TokenType.e,
    ':' => TokenType.keyValueSeparator,
    '.' => TokenType.dot,
    ' ' or '\n' or '\r' or '\t' => TokenType.whitespace,
    't' => TokenType.literalTrue,
    'f' => TokenType.literalFalse,
    'n' => TokenType.literalNull,
    _ => TokenType.other
  };
}


public static class TupleExt {
  extension<T>(ITuple tuple) {
    public bool contains(T item) {
      var length = tuple.Length;
      for (var i = 0; i < length; i++) {
        if (Equals(tuple[i], item)) return true;
      }
      return false;
    }
  }
}

