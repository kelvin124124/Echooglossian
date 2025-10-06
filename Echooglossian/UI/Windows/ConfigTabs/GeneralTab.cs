using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Echooglossian.Localization;
using Echooglossian.Utils;
using System;
using System.Globalization;
using static Echooglossian.Utils.LanguageDictionary;

namespace Echooglossian.UI.Windows.ConfigTabs;

public static class GeneralTab
{
    public static bool Draw()
    {
        bool configChanged = false;

        // Plugin enable main toggle
        configChanged |= DrawCheckbox("Enable Plugin", 
            () => Service.config.PluginEnabled, 
            value => Service.config.PluginEnabled = value);

        ImGui.Separator();

        // Language settings
        configChanged |= DrawLanguageSettings();

        ImGui.Separator();

        // Font settings
        configChanged |= DrawFontSettings();

        ImGui.Separator();

        // Other general settings
        configChanged |= DrawCheckbox("Show translations in cutscenes", 
            () => Service.config.ShowInCutscenes, 
            value => {
                Service.config.ShowInCutscenes = value;
                Service.pluginInterface.UiBuilder.DisableCutsceneUiHide = value;
            });

        return configChanged;
    }

    private static bool DrawLanguageSettings()
    {
        bool configChanged = false;

        // Plugin Language
        ImGui.TextUnformatted("Plugin Language:");
        var selectedPluginLanguage = Service.config.SelectedPluginLanguage;
        if (ImGui.BeginCombo("##PluginLanguage", selectedPluginLanguage.Name))
        {
            foreach (var lang in GetLanguages())
            {
                bool isSelected = lang.Code == selectedPluginLanguage.Code;
                if (ImGui.Selectable(lang.Name, isSelected))
                {
                    Service.config.SelectedPluginLanguage = lang;
                    Resources.Culture = (CultureInfo)lang;
                    configChanged = true;
                }
                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        // Target Language
        ImGui.TextUnformatted("Target Language:");
        var selectedTargetLanguage = Service.config.SelectedTargetLanguage;
        if (ImGui.BeginCombo("##TargetLanguage", selectedTargetLanguage.Name))
        {
            foreach (var lang in GetLanguages())
            {
                bool isSelected = lang.Code == selectedTargetLanguage.Code;
                if (ImGui.Selectable(lang.Name, isSelected))
                {
                    Service.config.SelectedTargetLanguage = lang;
                    configChanged = true;
                }
                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        return configChanged;
    }

    private static bool DrawFontSettings()
    {
        bool configChanged = false;

        ImGui.TextUnformatted("Font Settings:");
        
        configChanged |= DrawSliderFloat("Font Size", 
            () => Service.config.FontSize, 
            value => Service.config.FontSize = value, 
            8.0f, 48.0f, "%.1f");

        configChanged |= DrawSliderFloat("Font Scale", 
            () => Service.config.FontScale, 
            value => Service.config.FontScale = value, 
            0.5f, 3.0f, "%.2f");

        return configChanged;
    }

    private static bool DrawCheckbox(string label, Func<bool> getter, Action<bool> setter)
    {
        bool value = getter();
        if (ImGui.Checkbox(label, ref value))
        {
            setter(value);
            return true;
        }
        return false;
    }

    private static bool DrawSliderFloat(string label, Func<float> getter, Action<float> setter, float min, float max, string format)
    {
        float value = getter();
        if (ImGui.SliderFloat(label, ref value, min, max, format))
        {
            setter(value);
            return true;
        }
        return false;
    }
}