//using Avalonia.Data.Converters;
//using System;
//using System.Globalization;

//namespace AvaloniaFirstApp.Views.Converters;

//public class ScaleAndPositionConverter : IValueConverter
//{
//    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
//    {


//        double fieldScale = (double)[0];

//        return (decimal?)value + (decimal?)parameter;
//    }

//    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
//    {
//        // If we want to convert back, we need to subtract instead of add.
//        return (decimal?)value - (decimal?)parameter;
//    }
//}
