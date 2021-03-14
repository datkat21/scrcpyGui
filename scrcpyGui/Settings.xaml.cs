using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using AdonisUI.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace scrcpyGui
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : AdonisWindow
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void GeneralTab_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SettingsFrame.Source = new Uri(@"/Pages/SettingsGeneral.xaml", UriKind.Relative);
        }

        private void StreamingTab_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SettingsFrame.Source = new Uri(@"/Pages/SettingsStreaming.xaml", UriKind.Relative);
        }

        public void SaveSettings()
        {
            stn.Default.Save();
        }
    }
}
