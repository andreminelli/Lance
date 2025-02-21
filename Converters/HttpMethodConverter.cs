using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Net.Http;

namespace Lance.Converters
{
    class HttpMethodConverter : IValueConverter
    {
        public static readonly HttpMethodConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is HttpMethod method && targetType.IsAssignableTo(typeof(string)))
            {
                return method.Method.ToUpper();
            }

            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string methodName && targetType.IsAssignableTo(typeof(HttpMethod)))
            {
                return HttpMethod.Parse(methodName);
            }

            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }
    }
}
