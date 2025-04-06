using Dalamud.Interface.ImGuiNotification;
using Echoglossian.Properties;
using Echoglossian.Utils;
using System.Security.Cryptography;

namespace Echoglossian
{
    public class AssetManager
    {
        public AssetManager() => VerifyPluginAssets().ConfigureAwait(false);

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

        public async Task VerifyPluginAssets()
        {
            Service.pluginLog.Debug("Downloading plugin assets.");

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
            Service.config.Save();

            Service.pluginLog.Warning($"Missing {MissingAssetFiles.Count} asset(s): {string.Join(", ", MissingAssetFiles.Select(f => f.FileName))}");
            ShowNotification(Resources.DownloadingAssetsPopupMsg, NotificationType.Warning);

            // Download missing files concurrently
            var downloadTasks = MissingAssetFiles.Select(DownloadAssetAsync).ToList();
            await Task.WhenAll(downloadTasks);

            // Final check after downloads attempt
            MissingAssetFiles = [.. requiredAssets.Where(asset => !File.Exists(Path.Combine(assetPath, asset.FileName)))];

            if (MissingAssetFiles.Count == 0)
            {
                Service.config.isAssetPresent = true;
                Service.config.Save();

                ShowNotification(Resources.AssetsPresentPopupMsg, NotificationType.Success);
            }
            else
            {
                // Log remaining missing files if any failed
                Service.pluginLog.Error($"Failed to download all assets. Still missing: {string.Join(", ", MissingAssetFiles.Select(f => f.FileName))}");
            }
        }

        private bool AreAssetsIntact()
        {
            using var md5 = MD5.Create();
            foreach (var file in assetFiles)
            {
                var filePath = Path.Combine(assetPath, file);
                if (!File.Exists(filePath))
                {
                    return false;
                }

                try
                {
                    using var stream = File.OpenRead(filePath);

                    var hash = md5.ComputeHash(stream);
                    if (!hash.SequenceEqual(expectedHashes[file])) return false;
                }
                catch (Exception ex)
                {
                    Service.pluginLog.Error($"Failed to verify {file}: {ex.Message}");
                    ShowNotification($"{file} is inaccessible.", NotificationType.Error);
                    return false;
                }
            }
            return true;
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
                Icon = Dalamud.Interface.FontAwesomeIcon.Vault.ToNotificationIcon(),
                Type = type,
            };

            Service.notificationManager.AddNotification(notification);
        }
    }
}
