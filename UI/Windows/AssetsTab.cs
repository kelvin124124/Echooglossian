using Echoglossian.Properties;
using ImGuiNET;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    private partial bool DrawAssetsTab()
    {
        bool saveConfig = false;
        var pluginAssetsStatus = Service.config.PluginAssetsDownloaded;

        ImGui.TextWrapped(Resources.CurrentPluginAssetsStatus + ": " + (pluginAssetsStatus ? "Downloaded" : "Missing"));
        // Removed ImGui.Spacing()

        if (!pluginAssetsStatus) ImGui.TextWrapped(Resources.PluginAssetsNotDownloadedText);
        else ImGui.TextWrapped("Assets appear downloaded. Re-download if needed.");
        // Removed ImGui.Spacing()

        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        if (ImGui.Button(pluginAssetsStatus ? "Re-download Assets" : Resources.DownloadPluginAssetsButtonText))
        {
            try
            {
                this.DownloadAssets(0); this.DownloadAssets(1); this.DownloadAssets(2);
                this.DownloadAssets(3); this.DownloadAssets(4);
                this.PluginAssetsChecker(); // Assumes this updates config flag
                saveConfig = true; // Save potentially updated status
            }
            catch (Exception ex) { PluginLog.Error($"Error during asset download: {ex}"); } // Keep Error Log
        }
        ImGui.PopStyleColor(3);
        return saveConfig;
    }
}
