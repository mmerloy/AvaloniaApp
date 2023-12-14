using System.ComponentModel;
using System.Reflection;

namespace Utils;

public static class EnumExtension
{
    public static string GetDescriptionFromEnumValue(this Enum value)
    {
        value.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false);
        DescriptionAttribute? attribute = 
            value.GetType()
            .GetField(value.ToString())!
            .GetCustomAttributes(typeof(DescriptionAttribute), false)
            .SingleOrDefault() as DescriptionAttribute;
        return attribute == null ? value.ToString() : attribute.Description;
    }

    public static T? GetEnumValueFromDescriptionOrDefault<T>(string description)
    {
        var type = typeof(T);
        if (!type.IsEnum)
            throw new ArgumentException();
        FieldInfo[] fields = type.GetFields();
        var field = fields
                        .SelectMany(f => f.GetCustomAttributes(
                            typeof(DescriptionAttribute), false), (
                                f, a) => new { Field = f, Att = a })
                        .Where(a => ((DescriptionAttribute)a.Att)
                            .Description == description).SingleOrDefault();
        return field == null ? default : (T?)field.Field.GetRawConstantValue();
    }
}

