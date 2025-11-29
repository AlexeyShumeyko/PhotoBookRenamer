using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;

namespace PhotoBookRenamer.Utils.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return value is bool boolValue && boolValue ? Visibility.Visible : Visibility.Collapsed;

            bool invert = parameter is string strParam && strParam == "true";
            bool boolValue = value is bool b && b;

            if (invert) boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}
