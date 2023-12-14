using Avalonia.Data;
using Avalonia.Data.Converters;
using AvaloniaFirstApp.ViewModels;
using System;
using System.Globalization;

namespace AvaloniaFirstApp.Infrastructure.Converters;

public class RadioButtonsBoolToMethodConfigObjectConverter : IValueConverter
{
    //1 - Interpol
    //2 - Weight
    //3 - Recursion
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null || value is not MethodConfigurationViewModel)
            return false;

        if (object.ReferenceEquals(parameter, value))
            return true;
        else
            return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
            return parameter;

        return BindingOperations.DoNothing;
    }
}
