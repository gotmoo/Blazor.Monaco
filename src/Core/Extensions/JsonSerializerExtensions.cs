using System.Text.Json;

namespace Blazor.Monaco;

public static class JsonSerializerExtensions
{
    /// Serializes an object to a JSON string and converts Enum values to their description attributes if available.
    /// <typeparam name="T">The type of the object being serialized.</typeparam>
    /// <param name="obj">The object instance to serialize.</param>
    /// <param name="writeIndented">Specifies whether the output JSON should be indented. Defaults to false.</param>
    /// <returns>A JSON string representation of the object with Enum values represented by their description attributes, if specified.</returns>
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

    /// Deserializes a JSON string into an object of the specified type and maps Enum values using their description attributes if available.
    /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An instance of the specified type populated with data from the JSON string, with Enum values resolved to their description attributes if applicable.</returns>
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