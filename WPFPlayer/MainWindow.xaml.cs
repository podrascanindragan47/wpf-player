using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Messaging;
using WPFPlayer.Messages;
using WPFPlayer.ViewModels;
using PInvoke;
using static PInvoke.User32;
using static PInvoke.Kernel32;

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

            _timerPreventSleep.Interval = 30000;
            _timerPreventSleep.Tick += timerPreventSleep_Tick;
            _timerPreventSleep.Start();
        }

        private IntPtr _windowHandle;

        private Timer _timerPreventSleep = new Timer();

        private Timer _timerTransparent = new Timer();
        private void setTransparent(bool isTransparent)
        {
            SetWindowLongFlags style = (SetWindowLongFlags)User32.GetWindowLong(_windowHandle, WindowLongIndexFlags.GWL_EXSTYLE);
            if (isTransparent)
            {
                Opacity = 0.3;
                style |= SetWindowLongFlags.WS_EX_TRANSPARENT;
                _timerTransparent.Start();
            }
            else
            {
                Opacity = 1;
                style &= ~SetWindowLongFlags.WS_EX_TRANSPARENT;
                _timerTransparent.Stop();
            }
            User32.SetWindowLong(_windowHandle, WindowLongIndexFlags.GWL_EXSTYLE, style);
        }
        private void timerTransparent_Tick(object sender, EventArgs e)
        {
            POINT point = User32.GetCursorPos();
            RECT winRect;
            User32.GetWindowRect(_windowHandle, out winRect);

            if(point.x >= winRect.left && point.x <= winRect.right
                && point.y >= winRect.top && point.y <= winRect.bottom)
            {
            }
            else
            {
                this.setTransparent(false);
            }
        }

        private void timerPreventSleep_Tick(object sender, EventArgs e)
        {
            if(Media.IsPlaying)
            {
                Kernel32.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_AWAYMODE_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
            }
        }

        public void Receive(SetTransparentMessage message)
        {
            this.setTransparent(true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;
        }

        private void Media_MediaOpening(object sender, Unosquare.FFME.Common.MediaOpeningEventArgs e)
        {
            MainViewModel.Instance.CurrentMediaOptions = e.Options;
        }

        private void Media_MediaClosed(object sender, EventArgs e)
        {
            MainViewModel.Instance.CurrentMediaOptions = null;
        }

        private void TitleBarButton_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }
    }
}
