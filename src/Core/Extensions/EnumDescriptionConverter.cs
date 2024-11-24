using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blazor.Monaco;
public class EnumDescriptionConverter : JsonConverter<Enum>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override Enum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string description = reader.GetString();
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
                    return (Enum)field.GetValue(null);
                }
            }
            else if (field.Name == description)
            {
                return (Enum)field.GetValue(null);
            }
        }

        throw new JsonException($"Unable to convert \"{description}\" to Enum \"{typeToConvert}\".");
    }

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