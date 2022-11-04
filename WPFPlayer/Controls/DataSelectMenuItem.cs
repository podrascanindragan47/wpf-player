using System.Windows;
using System.Windows.Controls;

namespace WPFPlayer.Controls
{
    public class DataSelectMenuItem : MenuItem
    {
        public DataSelectMenuItem()
        {
            IsCheckable = true;
        }

        private object _value;
        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                Header = value;
                IsChecked = value == SelectedData;
            }
        }

        public static readonly DependencyProperty SelectedDataProperty = DependencyProperty.Register(
            nameof(SelectedData),
            typeof(object),
            typeof(DataSelectMenuItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDataChanged));
        private static void OnSelectedDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataSelectMenuItem view = (DataSelectMenuItem)d;
            view.IsChecked = e.NewValue == view.Value;
        }
        public object SelectedData
        {
            get => GetValue(SelectedDataProperty);
            set => SetValue(SelectedDataProperty, value);
        }

        protected override void OnClick()
        {
            base.OnClick();
            SelectedData = Value;
        }
    }
}
