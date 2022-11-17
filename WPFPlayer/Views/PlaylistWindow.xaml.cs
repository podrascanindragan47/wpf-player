using ModernWpf.Controls;
using System.Windows;
using WPFPlayer.ViewModels;

namespace WPFPlayer.Views
{
    /// <summary>
    /// Interaction logic for PlaylistWindow.xaml
    /// </summary>
    public partial class PlaylistWindow : Window
    {
        public PlaylistWindow()
        {
            InitializeComponent();
            Owner = App.Current.MainWindow;
        }

        private static PlaylistWindow _instance;
        public static PlaylistWindow Instance
        {
            get => _instance;
            set
            {
                _instance = value;
                MainViewModel.Instance.UpdateShowPlaylistWindow();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            (sender as AppBarButton).ContextMenu.IsOpen = true;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Instance = null;
        }
    }
}
