using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPFPlayer.Helpers;
using WPFPlayer.Messages;
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
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using AutoUpdaterDotNET;
using System.Diagnostics;
using ModernWpf;
using System.Collections.Generic;

namespace WPFPlayer.ViewModels
{
    public class MainViewModel : ObservableRecipient,
        IRecipient<PlayItemMessage>
    {
        public MainViewModel()
        {
            IsActive = true;
        }

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
                List<string> files = Directory.GetFiles(directory)
                    .Where(x => Constants.MediaFileExtensions.Any(y => x.EndsWith("." + y, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                PlaylistViewModel.Instance.AddFiles(new List<string> { args[1] }, true);
                PlaylistViewModel.Instance.AddFiles(files);

                await startMedia();
            }

            _timerHideControls.Interval = 2000;
            _timerHideControls.Tick += timerHideControls_Tick;

            _timerHideCursor.Interval = 2000;
            _timerHideCursor.Tick += timerHideCursor_Tick;

            PlayPauseCommand.NotifyCanExecuteChanged();
        }

        private void timerHideCursor_Tick(object sender, EventArgs e)
        {
            if(Media.IsPlaying)
            {
                Cursor = Cursors.None;
            }
            _timerHideCursor.Stop();
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

        private Timer _timerHideCursor = new Timer();

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

        public double Left
        {
            get => Settings.Default.Left;
            set
            {
                if(Settings.Default.Left == value)
                {
                    return;
                }

                Settings.Default.Left = value;

                OnPropertyChanged(nameof(Left));
            }
        }
        public double Top
        {
            get => Settings.Default.Top;
            set
            {
                if(Settings.Default.Top == value)
                {
                    return;
                }

                Settings.Default.Top = value;

                OnPropertyChanged(nameof(Top));
            }
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
        public double Right => Left + Width;
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

        private VideoCropType _videoCrop = VideoCropType.Default;
        public VideoCropType VideoCrop
        {
            get => _videoCrop;
            set
            {
                if(!SetProperty(ref _videoCrop, value))
                {
                    return;
                }

                showCropNotification();
                updateVideoFilter();
            }
        }

        public IconElement PlayPauseIcon => IsPlaying ? new SymbolIcon((Symbol)0xF8AE) : new SymbolIcon((Symbol)0xF5B0);
        public string PlayPauseLabel => IsPlaying ? "Pause" : "Play";

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

        public bool IsDarkMode
        {
            get => ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark;
            set
            {
                if (value == (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark))
                {
                    return;
                }

                if (value)
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                }
                else
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                }

                OnPropertyChanged(nameof(IsDarkMode));
            }
        }

        private Cursor _cursor;
        public Cursor Cursor
        {
            get => _cursor;
            set => SetProperty(ref _cursor, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isRandom = false;
        public bool IsRandom
        {
            get => _isRandom;
            set
            {
                if(!SetProperty(ref _isRandom, value))
                {
                    return;
                }

                NextMediaCommand.NotifyCanExecuteChanged();
                PreviousMediaCommand.NotifyCanExecuteChanged();

                Messenger.Send(new NotificationBarMessage
                {
                    Message = "Random: " + (value ? "On" : "Off")
                });
            }
        }

        private RepeatType _repeatType;
        public RepeatType RepeatType
        {
            get => _repeatType;
            set
            {
                if(!SetProperty(ref _repeatType, value))
                {
                    return;
                }

                NextMediaCommand.NotifyCanExecuteChanged();
                PreviousMediaCommand.NotifyCanExecuteChanged();

                Messenger.Send(new NotificationBarMessage
                {
                    Message = $"Loop: {value}"
                });
            }
        }

        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if(!SetProperty(ref _isPlaying, value))
                {
                    return;
                }

                OnPropertyChanged(nameof(PlayPauseIcon));
                OnPropertyChanged(nameof(PlayPauseLabel));
            } 
        }

        public bool ShowPlaylistWindow
        {
            get => PlaylistWindow.Instance != null;
            set
            {
                if(ShowPlaylistWindow == value)
                {
                    return;
                }
                if(value)
                {
                    PlaylistWindow.Instance = new PlaylistWindow();
                    PlaylistWindow.Instance.Show();
                    PlaylistViewModel.Instance.IsDocked = true;
                    PlaylistViewModel.Instance.DockTopDelta = 0;
                    PlaylistViewModel.Instance.DockBottomDelta = 0;
                    UpdatePlaylistWindowPosition();
                }
                else
                {
                    PlaylistWindow.Instance.Close();
                }
            }
        }
        public void UpdateShowPlaylistWindow()
        {
            OnPropertyChanged(nameof(ShowPlaylistWindow));
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
                    PlaylistViewModel.Instance.AddFiles(dlg.FileNames.ToList(), true);
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
                PlaylistViewModel.Instance.AddFiles(new List<string> { dlg.URL }, true);
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
                    List<string> files = Directory.GetFiles(dlg.SelectedPath)
                        .Where(x => Constants.MediaFileExtensions.Any(y => x.EndsWith("." + y, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    PlaylistViewModel.Instance.AddFiles(files, true);
                    await startMedia();

                    PlayPauseCommand.NotifyCanExecuteChanged();
                }
            }
        }));

        private RelayCommand _nextMediaCommand;
        public RelayCommand NextMediaCommand => _nextMediaCommand ?? (_nextMediaCommand = new RelayCommand(
            async () =>
            {
                IsPlaying = true;
                PlaylistViewModel.Instance.CurrentItem = PlaylistViewModel.Instance.GetNextMedia();
                await startMedia();
            },
            () => PlaylistViewModel.Instance.GetNextMedia() != null ));

        private RelayCommand _previousMediaCommand;
        public RelayCommand PreviousMediaCommand => _previousMediaCommand ?? (_previousMediaCommand = new RelayCommand(
            async () =>
            {
                IsPlaying = true;
                PlaylistViewModel.Instance.CurrentItem = PlaylistViewModel.Instance.GetPrevMedia();
                await startMedia();
            },
            () => PlaylistViewModel.Instance.GetPrevMedia() != null ));

        private RelayCommand _playPauseCommand;
        public RelayCommand PlayPauseCommand => _playPauseCommand ?? (_playPauseCommand = new RelayCommand(
            async () =>
            {
                IsPlaying = !IsPlaying;
                if(IsPlaying)
                {
                    switch (Media.MediaState)
                    {
                        case MediaPlaybackState.Close:
                            await startMedia();
                            break;
                        default:
                            await Media.Play();
                            break;
                    }
                }
                else
                {
                    await Media.Pause();
                }    
            },
            () => PlaylistViewModel.Instance.Items.Any() ));

        private RelayCommand<MediaStateChangedEventArgs> _mediaStateChangedCommand;
        public RelayCommand<MediaStateChangedEventArgs> MediaStateChangedCommand => _mediaStateChangedCommand ?? (_mediaStateChangedCommand = new RelayCommand<MediaStateChangedEventArgs>((e) =>
        {
            if (e.MediaState == MediaPlaybackState.Stop && e.OldMediaState == MediaPlaybackState.Play)
            {
                if(IsPlaying)
                {
                    if (Media.IsNetworkStream && Media.IsSeekable && (TotalTime - Media.Position).TotalSeconds >= 3)
                    {
                        var currentPosition = Media.Position;
                        App.Current.Dispatcher.Invoke(async () =>
                        {
                            await startMedia(currentPosition);
                        });
                        return;
                    }

                    if(Media.Position.TotalSeconds < 0.5)
                    {
                        var nextItem = PlaylistViewModel.Instance.GetNextMedia();
                        if (nextItem != null)
                        {
                            PlaylistViewModel.Instance.CurrentItem = nextItem;
                            App.Current.Dispatcher.Invoke(async () =>
                            {
                                await startMedia();
                            });
                            return;
                        }
                        else
                        {
                            IsPlaying = false;
                        }
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(async () =>
                        {
                            await Media.Play();
                        });
                    }
                }
            }

            StopCommand.NotifyCanExecuteChanged();
        }));

        private RelayCommand _stopCommand;
        public RelayCommand StopCommand => _stopCommand ?? (_stopCommand = new RelayCommand(
            async () =>
            {
                IsPlaying = false;
                await Media.Close();

                PlaylistViewModel.Instance.CurrentItem = PlaylistViewModel.Instance.Items[0];
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
            resetCursor();

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
            resetCursor();

            if (Media.MediaState == MediaPlaybackState.Play && MinimizeOnMouseEnter && (DateTime.Now - _minimizeOnMouseEnterTime).TotalSeconds > 2)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                {
                    WindowState = WindowState.Minimized;
                    await Media.Pause();
                }
                else
                {
                    App.Current.MainWindow.Activate();
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
                    App.Current.MainWindow.Activate();
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
                PlaylistViewModel.Instance.AddFiles(files.ToList(), true);
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
            int i = VideoCropType.All.IndexOf(VideoCrop) + 1;
            if(i >= VideoCropType.All.Count)
            {
                i = 0;
            }

            VideoCrop = VideoCropType.All[i];

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

        private RelayCommand _toggleRepeatCommand;
        public RelayCommand ToggleRepeatCommand => _toggleRepeatCommand ?? (_toggleRepeatCommand = new RelayCommand(() =>
        {
            if(RepeatType == RepeatType.One)
            {
                RepeatType = RepeatType.Off;
            }
            else
            {
                RepeatType++;
            }
        }));

        private RelayCommand _toggleRandomCommand;
        public RelayCommand ToggleRandomCommand => _toggleRandomCommand ?? (_toggleRandomCommand = new RelayCommand(() =>
        {
            IsRandom = !IsRandom;
        }));

        private RelayCommand<EventArgs> _windowLocationChangedCommand;
        public RelayCommand<EventArgs> WindowLocationChangedCommand => _windowLocationChangedCommand ?? (_windowLocationChangedCommand = new RelayCommand<EventArgs>((e) =>
        {
            UpdatePlaylistWindowPosition();
        }));
        private RelayCommand<SizeChangedEventArgs> _windowSizeChangedCommand;
        public RelayCommand<SizeChangedEventArgs> WindowSizeChangedCommand => _windowSizeChangedCommand ?? (_windowSizeChangedCommand = new RelayCommand<SizeChangedEventArgs>((e) =>
        {
            UpdatePlaylistWindowPosition();
        }));

        private RelayCommand _togglePlaylistCommand;
        public RelayCommand TogglePlaylistCommand => _togglePlaylistCommand ?? (_togglePlaylistCommand = new RelayCommand(() =>
        {
            ShowPlaylistWindow = !ShowPlaylistWindow;
        }));

        public void UpdatePlaylistWindowPosition()
        {
            if (PlaylistViewModel.Instance != null && PlaylistViewModel.Instance.IsDocked && WindowState == WindowState.Normal)
            {
                PlaylistViewModel.Instance.IsMovingFromHost = true;

                PlaylistViewModel.Instance.Left = Right;
                PlaylistViewModel.Instance.Top = Top + PlaylistViewModel.Instance.DockTopDelta;
                PlaylistViewModel.Instance.Height = Top + Height - PlaylistViewModel.Instance.Top + PlaylistViewModel.Instance.DockBottomDelta;
                PlaylistViewModel.Instance.IsMovingFromHost = false;
            }
        }

        private async Task startMedia(TimeSpan? startPostion = null)
        {
            IsLoading = true;
            IsPlaying = true;

            await Media.Open(PlaylistViewModel.Instance.CurrentItem.Data.MediaSource);
            Media.Position = startPostion ?? TimeSpan.Zero;
            TotalTime = Media.NaturalDuration ?? TimeSpan.Zero;
            updateVideoFilter();
            await Media.Play();

            IsLoading = false;

            _timerHideCursor.Start();
        }

        private void jumpSeconds(int seconds)
        {
            Media.Position = Media.Position.Add(TimeSpan.FromSeconds(seconds));
            showTimeNotfication();
        }

        private async void recreateMainWindow()
        {
            TimeSpan oldPosition = TimeSpan.Zero;
            if (IsPlaying)
            {
                oldPosition = Media.Position;
                await Media.Close();
            }
            var oldWindow = App.Current.MainWindow;

            App.Current.MainWindow = new MainWindow();
            App.Current.MainWindow.Show();
            oldWindow.Close();

            if (IsPlaying)
            {
                await startMedia(oldPosition);
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

        public void updateVideoFilter()
        {
            if(!Media.IsOpen)
            {
                return;
            }
            string videoFilter = VideoCrop.GetVideoFilter(Media.NaturalVideoWidth, Media.NaturalVideoHeight);
            CurrentMediaOptions.VideoFilter = videoFilter;
        }

        private void showCropNotification()
        {
            Messenger.Send(new NotificationBarMessage
            {
                Message = $"Crop: {VideoCrop}"
            });
        }

        private void resetCursor()
        {
            if(Cursor == Cursors.None)
            {
                Cursor = Cursors.Arrow;
            }

            _timerHideCursor.Stop();
            if(Media.IsPlaying)
            {
                _timerHideCursor.Start();
            }
        }

        public async void Receive(PlayItemMessage message)
        {
            IsPlaying = true;
            PlaylistViewModel.Instance.CurrentItem = message.Item;
            await startMedia();
        }
    }
}
