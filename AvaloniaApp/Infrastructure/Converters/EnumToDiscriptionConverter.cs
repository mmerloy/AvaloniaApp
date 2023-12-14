using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Utils;

namespace AvaloniaFirstApp.Infrastructure.Converters;

public class EnumToDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ((Enum)value!).GetDescriptionFromEnumValue();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
