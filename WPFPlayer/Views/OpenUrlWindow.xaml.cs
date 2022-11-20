using System;
using System.Windows;

namespace WPFPlayer.Views
{
    /// <summary>
    /// Interaction logic for OpenUrlWindow.xaml
    /// </summary>
    public partial class OpenUrlWindow : Window
    {
        public OpenUrlWindow()
        {
            InitializeComponent();
            Owner = App.Current.MainWindow;

            txtURL.Focus();
            string clipboard = Clipboard.GetText();
            if(!string.IsNullOrWhiteSpace(clipboard))
            {
                Uri uri;
                if(Uri.TryCreate(clipboard, UriKind.RelativeOrAbsolute, out uri))
                {
                    txtURL.Text = clipboard;
                    txtURL.SelectAll();
                }
            }
        }

        public string URL
        {
            get => txtURL.Text;
            set => txtURL.Text = value;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
