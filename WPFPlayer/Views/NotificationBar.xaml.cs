using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Controls;
using System.Windows.Forms;
using WPFPlayer.Messages;

namespace WPFPlayer.Views
{
    /// <summary>
    /// Interaction logic for NotificationBar.xaml
    /// </summary>
    public partial class NotificationBar : TextBlock,
        IRecipient<NotificationBarMessage>
    {
        private Timer _timerHideNotification;
        public NotificationBar()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.RegisterAll(this);

            _timerHideNotification = new Timer();
            _timerHideNotification.Interval = 2000;
            _timerHideNotification.Tick += timerHideNotification_Tick;
        }

        private void timerHideNotification_Tick(object sender, System.EventArgs e)
        {
            Text = string.Empty;
            _timerHideNotification.Stop();
        }

        public void Receive(NotificationBarMessage message)
        {
            _timerHideNotification.Stop();
            _timerHideNotification.Start();
            Text = message.Message;
        }
    }
}
