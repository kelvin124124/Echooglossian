using Echoglossian.Utils;
using ImGuiNET;
using System.Threading.Tasks;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    // should not apply locaization, in case asset is missing
    private partial void DrawAssetsTab()
    {
        bool pluginAssetsStatus = Service.config.isAssetPresent;

        ImGui.TextUnformatted("Asset status" + ": " + (pluginAssetsStatus ? "✓ Downloaded" : "X Missing"));

        if (!pluginAssetsStatus) ImGui.TextUnformatted("Some asset files are missing, press the button below to re-download them.");
        else ImGui.NewLine();

        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        if (ImGui.Button("Verify Assets"))
        {
            Task.Run(() => Service.assetManager.VerifyPluginAssets());
        }
        ImGui.PopStyleColor(3);
    }
}
