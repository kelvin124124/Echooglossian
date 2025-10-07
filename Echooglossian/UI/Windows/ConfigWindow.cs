using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Echooglossian.Localization;
using Echooglossian.UI.Windows.ConfigTabs;
using Echooglossian.Utils;
using System.Globalization;
using System.Numerics;

namespace Echooglossian.UI.Windows;

public partial class ConfigWindow : Window
{
    private bool saveConfig = false;

    public ConfigWindow(Plugin plugin) : base(
        $"Echoglossian - Plugin Version: {Service.configuration.PluginVersion}",
        ImGuiWindowFlags.None)
    {
        Size = new Vector2(1000, 600);
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 500),
            MaximumSize = new Vector2(1200, 800)
        };

        Resources.Culture = (CultureInfo)Service.configuration.SelectedPluginLanguage;

        if (Service.configuration.LLMPresets.Count == 0)
        {
            Service.configuration.LLMPresets.AddRange(DefaultLLMPresets.Default);
        }
    }

    // TODO: use general font handle built in font manager
    public override void Draw()
    {
        // Main tab bar
        if (ImGui.BeginTabBar("ConfigTabBar##Echo"))
        {
            if (ImGui.BeginTabItem("General"))
            {
                saveConfig |= GeneralTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Translation Engine"))
            {
                saveConfig |= TranslationEngineTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Modules"))
            {
                saveConfig |= ModulesTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("UI Settings"))
            {
                saveConfig |= UISettingsTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Assets"))
            {
                AssetsTab.Draw();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        if (saveConfig)
        {
            Service.configuration.Save();
            saveConfig = false;
        }
    }

}
