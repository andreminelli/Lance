using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Lance.ViewModels;

namespace Lance.Converters;

public class HeaderKeyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RequestHeader requestHeader && targetType.IsAssignableTo(typeof(string)))
        {
            return requestHeader.Key;
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}