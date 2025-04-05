using Echoglossian.Properties;
using Echoglossian.Utils;
using ImGuiNET;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    private partial bool DrawAssetsTab()
    {
        bool saveConfig = false;
        var pluginAssetsStatus = Service.config.isAssetPresent;

        ImGui.TextWrapped(Resources.CurrentPluginAssetsStatus + ": " + (pluginAssetsStatus ? "Downloaded" : "Missing"));

        if (!pluginAssetsStatus) ImGui.TextWrapped(Resources.PluginAssetsNotDownloadedText);
        else ImGui.TextWrapped("Assets appear downloaded. Re-download if needed.");

        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        if (ImGui.Button(pluginAssetsStatus ? "Re-download Assets" : Resources.DownloadPluginAssetsButtonText))
        {
            try
            {
                this.DownloadAssets(0); this.DownloadAssets(1); this.DownloadAssets(2);
                this.DownloadAssets(3); this.DownloadAssets(4);
                this.PluginAssetsChecker();
                saveConfig = true;
            }
            catch (Exception ex) { Service.pluginLog.Error($"Error during asset download: {ex}"); }
        }
        ImGui.PopStyleColor(3);
        return saveConfig;
    }
}
