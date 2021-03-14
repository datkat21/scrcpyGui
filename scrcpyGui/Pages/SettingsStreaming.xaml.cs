using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace scrcpyGui.Pages
{
    /// <summary>
    /// Interaction logic for SettingsStreaming.xaml
    /// </summary>
    public partial class SettingsStreaming : Page
    {
        public SettingsStreaming()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void SetBitrate()
        {

        }

        private void BR_Auto_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void BR_1_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void BR_2_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void BR_4_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void BR_6_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void BR_8_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void BR_Custom_Selected(object sender, RoutedEventArgs e)
        {

        }
    }
}
