using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Emgu.CV.DepthAI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPFPlayer.Helpers;
using WPFPlayer.Models;
using WPFPlayer.Properties;

namespace WPFPlayer.ViewModels
{
    public class PlaylistViewModel : ObservableRecipient
    {
        private static PlaylistViewModel _instance;
        public static PlaylistViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlaylistViewModel();
                }
                return _instance;
            }
        }

        public ObservableCollection<PlaylistItemViewModel> Items { get; private set; } = new ObservableCollection<PlaylistItemViewModel>();

        private PlaylistItemViewModel _currentItem;
        public PlaylistItemViewModel CurrentItem
        {
            get => _currentItem;
            set
            {
                if(!SetProperty(ref _currentItem, value))
                {
                    return;
                }

                MainViewModel.Instance.NextMediaCommand.NotifyCanExecuteChanged();
                MainViewModel.Instance.PreviousMediaCommand.NotifyCanExecuteChanged();
                foreach(var item in Items){
                    item.UpdateTextFontWeight();
                }
            }
        }

        public bool IsMovingFromHost = false;

        public double Left
        {
            get => Settings.Default.PlaylistWindowLeft;
            set
            {
                if(Settings.Default.PlaylistWindowLeft == value)
                {
                    return;
                }

                Settings.Default.PlaylistWindowLeft = value;

                OnPropertyChanged(nameof(Left));
            }
        }
        public double Top
        {
            get => Settings.Default.PlaylistWindowTop;
            set
            {
                if(Settings.Default.PlaylistWindowTop == value)
                {
                    return;
                }

                Settings.Default.PlaylistWindowTop = value;

                OnPropertyChanged(nameof(Top));
            }
        }
        public double Width
        {
            get => Settings.Default.PlaylistWindowWidth;
            set
            {
                if(Settings.Default.PlaylistWindowWidth == value)
                {
                    return;
                }

                Settings.Default.PlaylistWindowWidth = value;

                OnPropertyChanged(nameof(Width));
            }
        }
        public double Height
        {
            get => Settings.Default.PlaylistWindowHeight;
            set
            {
                if(Settings.Default.PlaylistWindowHeight == value)
                {
                    return;
                }

                Settings.Default.PlaylistWindowHeight = value;

                OnPropertyChanged(nameof(Height));
            }
        }
        public bool IsDocked
        {
            get => Settings.Default.PlaylistWindowDocked;
            set
            {
                if(Settings.Default.PlaylistWindowDocked == value)
                {
                    return;
                }

                Settings.Default.PlaylistWindowDocked = value;

                OnPropertyChanged(nameof(IsDocked));
            }
        }
        public double DockTopDelta
        {
            get => Settings.Default.PlaylistWindowDockTopDelta;
            set
            {
                if (Settings.Default.PlaylistWindowDockTopDelta == value)
                {
                    return;
                }

                Settings.Default.PlaylistWindowDockTopDelta = value;

                OnPropertyChanged(nameof(DockTopDelta));
            }
        }
        public double DockBottomDelta
        {
            get => Settings.Default.PlaylistWindowDockBottomDelta;
            set
            {
                if (Settings.Default.PlaylistWindowDockBottomDelta == value)
                {
                    return;
                }

                Settings.Default.PlaylistWindowDockBottomDelta = value;

                OnPropertyChanged(nameof(DockBottomDelta));
            }
        }

        public void AddFiles(List<string> urls, bool selectFirstItem = false)
        {
            foreach (var url in urls)
            {
                Uri uri = new Uri(url);

                PlaylistItemViewModel item = Items.FirstOrDefault(x => x.Data.MediaSource == uri);
                if (item == null)
                {
                    item = new PlaylistItemViewModel(new PlaylistItem
                    {
                        MediaSource = new System.Uri(url)
                    });
                    Items.Add(item);
                }

                if (selectFirstItem || Items.Count == 0)
                {
                    selectFirstItem = false;
                    CurrentItem = item;
                }
            }

            MainViewModel.Instance.NextMediaCommand.NotifyCanExecuteChanged();
            MainViewModel.Instance.PreviousMediaCommand.NotifyCanExecuteChanged();

            Task.Run(() =>
            {
                ExtractItemInformations();
            });
        }

        public PlaylistItemViewModel GetNextMedia()
        {
            if (Items.Count == 0)
            {
                return null;
            }

            if (MainViewModel.Instance.IsRandom)
            {
                return getNextRandomItem();
            }

            int i = Items.IndexOf(CurrentItem) + 1;
            if(i == Items.Count)
            {
                if(MainViewModel.Instance.RepeatType == RepeatType.Off)
                {
                    return null;
                }
                i = 0;
            }

            return Items[i];
        }

        public PlaylistItemViewModel GetPrevMedia()
        {
            if (Items.Count == 0)
            {
                return null;
            }

            if (MainViewModel.Instance.IsRandom)
            {
                return getNextRandomItem();
            }

            int i = Items.IndexOf(CurrentItem) - 1;
            if (i < 0)
            {
                if (MainViewModel.Instance.RepeatType == RepeatType.Off)
                {
                    return null;
                }
                i = Items.Count - 1;
            }

            return Items[i];
        }

        public PlaylistItemViewModel getNextRandomItem()
        {
            if(MainViewModel.Instance.RepeatType == RepeatType.Off)
            {
                List<PlaylistItemViewModel> remainItems = Items.Where(x => !x.IsViewed).ToList();
                if(remainItems.Count == 0)
                {
                    return null;
                }
                return remainItems[Constants.RANDOM.Next(remainItems.Count)];
            }

            return Items[Constants.RANDOM.Next(Items.Count)];
        }

        public void ExtractItemInformations()
        {
            foreach (var item in Items.Where(x => !x.IsExtractedInfo))
            {
                item.ExtractInformation();
            }
        }

        private RelayCommand _removeItemsCommand;
        public RelayCommand RemoveItemsCommand => _removeItemsCommand ?? (_removeItemsCommand = new RelayCommand(
            () =>
            {
                List<PlaylistItemViewModel> deleteItems = Items.Where(x => x.IsSelected).ToList();
                foreach (var item in deleteItems)
                {
                    Items.Remove(item);
                }

                if (deleteItems.Contains(CurrentItem))
                {
                    MainViewModel.Instance.StopCommand.Execute(null);
                }
            },
            () => Items.Any(x => x.IsSelected)));

        private RelayCommand _moveUpCommand;
        public RelayCommand MoveUpCommand => _moveUpCommand ?? (_moveUpCommand = new RelayCommand(
            () =>
            {
                List<PlaylistItemViewModel> moveItems = Items.Where(x => x.IsSelected).OrderBy(x => Items.IndexOf(x)).ToList();

                for (int i = 0; i < Items.Count && Items[i].IsSelected; i++)
                {
                    moveItems.Remove(Items[i]);
                }

                foreach (var item in moveItems)
                {
                    int i = Items.IndexOf(item);
                    Items.Remove(item);
                    Items.Insert(i - 1, item);
                }
            },
            () => Items.Any(x => x.IsSelected)));

        private RelayCommand _moveDownCommand;
        public RelayCommand MoveDownCommand => _moveDownCommand ?? (_moveDownCommand = new RelayCommand(
            () =>
            {
                List<PlaylistItemViewModel> moveItems = Items.Where(x => x.IsSelected).OrderByDescending(x => Items.IndexOf(x)).ToList();

                for (int i = Items.Count - 1; i >= 0 && Items[i].IsSelected; i--)
                {
                    moveItems.Remove(Items[i]);
                }

                foreach (var item in moveItems)
                {
                    int i = Items.IndexOf(item);
                    Items.Remove(item);
                    Items.Insert(i + 1, item);
                }
            },
            () => Items.Any(x => x.IsSelected)));

        private RelayCommand<EventArgs> _windowLocationChangedCommand;
        public RelayCommand<EventArgs> WindowLocationChangedCommand => _windowLocationChangedCommand ?? (_windowLocationChangedCommand = new RelayCommand<EventArgs>((e) =>
        {
            if(IsMovingFromHost || MainViewModel.Instance.WindowState != WindowState.Normal)
            {
                return;
            }

            if (IsDocked)
            {
                double delta = Math.Abs(MainViewModel.Instance.Right - Left);
                if(delta > DOCK_DELTA)
                {
                    IsDocked = false;
                    return;
                }
                else
                {
                    updateTopBottomDock();
                }
            }
            else
            {
                double delta = Math.Abs(MainViewModel.Instance.Right - Left);
                if (delta > DOCK_DELTA)
                {
                    return;
                }

                IsDocked = true;
                updateTopBottomDock();
            }
        }));
        private RelayCommand<SizeChangedEventArgs> _windowSizeChangedCommand;
        public RelayCommand<SizeChangedEventArgs> WindowSizeChangedCommand => _windowSizeChangedCommand ?? (_windowSizeChangedCommand = new RelayCommand<SizeChangedEventArgs>((e) =>
        {
            if(IsMovingFromHost || MainViewModel.Instance.WindowState != WindowState.Normal)
            {
                return;
            }

            if(IsDocked && e.HeightChanged)
            {
                updateTopBottomDock();
            }
        }));

        private void updateTopBottomDock()
        {
            double delta = Math.Abs(MainViewModel.Instance.Top - Top);
            if (delta <= DOCK_DELTA)
            {
                DockTopDelta = 0;
            }
            if (DockTopDelta != 0 || delta > DOCK_DELTA)
            {
                DockTopDelta = Top - MainViewModel.Instance.Top;
            }

            delta = Math.Abs(MainViewModel.Instance.Top + MainViewModel.Instance.Height - Top - Height);
            if (delta <= DOCK_DELTA)
            {
                DockBottomDelta = 0;
            }
            if (DockBottomDelta != 0 || delta > DOCK_DELTA)
            {
                DockBottomDelta = Top + Height - MainViewModel.Instance.Top - MainViewModel.Instance.Height;
            }

            MainViewModel.Instance.UpdatePlaylistWindowPosition();
        }

        private static readonly double DOCK_DELTA = 24;
    }
}
