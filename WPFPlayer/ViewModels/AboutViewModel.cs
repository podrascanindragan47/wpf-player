using AutoUpdaterDotNET;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;
using System;

namespace WPFPlayer.ViewModels
{
    public class AboutViewModel : ObservableRecipient
    {
        public string AppTitle
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return $"WPF Player v{version.Major}.{version.Minor}.{version.Build}";
            }
        }
    }
}
