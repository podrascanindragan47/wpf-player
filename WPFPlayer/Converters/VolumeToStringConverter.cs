
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPFPlayer.Converters
{
    public class VolumeToStringConverter : MarkupExtension, IValueConverter
    {
        private static VolumeToStringConverter instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value;
            return $"{(int)(v*100 + 0.5)}%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new VolumeToStringConverter());
        }
    }
}
