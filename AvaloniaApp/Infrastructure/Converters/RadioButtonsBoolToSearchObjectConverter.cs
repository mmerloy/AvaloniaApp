using Avalonia.Data;
using Avalonia.Data.Converters;
using AvaloniaFirstApp.Models;
using System;
using System.Globalization;

namespace AvaloniaFirstApp.Infrastructure.Converters;

public class RadioButtonsBoolToSearchObjectConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is null || parameter is null)
            return null;
        
        SearchObjectType sOType = (SearchObjectType)value;
        if (sOType == Enum.Parse<SearchObjectType>(parameter.ToString()!))
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

        byte searchObjectType;

        if (!byte.TryParse(parameter.ToString(), out searchObjectType))
            return BindingOperations.DoNothing;

        if (bVal)
            return (SearchObjectType)searchObjectType;

        return BindingOperations.DoNothing;
    }
}
