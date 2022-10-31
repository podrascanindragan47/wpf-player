using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPFPlayer.Helpers;
using WPFPlayer.Messages;
using WPFPlayer.Models;
using WPFPlayer.Properties;
using WPFPlayer.Views;
using ModernWpf.Controls;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Unosquare.FFME;
using Unosquare.FFME.Common;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using AutoUpdaterDotNET;
using System.Diagnostics;

namespace WPFPlayer.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        private static MainViewModel _instance;
        public static MainViewModel Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new MainViewModel();
                }
                return _instance;
            }
        }

        private async void initialize()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
            {
                string directory = Path.GetDirectoryName(args[1]);
                string[] files = Directory.GetFiles(directory)
                    .Where(x => Constants.MediaFileExtensions.Any(y => x.EndsWith("." + y, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();
                for (int i = 0; i < files.Length; i++)
                {
                    Playlist.Medias.Add(new FileMedia
                    {
                        Path = files[i]
                    });

                    if (files[i] == args[1])
                    {
                        CurrentPlayIndex = i;
                    }
                }

                await startMedia();
                PlayPauseCommand.NotifyCanExecuteChanged();
            }

            _timerHideControls.Interval = 2000;
            _timerHideControls.Tick += timerHideControls_Tick;
        }

        private void timerHideControls_Tick(object sender, EventArgs e)
        {
            if (IsMinimalInterface)
            {
                IsVisibleUIs = false;
            }
            _timerHideControls.Stop();
        }

        private Timer _timerHideControls = new Timer();

        private MediaElement _media = null;
        public MediaElement Media
        {
            get => _media;
            set
            {
                bool isNotInitialized = _media == null;
                _media = value;
                if(isNotInitialized)
                {
                    initialize();
                }
            }
        }

        private Playlist _playlist = new Playlist();
        public Playlist Playlist
        {
            get => _playlist;
            set => SetProperty(ref _playlist, value);
        }

        private int _currentPlayIndex = 0;
        public int CurrentPlayIndex
        {
            get => _currentPlayIndex;
            set
            {
                SetProperty(ref _currentPlayIndex, value);

                NextMediaCommand.NotifyCanExecuteChanged();
                PreviousMediaCommand.NotifyCanExecuteChanged();
            }
        }

        private TimeSpan _seekTime = TimeSpan.Zero;
        public TimeSpan SeekTime
        {
            get => _seekTime;
            set => SetProperty(ref _seekTime, value);
        }

        private TimeSpan _totalTime = TimeSpan.Zero;
        public TimeSpan TotalTime
        {
            get => _totalTime;
            set => SetProperty(ref _totalTime, value);
        }

        public double Volume
        {
            get => Settings.Default.Volume;
            set
            {
                if(Settings.Default.Volume == value)
                {
                    return;
                }

                Settings.Default.Volume = value;

                OnPropertyChanged(nameof(Volume));
                OnPropertyChanged(nameof(VolumeIcon));
            }
        }

        private bool _isMuted = false;
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (!SetProperty(ref _isMuted, value))
                {
                    return;
                }
                OnPropertyChanged(nameof(VolumeIcon));

                if (value)
                {
                    Messenger.Send(new NotificationBarMessage
                    {
                        Message = "Mute"
                    });
                }
                else
                {
                    showVolumeNotification();
                }
            }
        }

        private bool _isVisibleUIs = false;
        public bool IsVisibleUIs
        {
            get => _isVisibleUIs;
            set => SetProperty(ref _isVisibleUIs, value);
        }

        public double Width
        {
            get => Settings.Default.Width;
            set
            {
                if (WindowState != WindowState.Maximized)
                {
                    Settings.Default.Width = value;
                }
                OnPropertyChanged(nameof(Width));
            }
        }
        public double Height
        {
            get => Settings.Default.Height;
            set
            {
                if (WindowState != WindowState.Maximized)
                {
                    Settings.Default.Height = value;
                }
                OnPropertyChanged(nameof(Height));
            }
        }
        public WindowState WindowState
        {
            get => Settings.Default.WindowState;
            set
            {
                if(Settings.Default.WindowState == value)
                {
                    return;
                }
                Settings.Default.WindowState = value;
                OnPropertyChanged(nameof(WindowState));
                OnPropertyChanged(nameof(IsMinimalInterface));
            }
        }

        private DateTime _minimizeOnMouseEnterTime;
        private bool _minimizeOnMouseEnter = false;
        public bool MinimizeOnMouseEnter
        {
            get => _minimizeOnMouseEnter;
            set
            {
                if(!SetProperty(ref _minimizeOnMouseEnter, value))
                {
                    return;
                }

                if(value)
                {
                    _minimizeOnMouseEnterTime = DateTime.Now;
                }
            }
        }

        private bool _transparentOnMouseEnter = false;
        public bool TransparentOnMouseEnter
        {
            get => _transparentOnMouseEnter;
            set
            {
                if(!SetProperty(ref _transparentOnMouseEnter, value))
                {
                    return;
                }

                recreateMainWindow();
            }
        }

        public IconElement VolumeIcon
        {
            get
            {
                if(IsMuted)
                {
                    return new SymbolIcon(Symbol.Mute);
                }
                if(Volume < 0.25)
                {
                    return new SymbolIcon((Symbol)0xE992);
                }
                else if(Volume < 0.5)
                {
                    return new SymbolIcon((Symbol)0xE993);
                }
                else if(Volume < 0.75)
                {
                    return new SymbolIcon((Symbol)0xE994);
                }
                return new SymbolIcon(Symbol.Volume);
            }
        }

        private MediaOptions _currentMediaOptions;
        public MediaOptions CurrentMediaOptions
        {
            get => _currentMediaOptions;
            set => SetProperty(ref _currentMediaOptions, value);
        }

        private VideoCropType _videoCrop;
        public VideoCropType VideoCrop
        {
            get => _videoCrop;
            set
            {
                if(!SetProperty(ref _videoCrop, value))
                {
                    return;
                }

                updateVideoFilter();

                OnPropertyChanged(nameof(IsCropDefault));
                OnPropertyChanged(nameof(IsCrop16x10));
                OnPropertyChanged(nameof(IsCrop16x9));
                OnPropertyChanged(nameof(IsCrop4x3));
                OnPropertyChanged(nameof(IsCrop1p85x1));
                OnPropertyChanged(nameof(IsCrop2p21x1));
                OnPropertyChanged(nameof(IsCrop2p35x1));
                OnPropertyChanged(nameof(IsCrop2p39x1));
                OnPropertyChanged(nameof(IsCrop5x3));
                OnPropertyChanged(nameof(IsCrop5x4));
                OnPropertyChanged(nameof(IsCrop1x1));
            }
        }

        public bool IsCropDefault
        {
            get => VideoCrop == VideoCropType.Default;
            set
            {
                if (VideoCrop == VideoCropType.Default)
                {
                    return;
                }

                VideoCrop = VideoCropType.Default;
                showCropNotification();
            }
        }
        public bool IsCrop16x10
        {
            get => VideoCrop == VideoCropType.C16x10;
            set
            {
                if (VideoCrop == VideoCropType.C16x10)
                {
                    return;
                }

                VideoCrop = VideoCropType.C16x10;
                showCropNotification();
            }
        }
        public bool IsCrop16x9
        {
            get => VideoCrop == VideoCropType.C16x9;
            set
            {
                if (VideoCrop == VideoCropType.C16x9)
                {
                    return;
                }

                VideoCrop = VideoCropType.C16x9;
                showCropNotification();
            }
        }
        public bool IsCrop4x3
        {
            get => VideoCrop == VideoCropType.C4x3;
            set
            {
                if (VideoCrop == VideoCropType.C4x3)
                {
                    return;
                }

                VideoCrop = VideoCropType.C4x3;
                showCropNotification();
            }
        }
        public bool IsCrop1p85x1
        {
            get => VideoCrop == VideoCropType.C1p85x1;
            set
            {
                if (VideoCrop == VideoCropType.C1p85x1)
                {
                    return;
                }

                VideoCrop = VideoCropType.C1p85x1;
                showCropNotification();
            }
        }
        public bool IsCrop2p21x1
        {
            get => VideoCrop == VideoCropType.C2p21x1;
            set
            {
                if (VideoCrop == VideoCropType.C2p21x1)
                {
                    return;
                }

                VideoCrop = VideoCropType.C2p21x1;
                showCropNotification();
            }
        }
        public bool IsCrop2p35x1
        {
            get => VideoCrop == VideoCropType.C2p35x1;
            set
            {
                if (VideoCrop == VideoCropType.C2p35x1)
                {
                    return;
                }

                VideoCrop = VideoCropType.C2p35x1;
                showCropNotification();
            }
        }
        public bool IsCrop2p39x1
        {
            get => VideoCrop == VideoCropType.C2p39x1;
            set
            {
                if (VideoCrop == VideoCropType.C2p39x1)
                {
                    return;
                }

                VideoCrop = VideoCropType.C2p39x1;
                showCropNotification();
            }
        }
        public bool IsCrop5x3
        {
            get => VideoCrop == VideoCropType.C5x3;
            set
            {
                if (VideoCrop == VideoCropType.C5x3)
                {
                    return;
                }

                VideoCrop = VideoCropType.C5x3;
                showCropNotification();
            }
        }
        public bool IsCrop5x4
        {
            get => VideoCrop == VideoCropType.C5x4;
            set
            {
                if (VideoCrop == VideoCropType.C5x4)
                {
                    return;
                }

                VideoCrop = VideoCropType.C5x4;
                showCropNotification();
            }
        }
        public bool IsCrop1x1
        {
            get => VideoCrop == VideoCropType.C1x1;
            set
            {
                if (VideoCrop == VideoCropType.C1x1)
                {
                    return;
                }

                VideoCrop = VideoCropType.C1x1;
                showCropNotification();
            }
        }

        public IconElement PlayPauseIcon => Media.MediaState != MediaPlaybackState.Play ? new SymbolIcon((Symbol)0xF5B0) : new SymbolIcon((Symbol)0xF8AE);
        public string PlayPauseLabel => Media.MediaState != MediaPlaybackState.Play ? "Play" : "Pause";

        public bool IsMinimalInterface
        {
            get
            {
                if(WindowState == WindowState.Maximized)
                {
                    return true;
                }
                return Settings.Default.IsMinimalInterface;
            }
            set
            {
                if(Settings.Default.IsMinimalInterface == value)
                {
                    return;
                }
                Settings.Default.IsMinimalInterface = value;
                OnPropertyChanged(nameof(IsMinimalInterface));
                if(WindowState != WindowState.Maximized)
                {
                    if (IsMinimalInterface)
                    {
                        Height -= 124;
                    }
                    else
                    {
                        Height += 124;
                    }
                }
            }
        }

        private RelayCommand _openFilesCommand;
        public RelayCommand OpenFilesCommand => _openFilesCommand ?? (_openFilesCommand = new RelayCommand(async () =>
        {
            using (OpenFileDialog dlg = new OpenFileDialog()
            {
                Filter = Constants.getOpenFileFilter(),
                Multiselect = true,
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    int i = Playlist.Medias.Count;

                    Playlist.Medias.AddRange(dlg.FileNames.Select(x => new FileMedia
                    {
                        Path = x
                    }));

                    CurrentPlayIndex = i;

                    await startMedia();

                    PlayPauseCommand.NotifyCanExecuteChanged();
                }
            }
        }));

        private RelayCommand _openURLCommand;
        public RelayCommand OpenURLCommand => _openURLCommand ?? (_openURLCommand = new RelayCommand(async () =>
        {
            OpenUrlWindow dlg = new OpenUrlWindow();
            if(dlg.ShowDialog().Value)
            {
                Playlist.Medias.Add(new UrlMedia
                {
                    URL = dlg.URL,
                });
                CurrentPlayIndex = Playlist.Medias.Count - 1;

                await startMedia();

                PlayPauseCommand.NotifyCanExecuteChanged();
            }
        }));

        private RelayCommand _openFolderCommand;
        public RelayCommand OpenFolderCommand => _openFolderCommand ?? (_openFolderCommand = new RelayCommand(async () =>
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.ShowNewFolderButton = false;
                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    string[] files = Directory.GetFiles(dlg.SelectedPath)
                        .Where(x => Constants.MediaFileExtensions.Any(y => x.EndsWith("." + y, StringComparison.OrdinalIgnoreCase)))
                        .ToArray();
                    Playlist.Medias.AddRange(files.Select(x => new FileMedia
                    {
                        Path = x
                    }));
                    CurrentPlayIndex = Playlist.Medias.Count - files.Length;
                    await startMedia();
                    PlayPauseCommand.NotifyCanExecuteChanged();
                }
            }
        }));

        private RelayCommand _nextMediaCommand;
        public RelayCommand NextMediaCommand => _nextMediaCommand ?? (_nextMediaCommand = new RelayCommand(
            async () =>
            {
                await playNextMedia();
            },
            () => Playlist.Medias.Count > 0 && CurrentPlayIndex < Playlist.Medias.Count - 1));

        private RelayCommand _previousMediaCommand;
        public RelayCommand PreviousMediaCommand => _previousMediaCommand ?? (_previousMediaCommand = new RelayCommand(
            async () =>
            {
                await playPreviousMedia();
            },
            () => Playlist.Medias.Count > 0 && CurrentPlayIndex > 0));

        private RelayCommand _playPauseCommand;
        public RelayCommand PlayPauseCommand => _playPauseCommand ?? (_playPauseCommand = new RelayCommand(
            async () =>
            {
                switch (Media.MediaState)
                {
                    case MediaPlaybackState.Play:
                        await Media.Pause();
                        break;
                    case MediaPlaybackState.Close:
                        await startMedia();
                        break;
                    default:
                        await Media.Play();
                        break;
                }
            },
            () => Playlist.Medias.Count > 0));

        private RelayCommand<MediaStateChangedEventArgs> _mediaStateChangedCommand;
        public RelayCommand<MediaStateChangedEventArgs> MediaStateChangedCommand => _mediaStateChangedCommand ?? (_mediaStateChangedCommand = new RelayCommand<MediaStateChangedEventArgs>((e) =>
        {
            OnPropertyChanged(nameof(PlayPauseIcon));
            OnPropertyChanged(nameof(PlayPauseLabel));
            StopCommand.NotifyCanExecuteChanged();
        }));

        private RelayCommand _stopCommand;
        public RelayCommand StopCommand => _stopCommand ?? (_stopCommand = new RelayCommand(
            async () =>
            {
                await Media.Close();
                CurrentPlayIndex = 0;
            },
            () => Media.IsPlaying || Media.IsPaused));

        private RelayCommand _toggleFullScreenCommand;
        public RelayCommand ToggleFullScreenCommand => _toggleFullScreenCommand ?? (_toggleFullScreenCommand = new RelayCommand(() =>
        {
            if(WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }));

        private RelayCommand _toggleMuteCommand;
        public RelayCommand ToggleMuteCommand => _toggleMuteCommand ?? (_toggleMuteCommand = new RelayCommand(() =>
        {
            IsMuted = !IsMuted;
        }));

        private RelayCommand<MouseWheelEventArgs> _wheelCommand;
        public RelayCommand<MouseWheelEventArgs> WheelCommand => _wheelCommand ?? (_wheelCommand = new RelayCommand<MouseWheelEventArgs>((e) =>
        {
            if(e.Delta > 0)
            {
                Volume = Math.Min(Volume + 0.05, 1.0);
            }
            else
            {
                Volume = Math.Max(Volume - 0.05, 0);
            }

            showVolumeNotification();
        }));

        private RelayCommand<MouseEventArgs> _mouseMoveCommand;
        public RelayCommand<MouseEventArgs> MouseMoveCommand => _mouseMoveCommand ?? (_mouseMoveCommand = new RelayCommand<MouseEventArgs>((e) =>
        {
            IsVisibleUIs = true;
            if (IsMinimalInterface)
            {
                _timerHideControls.Stop();
                _timerHideControls.Start();
            }
        }));

        private RelayCommand<MouseEventArgs> _mouseEnterCommand;
        public RelayCommand<MouseEventArgs> MouseEnterCommand => _mouseEnterCommand ?? (_mouseEnterCommand = new RelayCommand<MouseEventArgs>(async (e) =>
        {
            if(Media.MediaState == MediaPlaybackState.Play && MinimizeOnMouseEnter && (DateTime.Now - _minimizeOnMouseEnterTime).TotalSeconds > 2)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                {
                    WindowState = WindowState.Minimized;
                    await Media.Pause();
                }
                else
                {
                    (App.Current.MainWindow as MainWindow).SetForegroundWindow();
                }
                return;
            }

            if(TransparentOnMouseEnter)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                {
                    Messenger.Send<SetTransparentMessage>();
                }
                else
                {
                    (App.Current.MainWindow as MainWindow).SetForegroundWindow();
                }
                return;
            }
        }));

        private RelayCommand _veryShortBackwardCommand;
        public RelayCommand VeryShortBackwardCommand => _veryShortBackwardCommand ?? (_veryShortBackwardCommand = new RelayCommand(() =>
        {
            jumpSeconds(-3);
        }));

        private RelayCommand _veryShortForwardCommand;
        public RelayCommand VeryShortForwardCommand => _veryShortForwardCommand ?? (_veryShortForwardCommand = new RelayCommand(() =>
        {
            jumpSeconds(3);
        }));

        private RelayCommand<DragEventArgs> _dropObjectsCommand;
        public RelayCommand<DragEventArgs> DropObjectsCommand => _dropObjectsCommand ?? (_dropObjectsCommand = new RelayCommand<DragEventArgs>(async (e) =>
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Playlist.Medias.AddRange(files.Select(x => new FileMedia
                {
                    Path = x
                }));
                CurrentPlayIndex = Playlist.Medias.Count - files.Length;
                await startMedia();

                PlayPauseCommand.NotifyCanExecuteChanged();
                return;
            }
        }));

        private RelayCommand _toggleTransparentOnMouseEnter;
        public RelayCommand ToggleTransparentOnMouseEnter => _toggleTransparentOnMouseEnter ?? (_toggleTransparentOnMouseEnter = new RelayCommand(() =>
        {
            TransparentOnMouseEnter = !TransparentOnMouseEnter;
        }));

        private RelayCommand _volumeUpCommand;
        public RelayCommand VolumeUpCommand => _volumeUpCommand ?? (_volumeUpCommand = new RelayCommand(() =>
        {
            Volume = Math.Min(Volume + 0.05, 1.0);

            showVolumeNotification();
        }));
        private RelayCommand _volumeDownCommand;
        public RelayCommand VolumeDownCommand => _volumeDownCommand ?? (_volumeDownCommand = new RelayCommand(() =>
        {
            Volume = Math.Max(Volume - 0.05, 0);

            showVolumeNotification();
        }));

        private RelayCommand _toggleVideoCropCommand;
        public RelayCommand ToggleVideoCropCommand => _toggleVideoCropCommand ?? (_toggleVideoCropCommand = new RelayCommand(() =>
        {
            if (VideoCrop == VideoCropType.C1x1)
            {
                VideoCrop = VideoCropType.Default;
            }
            else
            {
                VideoCrop++;
            }

            showCropNotification();
        }));

        private RelayCommand _helpCommand;
        public RelayCommand HelpCommand => _helpCommand ?? (_helpCommand = new RelayCommand(() =>
        {
            Process.Start(Constants.HelpUrl);
        }));

        private RelayCommand _checkForUpdateCommand;
        public RelayCommand CheckForUpdateCommand => _checkForUpdateCommand ?? (_checkForUpdateCommand = new RelayCommand(() =>
        {
            AutoUpdater.Start(Constants.AutoUpdaterUrl);
            Settings.Default.LastUpdateCheckTime = DateTime.Now;
        }));

        private RelayCommand _aboutCommand;
        public RelayCommand AboutCommand => _aboutCommand ?? (_aboutCommand = new RelayCommand(() =>
        {
            new AboutWindow().ShowDialog();
        }));

        private RelayCommand _toggleMinimalInterface;
        public RelayCommand ToggleMinimalInterface => _toggleMinimalInterface ?? (_toggleMinimalInterface = new RelayCommand(() =>
        {
            IsMinimalInterface = !IsMinimalInterface;
        }));

        private async Task startMedia()
        {
            await Media.Open(Playlist.Medias[CurrentPlayIndex].Uri);

            TotalTime = Media.NaturalDuration ?? TimeSpan.Zero;

            await Media.Play();
            updateVideoFilter();
        }

        private async Task playNextMedia()
        {
            CurrentPlayIndex++;
            await startMedia();
        }

        private async Task playPreviousMedia()
        {
            CurrentPlayIndex--;
            await startMedia();
        }

        private void jumpSeconds(int seconds)
        {
            Media.Position = Media.Position.Add(TimeSpan.FromSeconds(seconds));
            showTimeNotfication();
        }

        private async void recreateMainWindow()
        {
            bool isPlaying = false;
            TimeSpan oldPosition = TimeSpan.Zero;
            if (Media.MediaState == MediaPlaybackState.Play)
            {
                isPlaying = true;
                oldPosition = Media.Position;
                await Media.Close();
            }
            var oldWindow = App.Current.MainWindow;

            App.Current.MainWindow = new MainWindow();
            App.Current.MainWindow.Show();
            oldWindow.Close();

            if (isPlaying)
            {
                await startMedia();
                Media.Position = oldPosition;
            }
        }

        private void showVolumeNotification()
        {
            Messenger.Send(new NotificationBarMessage
            {
                Message = $"Volume {(int)(Volume * 100 + 0.5)}%"
            });
        }

        private void showTimeNotfication()
        {
            Messenger.Send(new NotificationBarMessage
            {
                Message = $"{SeekTime:hh\\:mm\\:ss} / {TotalTime:hh\\:mm\\:ss}"
            });
        }

        private void updateVideoFilter()
        {
            if(!Media.IsOpen)
            {
                return;
            }
            string videoFilter = getVideoCropFilterString();
            CurrentMediaOptions.VideoFilter = videoFilter;
        }

        private string getVideoCropFilterString()
        {
            if(VideoCrop == VideoCropType.Default)
            {
                return string.Empty;
            }

            float kh = 1, kw = 1;
            switch(VideoCrop)
            {
                case VideoCropType.C16x10:
                    kw = 16;
                    kh = 10;
                    break;
                case VideoCropType.C16x9:
                    kw = 16;
                    kh = 9;
                    break;
                case VideoCropType.C4x3:
                    kw = 4;
                    kh = 3;
                    break;
                case VideoCropType.C1p85x1:
                    kw = 1.85f;
                    kh = 1;
                    break;
                case VideoCropType.C2p21x1:
                    kw = 2.21f;
                    kh = 1;
                    break;
                case VideoCropType.C2p35x1:
                    kw = 2.35f;
                    kh = 1;
                    break;
                case VideoCropType.C2p39x1:
                    kw = 2.39f;
                    kh = 1;
                    break;
                case VideoCropType.C5x3:
                    kw = 5;
                    kh = 3;
                    break;
                case VideoCropType.C5x4:
                    kw = 5;
                    kh = 4;
                    break;
                case VideoCropType.C1x1:
                    kw = 1;
                    kh = 1;
                    break;
            }

            if(Media.NaturalVideoHeight / kh * kw < Media.NaturalVideoWidth)
            {
                return $"crop=in_h*{kw}/{kh}:in_h";
            }
            else
            {
                return $"crop=in_w:in_w*{kh}/{kw}";
            }
        }

        private void showCropNotification()
        {
            string notification = "Default";
            switch (VideoCrop)
            {
                case VideoCropType.C16x10:
                    notification = "16:10";
                    break;
                case VideoCropType.C16x9:
                    notification = "16:9";
                    break;
                case VideoCropType.C4x3:
                    notification = "4:3";
                    break;
                case VideoCropType.C1p85x1:
                    notification = "1.85:1";
                    break;
                case VideoCropType.C2p21x1:
                    notification = "2.21:1";
                    break;
                case VideoCropType.C2p35x1:
                    notification = "2.35:1";
                    break;
                case VideoCropType.C2p39x1:
                    notification = "2.39:1";
                    break;
                case VideoCropType.C5x3:
                    notification = "5:3";
                    break;
                case VideoCropType.C5x4:
                    notification = "5:4";
                    break;
                case VideoCropType.C1x1:
                    notification = "1:1";
                    break;
            }

            Messenger.Send(new NotificationBarMessage
            {
                Message = $"Crop: {notification}"
            });
        }
    }
}
