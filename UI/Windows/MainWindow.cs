using Dalamud.Interface.Windowing;
using Echoglossian.Properties;
using ImGuiNET;
using System.Numerics;

namespace Echoglossian.UI.Windows;

public partial class MainWindow : Window
{
    private bool saveConfig = false;

    public MainWindow(Echoglossian plugin) : base(
    $"Echoglossian - Plugin Version: {Service.config.PluginVersion}",
    ImGuiWindowFlags.AlwaysAutoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(900, 700),
            MaximumSize = new Vector2(1920, 1080)
        };
    }

    public override void Draw()
    {
        // Configuration Tabs
        if (ImGui.BeginTabBar("ConfigTabBar", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton))
        {
            if (ImGui.BeginTabItem(Resources.GeneralTabName)) { saveConfig |= DrawGeneralTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Resources.TalkTabName)) { saveConfig |= DrawTalkTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Resources.BattleTalkTabName)) { saveConfig |= DrawBattleTalkTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Resources.ToastTabName)) { saveConfig |= DrawToastsTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Resources.JournalTabName)) { saveConfig |= DrawJournalTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Resources.TalkSubtitileTabName)) { saveConfig |= DrawTalkSubtitleTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Resources.TranslationEngineTabName)) { saveConfig |= DrawEngineSettingsTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Resources.AssetTabName)) { saveConfig |= DrawAssetsTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Resources.MiscTabName)) { saveConfig |= DrawMiscTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem(Resources.AboutTabName)) { DrawAboutTab(); ImGui.EndTabItem(); }
            ImGui.EndTabBar();
        }
    }

    private partial bool DrawGeneralTab();
    private partial bool DrawTalkTab();
    private partial bool DrawBattleTalkTab();
    private partial bool DrawToastsTab();
    private partial bool DrawJournalTab();
    private partial bool DrawTalkSubtitleTab();
    private partial bool DrawEngineSettingsTab();
    private partial bool DrawAssetsTab();
    private partial bool DrawMiscTab();
    private partial void DrawAboutTab();
}
