using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using io.wispforest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace io.wispforest.endec.format.newtonsoft;

public class JsonUtils {
    public static string writeToString(JToken value) {
        var stream = new StringWriter();
        value.WriteTo(new JsonTextWriter(stream));
        return stream.ToString();
    }
    
    public static JToken readFromString(string value) => JToken.ReadFrom(new JsonTextReader(new StringReader(value)));

    public static JToken readFromFile(string path) => readFromString(File.ReadAllText(path));
    
    public static JToken readFromStream(Stream value) => readFromString(new StreamReader(value).ReadToEnd());
    
    public static JToken readFromFileIfPresent(string path) => File.Exists(path) ? readFromString(File.ReadAllText(path)) : new JObject();

    public static T? parseFromFile<T>(string path) where T : EndecGetter<T> => parseFromFile(path, EndecGetter.endec<T>());
    
    public static T? parseFromFile<T>(string path, Endec<T> endec) => File.Exists(path) ? endec.decodeFully(JsonDeserializer.of, readFromFile(path)) : default;
    
    public static T parseFromStream<T>(Stream stream) where T : EndecGetter<T> => parseFromStream<T>(stream, EndecGetter.endec<T>());
    
    public static T parseFromStream<T>(Stream stream, Endec<T> endec) => endec.decodeFully(JsonDeserializer.of, readFromStream(stream));
    
    public static T parseFromString<T>(string str, Endec<T> endec) => endec.decodeFully(JsonDeserializer.of, readFromString(str));
    
    public static Dictionary<string,T> parseFiles<T>(string directory, Func<string, string> keyMaker, Action<string, Exception> onError) where T : EndecGetter<T>
        => parseFiles<T>(directory, EndecGetter.endec<T>(), keyMaker, onError);

    private static readonly List<string> ALLOWED_JSON_PATTERNS = ["*.json", "*.json5"];
    
    public static Dictionary<string, T> parseFiles<T>(string directory, Endec<T> endec, Func<string, string> keyMaker, Action<string, Exception> onError) {
        var files = ALLOWED_JSON_PATTERNS.SelectMany(pattern => Directory.GetFiles(directory, pattern)).ToList();

        var entries = new Dictionary<string, T>();
        
        foreach (var file in files) {
            if (file is null || !File.Exists(file)) continue;

            try {
                entries[keyMaker(file)] = parseFromFile(file, endec)!;
            } catch (Exception e) {
                onError(file, e);
            }
        }

        return entries;
    }
}