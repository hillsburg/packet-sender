using System;
using System.Windows.Data;
using System.Windows.Media;

namespace PacketSender.Converter
{
    public class Rgb2SolidColorBrushConverter : IValueConverter
    {
        public SolidColorBrush DefaultValue { get; } = Brushes.Transparent;

        /// <summary>
        /// Convert RGB hex string to color
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return DefaultValue;
            }

            if (value is string rgbStr)
            {
                try
                {
                    if (string.IsNullOrEmpty(rgbStr))
                    {
                        return DefaultValue;
                    }

                    Color color = (Color)ColorConverter.ConvertFromString(rgbStr);
                    return new SolidColorBrush(color);
                }
                catch (Exception e)
                {
                    return DefaultValue;
                }
            }

            return DefaultValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var brush = value as SolidColorBrush;
            return brush?.Color.ToString();
        }
    }
}
