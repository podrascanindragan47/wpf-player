using System;
using System.Windows;
using System.Windows.Interop;
using WinApi.Windows;
using NativeWindow = WinApi.Windows.NativeWindow;
using WinApi.User32;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Messaging;
using WPFPlayer.Messages;
using WPFPlayer.ViewModels;

namespace WPFPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,
        IRecipient<SetTransparentMessage>
    {
        public MainWindow()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.RegisterAll(this);

            MainViewModel.Instance.Media = Media;
            
            _timerTransparent.Interval = 1000;
            _timerTransparent.Tick += timerTransparent_Tick;
        }

        private NativeWindow nativeWindow;

        private Timer _timerTransparent = new Timer();
        private void setTransparent(bool isTransparent)
        {
            var style = nativeWindow.GetExStyles();
            if(isTransparent)
            {
                Opacity = 0.3;
                style |= WindowExStyles.WS_EX_TRANSPARENT;
                _timerTransparent.Start();
            }
            else
            {
                Opacity = 1;
                style &= ~WindowExStyles.WS_EX_TRANSPARENT;
                _timerTransparent.Stop();
            }
            nativeWindow.SetExStyles(style);
        }
        private void timerTransparent_Tick(object sender, EventArgs e)
        {
            NetCoreEx.Geometry.Point point;
            User32Methods.GetCursorPos(out point);
            NetCoreEx.Geometry.Rectangle winRect;
            User32Methods.GetWindowRect(nativeWindow.Handle, out winRect);

            if(point.X >= winRect.Left && point.X <= winRect.Right
                && point.Y >= winRect.Top && point.Y <= winRect.Bottom)
            {
            }
            else
            {
                this.setTransparent(false);
            }
        }
        public void Receive(SetTransparentMessage message)
        {
            this.setTransparent(true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            nativeWindow = WindowFactory.CreateWindowFromHandle(new WindowInteropHelper(this).Handle);
        }

        private void Media_MediaOpening(object sender, Unosquare.FFME.Common.MediaOpeningEventArgs e)
        {
            MainViewModel.Instance.CurrentMediaOptions = e.Options;
        }

        private void Media_MediaClosed(object sender, EventArgs e)
        {
            MainViewModel.Instance.CurrentMediaOptions = null;
        }

        public void SetForegroundWindow()
        {
            User32Methods.SetForegroundWindow(nativeWindow.Handle);
        }

        private void TitleBarButton_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }
    }
}
