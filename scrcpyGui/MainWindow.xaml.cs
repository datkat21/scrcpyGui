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

#pragma warning disable IDE1006

namespace scrcpyGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static Task WaitForExitAsync(this Process process,
        CancellationToken cancellationToken = default(CancellationToken))
        {
            if (process.HasExited) return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            if (cancellationToken != default(CancellationToken))
                cancellationToken.Register(() => tcs.SetCanceled());

            return process.HasExited ? Task.CompletedTask : tcs.Task;
        }
    }



    public partial class MainWindow : AdonisWindow
    {
        public string currentPath = null;
        public string streamSettings = null;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            AdonisUI.Controls.MessageBoxResult result = AdonisUI.Controls.MessageBox.Show("This program OPENS FILES and can cause DAMAGE if used incorrectly.\nThis was created for educational purposes ONLY.\n\nPress \"YES\" if you are SURE you want to launch this program. ", "My App", AdonisUI.Controls.MessageBoxButton.YesNo, AdonisUI.Controls.MessageBoxImage.Exclamation, AdonisUI.Controls.MessageBoxResult.No);
            switch (result)
            {
                case AdonisUI.Controls.MessageBoxResult.No:
                    Application.Current.Shutdown();
                    break;
            }
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
            Process[] ps = Process.GetProcessesByName("scrcpy.exe");

            foreach (Process p in ps)
                p.Kill();
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            bool success = false;
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                writeOutput("Selected path " + dialog.SelectedPath + ".");
                try
                {
                    if (File.Exists(dialog.SelectedPath + "/adb.exe"))
                    {
                        scpyTab.Visibility = Visibility.Visible;
                        writeOutput("Got " + dialog.SelectedPath + "/adb.exe, attempting to launch it");
                        currentPath = dialog.SelectedPath;
                        Process adbProcess = new();
                        adbProcess.StartInfo.FileName = dialog.SelectedPath + "/adb.exe";
                        adbProcess.StartInfo.UseShellExecute = false;
                        adbProcess.StartInfo.RedirectStandardOutput = true;
                        adbProcess.Start();
                        string outputStr = adbProcess.StandardOutput.ReadToEnd().Truncate(20);
                        adbProcess.WaitForExit();
                        if (outputStr == "Android Debug Bridge")
                        {
                            writeOutput("\"" + outputStr + "\" seems correct... let's try listing the devices");
                            adbProcess.StartInfo.FileName = dialog.SelectedPath + "/adb.exe";
                            adbProcess.StartInfo.UseShellExecute = false;
                            adbProcess.StartInfo.Arguments = "devices";
                            adbProcess.Start();
                            string newOutput = adbProcess.StandardOutput.ReadToEnd();
                            adbProcess.WaitForExit();
                            var result = newOutput.Split(new string[] { "\n" }, StringSplitOptions.None);
                            // outputBox.Text += newOutput + "\n";
                            if (result[0] == "List of devices attached")
                            {
                                result.Skip(1).ToArray();
                                result.Take(result.Count() - 2).ToArray();
                            }
                            result.Skip(1).ToArray();
                            result.Take(result.Count() - 2).ToArray();
                            adbList.Items.Clear();
                            for (int i = 1; i < result.Length; i++)
                            {
                                if (result[i].Contains("unauthorized") || result[i].Contains("device"))
                                {
                                    writeOutput(i.ToString() + ": " + result[i]);

                                    string replacedResult = result[i].Replace("\t", " ").Replace(" ", "").Replace("unauthorized", "").Replace("device", "");
                                    replacedResult = Regex.Replace(replacedResult, @"\r\n?|\n", "");
                                    // * Uncommenting this because it's important now
                                    if (result[i].Contains("device"))
                                    {
                                        adbProcess.StartInfo.FileName = dialog.SelectedPath + "/adb.exe";
                                        adbProcess.StartInfo.UseShellExecute = false;
                                        adbProcess.StartInfo.Arguments = "-s " + replacedResult + " shell getprop ro.build.product";
                                        adbProcess.Start();
                                        string adbOutput = adbProcess.StandardOutput.ReadToEnd();
                                        adbProcess.WaitForExit();
                                        string convertedOutput = Regex.Replace(adbOutput, @"\r\n?|\n", "");
                                        if (!String.IsNullOrEmpty(convertedOutput))
                                        {
                                            writeOutput("Getting device name.. (" + convertedOutput + ")");
                                        }
                                        writeOutput(adbProcess.StartInfo.Arguments);
                                        // Define items
                                        Image img = new();
                                        TextBlock txt = new();
                                        WrapPanel panel = new();
                                        ListBoxItem itm = new();
                                        // Give items properties
                                        img.Source = new BitmapImage(new Uri("pack://application:,,,/assets/mobileLight.png"));
                                        Double width = 25; img.Width = width;
                                        txt.Text = convertedOutput;
                                        txt.Foreground = new SolidColorBrush(Colors.White);
                                        txt.Margin = new Thickness(5, 0, 0, 0);
                                        txt.VerticalAlignment = VerticalAlignment.Center;
                                        panel.Children.Add(img);
                                        panel.Children.Add(txt);
                                        itm.ToolTip = "This is an authorized device. You can stream it.";

                                        itm.Focusable = true;
                                        itm.Tag = replacedResult;
                                        itm.Content = panel;
                                        itm.Margin = new Thickness(0, 3, 0, 3);
                                        adbList.Items.Add(itm);
                                        success = true;
                                    }
                                    else if (result[i].Contains("unauthorized"))
                                    {
                                        // Define items
                                        Image img = new();
                                        TextBlock txt = new();
                                        WrapPanel panel = new();
                                        ListBoxItem itm = new();
                                        // Give items properties
                                        img.Source = new BitmapImage(new Uri("pack://application:,,,/assets/mobileNoGray.png"));
                                        double width = 25; img.Width = width;
                                        txt.Text = replacedResult + " (unauthorized, hover for more information)";
                                        txt.Foreground = new SolidColorBrush(Colors.Gray);
                                        txt.Margin = new Thickness(5, 0, 0, 0);
                                        txt.VerticalAlignment = VerticalAlignment.Center;
                                        panel.Children.Add(img);
                                        panel.Children.Add(txt);
                                        itm.ToolTip = "Unable to stream from this device: Device is unauthorized. Please check the device and allow this computer.";
                                        itm.Focusable = false;
                                        itm.Content = panel;
                                        itm.Margin = new Thickness(0, 3, 0, 3);
                                        itm.Tag = replacedResult;
                                        adbList.Items.Add(itm);
                                        success = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            writeOutput("Failed to read adb.exe, is it really adb?\n" + outputStr, false);
                        }

                    }
                    else
                    {
                        writeOutput("adb.exe was not found in " + dialog.SelectedPath + ".");
                    }
                }
                catch (Exception er)
                {
                    writeOutput("An error occured, please look below for details. (Path: " + dialog.SelectedPath + ")");
                    writeOutput(er.Message, false);
                }
            }

            if (success)
            {
                OpenFolder.Focusable = false;
                OpenFolder.ToolTip = "You have already selected a folder. To change it, go to Settings.";
            }
        }

        private void writeOutput(string text, bool newLine = true)
        {
            if (newLine)
            {
                outputBox.Text += text + "\n";
            }
            else
            {
                outputBox.Text += text;
            }
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string path)
        {
            Console.WriteLine("Processed file '{0}'.", path);
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings win = new();
            win.Owner = this;
            win.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            About about = new();
            about.ShowDialog();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            AdonisUI.Controls.MessageBoxResult result = AdonisUI.Controls.MessageBox.Show("Are you sure you want to clear the output?", "WPFcpy", AdonisUI.Controls.MessageBoxButton.YesNo, AdonisUI.Controls.MessageBoxImage.Question, AdonisUI.Controls.MessageBoxResult.No);
            switch (result)
            {
                case AdonisUI.Controls.MessageBoxResult.Yes:
                    outputBox.Clear();
                    break;
            }
        }

        private async void StartStream_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(currentPath))
                {
                    object serial = ((ListBoxItem)adbList.SelectedItem).Tag;
                    Process scrcpy = new();
                    scrcpy.StartInfo.FileName = currentPath + "/scrcpy.exe";
                    scrcpy.StartInfo.RedirectStandardOutput = true;
                    scrcpy.StartInfo.UseShellExecute = false;
                    scrcpy.EnableRaisingEvents = true;
                    scrcpy.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    scrcpy.StartInfo.CreateNoWindow = true;
                    scrcpy.StartInfo.Arguments = "-s " + serial + " " + streamSettings; // variable 'streamSettings' implements the custom settings defined in Settings.xaml
                    writeOutput("Attempting to stream " + serial);
                    scrcpy.Start();
                    await scrcpy.WaitForExitAsync();
                    string scrcpyOutput = scrcpy.StandardOutput.ReadToEnd();
                    logBox.Text += scrcpyOutput;
                }
                else
                {
                    writeOutput("Failed to get path... try selecting your folder again");
                }

            }
            catch (Exception err)
            {
                writeOutput("An error occured while streaming:\n" + err.Message);
            }
        }

    }
}
