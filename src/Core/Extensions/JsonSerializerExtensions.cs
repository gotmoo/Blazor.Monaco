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
#pragma warning disable IL2026
        return JsonSerializer.Serialize(obj, options);
#pragma warning restore IL2026
    }

    public static T? FromJsonWithEnumDescription<T>(this string json)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new EnumDescriptionConverter());
#pragma warning disable IL2026
        return JsonSerializer.Deserialize<T>(json, options);
#pragma warning restore IL2026
    }
}