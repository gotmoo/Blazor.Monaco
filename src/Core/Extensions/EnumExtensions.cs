using System.ComponentModel;
using System.Reflection;

namespace Blazor.Monaco;

public static class EnumExtensions
{
    public static string? GetOptionDescription<TEnum>(this TEnum value, bool lowercase = false)
        where TEnum : struct, Enum
    {
        const string fieldName = nameof(value);  
        var fieldInfo = typeof(TEnum).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        
        var description = Enum.GetName(typeof(TEnum), value);
        
        if (fieldInfo != null)
        {
            description = fieldInfo.GetCustomAttributes<DescriptionAttribute>(true)
                .FirstOrDefault()?.Description ?? description;  
        }

        return lowercase ? description?.ToLowerInvariant() : description;
    }
}