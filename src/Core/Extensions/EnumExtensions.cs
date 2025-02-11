using System.ComponentModel;
using System.Reflection;

namespace Blazor.Monaco;

public static class EnumExtensions
{
    /// <summary>
    /// Retrieves the description of an enumeration value, as defined by the <see cref="DescriptionAttribute"/>.
    /// If no description is provided, it falls back to the enumeration value name.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type.</typeparam>
    /// <param name="value">The enumeration value for which the description is retrieved.</param>
    /// <param name="lowercase">Indicates whether the returned description should be converted to lowercase.</param>
    /// <returns>
    /// The description associated with the enumeration value from the <see cref="DescriptionAttribute"/>,
    /// or the enumeration value name if no description is provided.
    /// </returns>
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