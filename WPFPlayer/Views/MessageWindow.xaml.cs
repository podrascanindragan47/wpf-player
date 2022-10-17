using System.Windows;

namespace WPFPlayer.Views
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            InitializeComponent();
            Owner = App.Current.MainWindow;
        }

        public string TextContent
        {
            get => txtContent.Text;
            set => txtContent.Text = value;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
