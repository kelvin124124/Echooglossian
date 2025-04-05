using Dalamud.Interface.ImGuiNotification;
using Echoglossian.Properties;

namespace Echoglossian
{
    public class AssetManager
    {
        public AssetManager() => CheckAndDownloadPluginAssets().ConfigureAwait(false);

        private record AssetInfo(string FileName, Uri DownloadUri);

        private readonly List<AssetInfo> requiredAssets =
        [
            new("NotoSansCJKhk-Regular.otf", new Uri("https://github.com/googlefonts/noto-cjk/raw/main/Sans/OTF/TraditionalChineseHK/NotoSansCJKhk-Regular.otf")),
            new("NotoSansCJKjp-Regular.otf", new Uri("https://github.com/googlefonts/noto-cjk/raw/main/Sans/OTF/Japanese/NotoSansCJKjp-Regular.otf")),
            new("NotoSansCJKkr-Regular.otf", new Uri("https://github.com/googlefonts/noto-cjk/raw/main/Sans/OTF/Korean/NotoSansCJKkr-Regular.otf")),
            new("NotoSansCJKsc-Regular.otf", new Uri("https://github.com/googlefonts/noto-cjk/raw/main/Sans/OTF/SimplifiedChinese/NotoSansCJKsc-Regular.otf")),
            new("NotoSansCJKtc-Regular.otf", new Uri("https://github.com/googlefonts/noto-cjk/raw/main/Sans/OTF/TraditionalChinese/NotoSansCJKtc-Regular.otf")),
        ];

        private readonly string[] assetFiles = ["NotoSansCJKhk-Regular.otf", "NotoSansCJKjp-Regular.otf", "NotoSansCJKkr-Regular.otf",
            "NotoSansCJKsc-Regular.otf", "NotoSansCJKtc-Regular.otf"];

        private readonly string[] complementaryFontFiles = ["NotoSansJP-VF-3.ttf", "NotoSansJP-VF-4.ttf", "NotoSansJP-VF-5.ttf",
            "NotoSansJP-VF-6.ttf", "NotoSansJP-VF-7.ttf"];

        private readonly string assetPath = Path.Combine(Service.pluginInterface.AssemblyLocation.Directory?.FullName!, "Font");

        private static readonly HttpClient HttpClient = new();

        private async Task CheckAndDownloadPluginAssets()
        {
            Service.pluginLog.Debug("Checking Plugin assets!");

            Directory.CreateDirectory(assetPath);

            List<AssetInfo> MissingAssetFiles = [.. requiredAssets.Where(asset => !File.Exists(Path.Combine(assetPath, asset.FileName)))];

            if (MissingAssetFiles.Count == 0)
            {
                Service.pluginLog.Debug("All assets present.");

                Service.config.isAssetPresent = true;
                ShowNotification(Resources.AssetsPresentPopupMsg, NotificationType.Success);
                return;
            }

            Service.config.isAssetPresent = false;

            Service.pluginLog.Warning($"Missing {MissingAssetFiles.Count} asset(s): {string.Join(", ", MissingAssetFiles.Select(f => f.FileName))}");
            ShowNotification(Resources.DownloadingAssetsPopupMsg, NotificationType.Warning);

            // Download missing files concurrently
            var downloadTasks = MissingAssetFiles.Select(DownloadAssetAsync).ToList();
            await Task.WhenAll(downloadTasks);

            // Final check after downloads attempt
            MissingAssetFiles = [.. requiredAssets.Where(asset => !File.Exists(Path.Combine(this.assetPath, asset.FileName)))];

            if (MissingAssetFiles.Count == 0)
            {
                Service.config.isAssetPresent = true;
                ShowNotification(Resources.AssetsPresentPopupMsg, NotificationType.Success);
            }
            else
            {
                // Log remaining missing files if any failed
                Service.pluginLog.Error($"Failed to download all assets. Still missing: {string.Join(", ", MissingAssetFiles.Select(f => f.FileName))}");
            }
        }

        private async Task DownloadAssetAsync(AssetInfo asset)
        {
            Service.pluginLog.Debug($"Attempting to download: {asset.FileName}");
            var filePath = Path.Combine(assetPath, asset.FileName);

            try
            {
                using var response = await HttpClient.GetAsync(asset.DownloadUri, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await stream.CopyToAsync(fileStream);
            }
            catch (Exception ex)
            {
                Service.pluginLog.Error($"Unexpected Error downloading {asset.FileName}: {ex.Message}");
                ShowNotification($"{Resources.AssetsDownloadError1stPart} {asset.FileName}{Resources.AssetsDownloadError2ndPart} (Unexpected Error)", NotificationType.Error);

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            Service.pluginLog.Debug($"Successfully downloaded: {asset.FileName}");
        }

        private static void ShowNotification(string message, NotificationType type)
        {
            var notification = new Notification
            {
                Content = message,
                Title = Resources.Name,
                Icon = NotificationUtilities.ToNotificationIcon(Dalamud.Interface.FontAwesomeIcon.Vault),
                Type = type,
            };

            Service.notificationManager.AddNotification(notification);
        }
    }
}
