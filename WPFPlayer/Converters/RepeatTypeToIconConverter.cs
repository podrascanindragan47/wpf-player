
using ModernWpf.Controls;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using WPFPlayer.Helpers;

namespace WPFPlayer.Converters
{
    public class RepeatTypeToIconConverter : MarkupExtension, IValueConverter
    {
        private static RepeatTypeToIconConverter instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((RepeatType)value)
            {
                case RepeatType.One:
                    return new SymbolIcon(Symbol.RepeatOne);
                case RepeatType.All:
                    return new SymbolIcon(Symbol.RepeatAll);
                default:
                    return new SymbolIcon((Symbol)0xF5E7);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new RepeatTypeToIconConverter());
        }
    }
}
