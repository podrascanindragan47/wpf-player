
using WPFPlayer.ViewModels;

namespace WPFPlayer.Messages
{
    public class PlayItemMessage
    {
        public PlayItemMessage(PlaylistItemViewModel item)
        {
            Item = item;
        }

        public PlaylistItemViewModel Item { get; private set; }
    }
}
 