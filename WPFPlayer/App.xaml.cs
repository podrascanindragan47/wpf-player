using AutoUpdaterDotNET;
using WPFPlayer.Helpers;
using WPFPlayer.Properties;
using WPFPlayer.Views;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using ModernWpf;

namespace WPFPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Unosquare.FFME.Library.FFmpegDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "ffmpeg");

            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            AutoUpdater.InstalledVersion = new Version($"{version.Major}.{version.Minor}.{version.Build}");
            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;

            if((DateTime.Now - Settings.Default.LastUpdateCheckTime).TotalDays >= 7)
            {
                AutoUpdater.Start(Constants.AutoUpdaterUrl);
                Settings.Default.LastUpdateCheckTime = DateTime.Now;
            }
            else
            {
                isUpdatingOnStartup = false;
            }

            if(Settings.Default.IsInitialized)
            {
                Settings.Default.IsInitialized = false;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = Settings.Default.Theme;
            }
        }

        private bool isUpdatingOnStartup = true;
        private void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.IsUpdateAvailable)
            {
                Settings.Default.TopMost = false;
                AutoUpdater.ShowUpdateForm(args);
            }
            else
            {
                if (!isUpdatingOnStartup)
                {
                    new MessageWindow()
                    {
                        Title = "WPF Player",
                        TextContent = "Your product is up to date!",
                    }.ShowDialog();
                }
                isUpdatingOnStartup = false;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Theme = ThemeManager.Current.ActualApplicationTheme;
            Settings.Default.Save();
        }
    }
}
