using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace MiniLauncher.Services
{
    public static class AutoUpdaterService
    {
        private const string GithubRepo = "Lyra-Everlyn/MiniLauncher";
        private static readonly string CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

        public static async Task CheckAndApplyUpdateAsync()
        {
            CleanUpOldVersion();

            string? downloadUrl = null;
            string cleanLatestVer = "";

            try
            {
                using var checkClient = new HttpClient();
                checkClient.DefaultRequestHeaders.Add("User-Agent", "MiniLauncher-Updater");
                checkClient.Timeout = TimeSpan.FromSeconds(5);

                string apiUrl = $"https://api.github.com/repos/{GithubRepo}/releases/latest";
                string response = await checkClient.GetStringAsync(apiUrl);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                string latestTag = root.GetProperty("tag_name").GetString() ?? "";
                cleanLatestVer = latestTag.TrimStart('v', 'V');

                if (!IsNewerVersion(cleanLatestVer, CurrentVersion))
                {
                    return;
                }

                if (root.TryGetProperty("assets", out var assets) && assets.GetArrayLength() > 0)
                {
                    foreach (var asset in assets.EnumerateArray())
                    {
                        string name = asset.GetProperty("name").GetString() ?? "";
                        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            downloadUrl = asset.GetProperty("browser_download_url").GetString();
                            break;
                        }
                    }
                }
            }
            catch
            {
                return;
            }

            if (string.IsNullOrEmpty(downloadUrl)) return;
            var result = MessageBox.Show(
                $"Update is available v{cleanLatestVer} (Current: v{CurrentVersion}).\nDo you want to update now?",
                "Update Mini Launcher",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var downloadClient = new HttpClient();
                    downloadClient.DefaultRequestHeaders.Add("User-Agent", "MiniLauncher-Updater");
                    downloadClient.Timeout = TimeSpan.FromMinutes(10);

                    await DownloadAndReplaceExecutableAsync(downloadClient, downloadUrl);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static async Task DownloadAndReplaceExecutableAsync(HttpClient client, string downloadUrl)
        {
            string? currentExePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(currentExePath)) return;

            string newExePath = currentExePath + ".new";
            string oldExePath = currentExePath + ".old";

            using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                await using var contentStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(newExePath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, true);

                await contentStream.CopyToAsync(fileStream);
            }

            MessageBox.Show("Download completed! The application will restart.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            if (File.Exists(oldExePath)) File.Delete(oldExePath);
            File.Move(currentExePath, oldExePath);
            File.Move(newExePath, currentExePath);

            Process.Start(new ProcessStartInfo
            {
                FileName = currentExePath,
                WorkingDirectory = Path.GetDirectoryName(currentExePath) ?? "",
                UseShellExecute = true
            });

            Environment.Exit(0);
        }

        private static void CleanUpOldVersion()
        {
            try
            {
                string? currentExePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(currentExePath))
                {
                    string oldExePath = currentExePath + ".old";
                    if (File.Exists(oldExePath)) File.Delete(oldExePath);
                }
            }
            catch { }
        }

        private static bool IsNewerVersion(string latest, string current)
        {
            if (Version.TryParse(latest, out var vLatest) && Version.TryParse(current, out var vCurrent))
            {
                return vLatest > vCurrent;
            }
            return false;
        }
    }
}
