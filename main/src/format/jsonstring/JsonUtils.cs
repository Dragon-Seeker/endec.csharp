using System;
using System.Text.Json;

namespace io.wispforest.endec.format.jsonstring;

public class JsonUtils {
    public static dynamic jsonDecode(string value) => JsonSerializer.Deserialize<dynamic>(value);
    public static string jsonEncode(dynamic value) => JsonSerializer.Serialize<dynamic>(value);
}