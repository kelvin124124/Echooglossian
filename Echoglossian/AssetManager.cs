using Dalamud.Interface.ImGuiNotification;
using Echoglossian.Localization;
using Echoglossian.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Echoglossian
{
    public class AssetManager
    {
        public AssetManager() => VerifyPluginAssets().ConfigureAwait(false);

        private record AssetInfo(string FileName, string checkSum, Uri DownloadUri);

        private readonly List<AssetInfo> requiredAssets =
        [
            new("NotoSansCJKhk-Regular.otf", "8b6616c27c14504089bce94f6393b556", new Uri("https://github.com/googlefonts/noto-cjk/raw/main/Sans/OTF/TraditionalChineseHK/NotoSansCJKhk-Regular.otf")),
            new("NotoSansCJKjp-Regular.otf", "6d57a40c6695bd46457315e2a9dc757a", new Uri("https://github.com/googlefonts/noto-cjk/raw/main/Sans/OTF/Japanese/NotoSansCJKjp-Regular.otf")),
            new("NotoSansCJKkr-Regular.otf", "be4ea586e2517793c1b6eb84f4f86c12", new Uri("https://github.com/googlefonts/noto-cjk/raw/main/Sans/OTF/Korean/NotoSansCJKkr-Regular.otf")),
            new("NotoSansCJKsc-Regular.otf", "6d30d01b9b25bd766898ecba71d5cd3d", new Uri("https://github.com/googlefonts/noto-cjk/raw/main/Sans/OTF/SimplifiedChinese/NotoSansCJKsc-Regular.otf")),
            new("NotoSansCJKtc-Regular.otf", "6d2d993ad59dd5c622786f1e79a97bbc", new Uri("https://github.com/googlefonts/noto-cjk/raw/main/Sans/OTF/TraditionalChinese/NotoSansCJKtc-Regular.otf")),
        ];

        private readonly string assetPath = Path.Combine(Service.pluginInterface.AssemblyLocation.Directory?.FullName!, "Font");

        private static readonly HttpClient HttpClient = new();

        public async Task VerifyPluginAssets()
        {
            Service.pluginLog.Debug("Verifying plugin assets.");
            ShowNotification("Verifying plugin assets...", NotificationType.Info);

            Directory.CreateDirectory(assetPath);

            var missingAssets = GetMissingAssets();

            if (missingAssets.Count == 0)
            {
                Service.pluginLog.Debug("Plugin assets are intact.");
                ShowNotification(Resources.AssetsPresentPopupMsg, NotificationType.Success);
                return;
            }

            // files are missing or corrupted
            Service.config.isAssetPresent = false;
            Service.config.Save();

            Service.pluginLog.Warning($"Missing {missingAssets.Count} asset(s): {string.Join(", ", missingAssets.Select(f => f.FileName))}");
            ShowNotification(Resources.DownloadingAssetsPopupMsg, NotificationType.Warning);

            // Download missing files concurrently
            var downloadTasks = missingAssets.Select(DownloadAssetAsync).ToList();
            await Task.WhenAll(downloadTasks);

            missingAssets = GetMissingAssets();
            if (missingAssets.Count == 0)
            {
                Service.config.isAssetPresent = true;
                Service.config.Save();

                ShowNotification(Resources.AssetsPresentPopupMsg, NotificationType.Success);
            }
            else
            {
                Service.pluginLog.Error($"Failed to download all assets. Still missing: {string.Join(", ", missingAssets.Select(f => f.FileName))}");
            }
        }

        private List<AssetInfo> GetMissingAssets()
        {
            using var md5 = MD5.Create();
            return requiredAssets.Where(asset =>
            {
                var filePath = Path.Combine(assetPath, asset.FileName);
                if (!File.Exists(filePath))
                    return true;

                try
                {
                    using var stream = File.OpenRead(filePath);
                    var hash = string.Concat(md5.ComputeHash(stream).Select(b => b.ToString("x2")));
                    if (hash != asset.checkSum)
                    {
                        Service.pluginLog.Warning($"Checksum mismatch for {asset.FileName}.");
                        ShowNotification($"{asset.FileName} is corrupted.", NotificationType.Error);
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error reading {asset.FileName}: {ex.Message}", ex);
                }
            }).ToList();
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
                ShowNotification($"Unexpected error when downloading plugin assets.", NotificationType.Error);

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
                Title = Resources.PluginName,
                Icon = Dalamud.Interface.FontAwesomeIcon.Vault.ToNotificationIcon(),
                Type = type,
            };

            Service.notificationManager.AddNotification(notification);
        }
    }
}
