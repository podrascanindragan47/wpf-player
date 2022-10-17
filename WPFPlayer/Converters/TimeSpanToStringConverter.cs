using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPFPlayer.Converters
{
    public class TimeSpanToStringConverter : MarkupExtension, IValueConverter
    {
        private static TimeSpanToStringConverter instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan d = (TimeSpan)value;
            return d.ToString(@"hh\:mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new TimeSpanToStringConverter());
        }
    }
}
