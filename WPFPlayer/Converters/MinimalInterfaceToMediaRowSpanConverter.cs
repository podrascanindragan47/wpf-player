
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPFPlayer.Converters
{
    public class MinimalInterfaceToMediaRowSpanConverter : MarkupExtension, IValueConverter
    {
        private static MinimalInterfaceToMediaRowSpanConverter instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool v = (bool)value;
            return v ? 2 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new MinimalInterfaceToMediaRowSpanConverter());
        }
    }
}
