using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPFPlayer.Converters
{
    public class WindowHeightToNotificationBarFontSizeConverter : MarkupExtension, IValueConverter
    {
        private static WindowHeightToNotificationBarFontSizeConverter instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double h = Math.Max((double)value / 15, 16);
            return h;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new WindowHeightToNotificationBarFontSizeConverter());
        }
    }
}
