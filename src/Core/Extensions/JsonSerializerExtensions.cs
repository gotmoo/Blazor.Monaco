using System.Text.Json;

namespace Blazor.Monaco;

public static class JsonSerializerExtensions
{
    public static string ToJsonWithEnumDescription<T>(this T obj, bool writeIndented = false)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = writeIndented,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new EnumDescriptionConverter());
        return JsonSerializer.Serialize(obj, options);
    }

    public static T FromJsonWithEnumDescription<T>(this string json)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new EnumDescriptionConverter());
        return JsonSerializer.Deserialize<T>(json, options);
    }
}