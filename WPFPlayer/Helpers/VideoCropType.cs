
using System.Collections.Generic;

namespace WPFPlayer.Helpers
{
    public class VideoCropType
    {
        public float kw { get; private set; }
        public float kh { get; private set; }

        private VideoCropType(float kx, float ky)
        {
            this.kw = kx;
            this.kh = ky;
        }

        public bool IsDefault => kw == 0 || kh == 0;

        public override string ToString()
        {
            if (IsDefault)
            {
                return "Default";
            }

            return $"{kw}:{kh}";
        }

        public string GetVideoFilter(int width, int height)
        {
            if(IsDefault)
            {
                return string.Empty;
            }

            if (height / kh * kw < width)
            {
                return $"crop=in_h*{kw}/{kh}:in_h";
            }
            else
            {
                return $"crop=in_w:in_w*{kh}/{kw}";
            }
        }

        public static readonly VideoCropType Default = new VideoCropType(0, 0);
        public static readonly VideoCropType C16x10 = new VideoCropType(16, 10);
        public static readonly VideoCropType C16x9 = new VideoCropType(16, 9);
        public static readonly VideoCropType C4x3 = new VideoCropType(4, 3);
        public static readonly VideoCropType C1p85x1 = new VideoCropType(1.85f, 1);
        public static readonly VideoCropType C2p21x1 = new VideoCropType(2.21f, 1);
        public static readonly VideoCropType C2p35x1 = new VideoCropType(2.35f, 1);
        public static readonly VideoCropType C2p39x1 = new VideoCropType(2.39f, 1);
        public static readonly VideoCropType C5x3 = new VideoCropType(5, 3);
        public static readonly VideoCropType C5x4 = new VideoCropType(5, 4);
        public static readonly VideoCropType C1x1 = new VideoCropType(1, 1);

        public static readonly List<VideoCropType> All = new List<VideoCropType>
        {
            Default,
            C16x10,
            C16x9,
            C4x3,
            C1p85x1,
            C2p21x1,
            C2p35x1,
            C2p39x1,
            C5x3,
            C5x4,
            C1x1
        };
    }
}
