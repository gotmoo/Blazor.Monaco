using System.ComponentModel;
using System.Reflection;

namespace Blazor.Monaco;

public static class EnumExtensions
{
    public static string? GetOptionDescription<TEnum>(this TEnum value, bool lowercase = false)
        where TEnum : struct, IConvertible
    {
        if (!typeof(TEnum).IsEnum)
        {
            return null;
        }

        var description = value.ToString();

        FieldInfo? fieldInfo = value.GetType().GetField(value.ToString() ?? "");
        if (fieldInfo == null) return lowercase ? description?.ToLowerInvariant() : description;
        var attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
        if (attributes.Length > 0)
        {
            description = ((DescriptionAttribute)attributes[0]).Description;
        }

        return lowercase ? description?.ToLowerInvariant() : description;
    }
}