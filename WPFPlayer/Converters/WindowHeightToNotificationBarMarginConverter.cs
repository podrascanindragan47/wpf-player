﻿
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPFPlayer.Converters
{
    public class WindowHeightToNotificationBarMarginConverter : MarkupExtension, IValueConverter
    {
        private static WindowHeightToNotificationBarMarginConverter instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double h = Math.Max((double)value / 15, 16);
            return new Thickness(0, h, h, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new WindowHeightToNotificationBarMarginConverter());
        }
    }
}
