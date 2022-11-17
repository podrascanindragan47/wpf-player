
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Unosquare.FFME.Common;
using WPFPlayer.Messages;
using WPFPlayer.Models;

namespace WPFPlayer.ViewModels
{
    public class PlaylistItemViewModel : ObservableRecipient
    {
        public PlaylistItemViewModel(PlaylistItem data)
        {
            Data = data;
        }

        public PlaylistItem Data { get; private set; }

        public string Title
        {
            get
            {
                Match m = Regex.Match(Data.MediaSource.OriginalString, @"^(?:.*[\\\/]|)([^\\\/]+)[\\\/]?$");
                if(m.Success)
                {
                    return m.Groups[1].Value;
                }
                return string.Empty;
            }
        }

        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get => _thumbnail;
            set
            {
                SetProperty(ref _thumbnail, value);
            }
        }

        private MediaInfo _mediaInfo;
        public MediaInfo MediaInfo
        {
            get => _mediaInfo;
            set
            {
                SetProperty(ref _mediaInfo, value);

                OnPropertyChanged(nameof(Duration));
            }
        }

        private TimeSpan _duration = TimeSpan.Zero;
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                SetProperty(ref _duration, value);
            }
        }

        private bool _isViewed;
        public bool IsViewed
        {
            get => _isViewed;
            set
            {
                SetProperty(ref _isViewed, value);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if(!SetProperty(ref _isSelected, value))
                {
                    return;
                }

                PlaylistViewModel.Instance.RemoveItemsCommand.NotifyCanExecuteChanged();
                PlaylistViewModel.Instance.MoveUpCommand.NotifyCanExecuteChanged();
                PlaylistViewModel.Instance.MoveDownCommand.NotifyCanExecuteChanged();
            }
        }

        public FontWeight TextFontWeight => PlaylistViewModel.Instance.CurrentItem == this ? FontWeight.FromOpenTypeWeight(700) : FontWeight.FromOpenTypeWeight(400);
        public void UpdateTextFontWeight()
        {
            OnPropertyChanged(nameof(TextFontWeight));
        }

        public bool IsExtractedInfo { get; set; } = false;
        public void ExtractInformation()
        {
            IsExtractedInfo = true;
            using (VideoCapture capture = new VideoCapture(Data.MediaSource.OriginalString))
            using(Mat frame = new Mat())
            using (Mat smallFrame = new Mat())
            {
                int fps = (int)capture.Get(CapProp.Fps);
                if(fps == 0)
                {
                    return;
                }
                int frame_count = (int)capture.Get(CapProp.FrameCount);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Duration = TimeSpan.FromSeconds(frame_count / fps);
                });

                capture.Set(CapProp.PosMsec, Math.Min(frame_count / fps * 1000 / 2, 3000));
                if(capture.Read(frame))
                {
                    double k = Math.Max(64.0f / frame.Width, 36.0f / frame.Height);
                    CvInvoke.Resize(frame, smallFrame, new System.Drawing.Size(0, 0), k, k);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Thumbnail = convertBitmap(smallFrame.ToBitmap());
                    });
                }
            }
        }

        private BitmapImage convertBitmap(System.Drawing.Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private RelayCommand _doubleClickCommand;
        public RelayCommand DoubleClickCommand => _doubleClickCommand ?? (_doubleClickCommand = new RelayCommand(() =>
        {
            if(PlaylistViewModel.Instance.CurrentItem != this)
            {
                Messenger.Send(new PlayItemMessage(this));
            }
        }));
    }
}
