using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blazor.Monaco;
public class EnumDescriptionConverter : JsonConverter<Enum>
{
    /// Determines whether the specified type can be converted by this converter.
    /// <param name="typeToConvert">The type to evaluate for compatibility with this converter.</param>
    /// <returns>True if the type is an Enum; otherwise, false.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

#pragma warning disable IL2092
    /// Reads and converts a JSON string into an Enum value based on its description or name.
    /// <param name="reader">The Utf8JsonReader to read the JSON data from.</param>
    /// <param name="typeToConvert">The type of the Enum to convert the JSON string to.</param>
    /// <param name="options">The JsonSerializerOptions used during deserialization.</param>
    /// <returns>The Enum value corresponding to the JSON string representation.
    /// Throws JsonException if the conversion fails.</returns>
    public override Enum Read(ref Utf8JsonReader reader, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type typeToConvert, JsonSerializerOptions options)
#pragma warning restore IL2092
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        string? description = reader.GetString();
        if (description == null)
        {
            throw new JsonException("Enum description is null.");
        }

        foreach (var field in typeToConvert.GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == description)
                {
                    return (Enum)field.GetValue(null)!;
                }
            }
            else if (field.Name == description)
            {
                return (Enum)field.GetValue(null)!;
            }
        }

        throw new JsonException($"Unable to convert \"{description}\" to Enum \"{typeToConvert}\".");
    }

    /// Writes the string representation of an Enum value to a JSON writer, using its description if available.
    /// <param name="writer">The Utf8JsonWriter to write the JSON data to.</param>
    /// <param name="value">The Enum value to convert to a JSON string.</param>
    /// <param name="options">The JsonSerializerOptions used during serialization.</param>
    public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field != null && Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
        {
            writer.WriteStringValue(attribute.Description);
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}