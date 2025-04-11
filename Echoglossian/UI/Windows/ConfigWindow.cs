using Dalamud.Interface.Windowing;
using Echoglossian.Localization;
using Echoglossian.Utils;
using ImGuiNET;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;
using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian.UI.Windows;

public partial class ConfigWindow : Window
{
    private bool saveConfig = false;

    public ConfigWindow(Echoglossian plugin) : base(
        $"Echoglossian - Plugin Version: {Service.config.PluginVersion}",
        ImGuiWindowFlags.AlwaysAutoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(900, 700),
            MaximumSize = new Vector2(1920, 1080)
        };

        Resources.Culture = (CultureInfo)Service.config.SelectedPluginLanguage;
    }

    public override void Draw()
    {
        // Plugin enable main toggle
        bool _pluginEnabled = Service.config.PluginEnabled;
        if (ImGui.Checkbox(Resources.PluginMainToggle, ref _pluginEnabled))
        {
            Service.config.PluginEnabled = true;
            saveConfig = true;
        }

        // Target Language selection
        ImGui.TextUnformatted(Resources.TargetLanguage);
        ImGui.SameLine();
        var selectedLanguage = Service.config.SelectedPluginLanguage;
        if (ImGui.BeginCombo("##TargetLanguage###Echo", selectedLanguage.Name))
        {
            foreach (var lang in GetLanguages())
            {
                bool isSelected = lang == selectedLanguage;
                if (ImGui.Selectable(lang.Name, isSelected))
                {
                    Service.config.SelectedPluginLanguage = lang;
                    saveConfig = true;
                }
                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        ImGui.Separator();

        // Module configuration tab
        // Left panel: Module list [Talk, BattleTalk, Toast, Journal, TalkSubstitle, Chat, PF] with image showcase
        if (ImGui.CollapsingHeader(Resources.ModulesConfiguration, ImGuiTreeNodeFlags.None))
        {
            short CurrentTab = 0;
            string[] tabs = [Resources.TalkModuleName, Resources.BattleTalkModuleName, Resources.ToastModuleName,
                Resources.JournalModuleName, Resources.TalkSubstitleModuleName, Resources.ChatModuleName, Resources.PFModuleName];

            ImGui.Columns(2, "ConfigColumns###Echo", false);
            ImGui.SetColumnWidth(0, 200);
            if (ImGui.BeginChild("TabsBox###Echo", new Vector2(190, 300), true))
            {
                for (short i = 0; i < tabs.Length; i++)
                {
                    if (ImGui.Selectable(tabs[i], CurrentTab == i))
                    {
                        CurrentTab = i;
                    }
                }
            }
            ImGui.EndChild();

            ImGui.NextColumn();

            // Right panel: Module configuration
            if (ImGui.BeginChild("ConfigBox", new Vector2(0, 300), true))
            {
                switch (CurrentTab)
                {
                    case 0:
                        ImGui.TextUnformatted(Resources.TalkModuleName);
                        break;
                    case 1:
                        ImGui.TextUnformatted(Resources.BattleTalkModuleName);
                        break;
                    case 2:
                        ImGui.TextUnformatted(Resources.ToastModuleName);
                        break;
                    case 3:
                        ImGui.TextUnformatted(Resources.JournalModuleName);
                        break;
                    case 4:
                        ImGui.TextUnformatted(Resources.TalkSubstitleModuleName);
                        break;
                    case 5:
                        ImGui.TextUnformatted(Resources.ChatModuleName);
                        break;
                    case 6:
                        ImGui.TextUnformatted(Resources.PFModuleName);
                        break;
                        // ... other cases
                }
            }
            ImGui.EndChild();

            ImGui.Columns(1);
        }

        // Asset configuration tab (should NOT apply localization)
        ImGui.PushStyleColor(ImGuiCol.Header, 0xFF000000 | 0x005E5BFF);
        if (ImGui.CollapsingHeader("Assets configuration", ImGuiTreeNodeFlags.None))
        {
            bool pluginAssetsStatus = Service.config.isAssetPresent;

            if (pluginAssetsStatus)
            {
                // green text
                ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "All assets are present.");
            }
            else
            {
                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "Some assets are missing.");
                ImGui.TextUnformatted("Some asset files are missing, press the button below to re-download them.");
            }

            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
            if (ImGui.Button("Verify Assets"))
            {
                Task.Run(() => Service.assetManager.VerifyPluginAssets());
            }
            ImGui.PopStyleColor(1);
        }
        ImGui.PopStyleColor(1);

        // Engine configuration tab
        if (ImGui.CollapsingHeader(Resources.TranslationEngineConfiguration, ImGuiTreeNodeFlags.None))
        {

        }

        // About footer


        if (saveConfig)
        {
            Service.config.Save();
            saveConfig = false;
        }
    }
}
