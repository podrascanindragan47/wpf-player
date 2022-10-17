
using System.Linq;

namespace WPFPlayer.Helpers
{
    public static class Constants
    {
        public static readonly string AutoUpdaterUrl = "https://raw.githubusercontent.com/podrascanindragan47/wpf-player/master/AutoUpdater.xml";
        public static readonly string HelpUrl = "https://github.com/podrascanindragan47/wpf-player";

        public static readonly string[] VideoFileExtensions =
        {
            "3g2",
            "3gp",
            "3gp2",
            "3gpp",
            "amv",
            "asf",
            "avi",
            "bik",
            "bin",
            "crf",
            "dav",
            "divx",
            "drc",
            "dv",
            "dvr-ms",
            "evo",
            "f4v",
            "flv",
            "gvi",
            "gxf",
            "iso",
            "m1v",
            "m2v",
            "m2t",
            "m2ts",
            "m4v",
            "mkv",
            "mov",
            "mp2",
            "mp2v",
            "mp4",
            "mp4v",
            "mpe",
            "mpeg",
            "mpeg1",
            "mpeg2",
            "mpeg4",
            "mpg",
            "mpv2",
            "mts",
            "mtv",
            "mxf",
            "mxg",
            "nsv",
            "nuv",
            "ogg",
            "ogm",
            "ogv",
            "ogx",
            "ps",
            "rec",
            "rm",
            "rmvb",
            "rpl",
            "thp",
            "tod",
            "tp",
            "ts",
            "tts",
            "txd",
            "vob",
            "vro",
            "webm",
            "wm",
            "wmv",
            "wtv",
            "xesc"
        };

        public static readonly string[] AudioFileExtensions =
        {
            "3ga",
            "669",
            "a52",
            "aac",
            "ac3",
            "adt",
            "adts",
            "aif",
            "aifc",
            "aiff",
            "amb",
            "amr",
            "aob",
            "ape",
            "au",
            "awb",
            "caf",
            "dts",
            "flac",
            "it",
            "kar",
            "m4a",
            "m4b",
            "m4p",
            "m5p",
            "mid",
            "mka",
            "mlp",
            "mod",
            "mpa",
            "mp1",
            "mp2",
            "mp3",
            "mpc",
            "mpga",
            "mus",
            "oga",
            "ogg",
            "oma",
            "opus",
            "qcp",
            "ra",
            "rmi",
            "s3m",
            "sid",
            "spx",
            "tak",
            "thd",
            "tta",
            "voc",
            "vqf",
            "w64",
            "wav",
            "wma",
            "wv",
            "xa",
            "xm"
        };

        public static readonly string[] PlaylistFileExtensions =
        {
            "asx",
            "b4s",
            "cue",
            "ifo",
            "m3u",
            "m3u8",
            "pls",
            "ram",
            "rar",
            "sdp",
            "vlc",
            "xspf",
            "wax",
            "wvx",
            "zip",
            "conf"
        };

        private static string[] _mediaFileExtensions;
        public static string[] MediaFileExtensions
        {
            get
            {
                if(_mediaFileExtensions == null)
                {
                    _mediaFileExtensions = VideoFileExtensions.Concat(AudioFileExtensions).Concat(PlaylistFileExtensions).ToArray();
                }
                return _mediaFileExtensions;
            }
        }


        public static string getOpenFileFilter()
        {
            string filter = $"Media Files ( *." + string.Join(" *.", MediaFileExtensions);
            filter += " )|*." + string.Join(";*.", MediaFileExtensions);

            filter += "|Video Files ( *." + string.Join(" *.", VideoFileExtensions);
            filter += " )|*." + string.Join(";*.", VideoFileExtensions);

            filter += "|Audio Files ( *." + string.Join(" *.", AudioFileExtensions);
            filter += " )|*." + string.Join(";*.", AudioFileExtensions);

            filter += "|Playlist Files ( *." + string.Join(" *.", PlaylistFileExtensions);
            filter += " )|*." + string.Join(";*.", PlaylistFileExtensions);

            return filter;
        }
    }
}
