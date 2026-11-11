using MiniLauncher.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MiniWorldLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow() { InitializeComponent(); this.Loaded += MainWindow_Loaded; }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e) => await AutoUpdaterService.CheckAndApplyUpdateAsync();


        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();



        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            string apiId = "410";
            if (ApiIdComboBox.SelectedItem is ComboBoxItem selectedItem)
                apiId = selectedItem.Content.ToString() ?? apiId;

            string deviceId = "WIN" + GenerateHex(16);
            string parentHwnd = "0";

            LaunchGame(deviceId, parentHwnd, apiId);
        }

        private string GenerateHex(int bytesCount)
        {
            byte[] randomBytes = new byte[bytesCount];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToHexString(randomBytes).ToLower();
        }

        private void LaunchGame(string deviceId, string parentHwnd, string apiId)
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string exeDir = Path.Combine(appDataPath, "miniworldOverseasgame");
                string exePath = Path.Combine(exeDir, "MiniGameApp.exe");

                if (!File.Exists(exePath))
                {
                    MessageBox.Show("Cannot find miniworldOverseasgame on your computer. Please check your installation.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string sessionId = GenerateHex(16);
                string rawString = $"account=MNWPC&deviceId={deviceId}&sessionId={sessionId}&userNames=&openstring=&apiid={apiId}";
                byte[] rawBytes = Encoding.UTF8.GetBytes(rawString);
                string encodedStr = Convert.ToBase64String(rawBytes);
                string finalBaseInfo = encodedStr.TrimEnd('=') + "*";
                string arguments = $"parenthwnd={parentHwnd}&baseinfo={finalBaseInfo}";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    WorkingDirectory = exeDir,
                    UseShellExecute = false
                };

                Process.Start(startInfo);

                this.WindowState = WindowState.Minimized;
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while launching the game:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}