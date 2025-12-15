using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace ICSharpCode.ILSpy.Metadata
{
    public class ByteWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int b)
            {
                if (b >= 1024)
                    return $"{b / 1024} KB";
                return $"{b} B";
            }
            return value ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
