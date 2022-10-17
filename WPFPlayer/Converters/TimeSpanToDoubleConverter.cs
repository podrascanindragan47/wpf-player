using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPFPlayer.Converters
{
    public class TimeSpanToDoubleConverter : MarkupExtension, IValueConverter
    {
        private static TimeSpanToDoubleConverter instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan d = (TimeSpan)value;
            return d.TotalMilliseconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value;
            return TimeSpan.FromMilliseconds(v);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new TimeSpanToDoubleConverter());
        }
    }
}
