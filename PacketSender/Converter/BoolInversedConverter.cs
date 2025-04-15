using System;
using System.Windows.Data;

namespace PacketSender.Converter
{
    public class BoolInversedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool boolVal = (bool)value;
            return !boolVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool boolVal = (bool)value;
            return !boolVal;
        }
    }
}
