using System;
using System.Windows.Data;
using System.Windows.Media;

namespace PacketSender.Converter
{
    public class LogLevelEnum2ColorRgbConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LogLevel colorEnum = (LogLevel)value;

            switch (colorEnum)
            {
                case LogLevel.Error:
                    return new SolidColorBrush(Colors.Red);
                case LogLevel.Warning:
                    return new SolidColorBrush(Colors.DarkOrange);
                case LogLevel.Debug:
                    return new SolidColorBrush(Colors.SaddleBrown);
                case LogLevel.Info:
                    return new SolidColorBrush(Colors.Black);
                default:
                    throw new InvalidOperationException("Unknown color");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color color = (Color)value;

            if (color == Colors.Red)
                return LogLevel.Error;
            if (color == Colors.DarkOrange)
                return LogLevel.Warning;
            if (color == Colors.SaddleBrown)
                return LogLevel.Debug;
            if (color == Colors.Black)
                return LogLevel.Info;
            else
                throw new InvalidOperationException("Unknown color");
        }
    }
}
