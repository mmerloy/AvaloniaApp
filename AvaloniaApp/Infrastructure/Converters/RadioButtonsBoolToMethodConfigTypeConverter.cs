using Avalonia.Data;
using Avalonia.Data.Converters;
using Domain.MethodConfigurations;
using System;
using System.Globalization;

namespace AvaloniaFirstApp.Infrastructure.Converters;

public class RadioButtonsBoolToMethodConfigTypeConverter : IValueConverter
{
    //1 - Interpol
    //2 - Weight
    //3 - Recursion
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
            return null;

        MethodConfigType configType = (MethodConfigType)value;

        if (configType == Enum.Parse<MethodConfigType>(parameter.ToString()!))
            return true;
        else
            return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
            return null;

        if (value is false)
            return BindingOperations.DoNothing;

        bool bVal = (bool)value;

        byte param;

        if (!byte.TryParse(parameter.ToString(), out param))
            return BindingOperations.DoNothing;

        if (bVal)
            return (MethodConfigType)param;

        return BindingOperations.DoNothing;
    }
}
