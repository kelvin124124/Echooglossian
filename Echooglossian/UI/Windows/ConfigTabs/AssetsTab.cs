using Dalamud.Bindings.ImGui;
using Echooglossian.Utils;
using System.Numerics;
using System.Threading.Tasks;

namespace Echooglossian.UI.Windows.ConfigTabs;

public static class AssetsTab
{
    public static bool Draw()
    {
        ImGui.TextUnformatted("Asset Management");
        ImGui.Separator();

        DrawAssetStatus();
        
        if (ImGui.Button("Verify and Download Assets"))
        {
            Task.Run(() => Service.assetManager.VerifyPluginAssets());
        }

        ImGui.Separator();
        ImGui.TextWrapped("Asset Status: Assets are required for proper font rendering across different languages. If you experience display issues with certain languages, try verifying assets.");

        return false; // Assets tab doesn't modify config directly
    }

    private static void DrawAssetStatus()
    {
        bool pluginAssetsStatus = Service.config.AssetPresent;

        if (pluginAssetsStatus)
        {
            ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "✓ All assets are present.");
        }
        else
        {
            ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "✗ Some assets are missing.");
            ImGui.TextWrapped("Some asset files are missing. These may include fonts required for proper text rendering in different languages.");
        }
    }
}