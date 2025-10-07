using Dalamud.Bindings.ImGui;
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
            () => Service.configuration.PluginEnabled,
            value => Service.configuration.PluginEnabled = value);

        ImGui.Separator();

        // Language settings
        configChanged |= DrawLanguageSettings();

        ImGui.Separator();

        // Font settings
        configChanged |= DrawFontSettings();

        ImGui.Separator();

        // Other general settings
        configChanged |= DrawCheckbox("Show translations in cutscenes",
            () => Service.configuration.ShowInCutscenes,
            value =>
            {
                Service.configuration.ShowInCutscenes = value;
                Service.pluginInterface.UiBuilder.DisableCutsceneUiHide = value;
            });

        return configChanged;
    }

    private static bool DrawLanguageSettings()
    {
        bool configChanged = false;

        // Plugin Language
        ImGui.TextUnformatted("Plugin Language:");
        var selectedPluginLanguage = Service.configuration.SelectedPluginLanguage;
        if (ImGui.BeginCombo("##PluginLanguage", selectedPluginLanguage.Name))
        {
            foreach (var lang in GetLanguages())
            {
                bool isSelected = lang.Code == selectedPluginLanguage.Code;
                if (ImGui.Selectable(lang.Name, isSelected))
                {
                    Service.configuration.SelectedPluginLanguage = lang;
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
        var selectedTargetLanguage = Service.configuration.SelectedTargetLanguage;
        if (ImGui.BeginCombo("##TargetLanguage", selectedTargetLanguage.Name))
        {
            foreach (var lang in GetLanguages())
            {
                bool isSelected = lang.Code == selectedTargetLanguage.Code;
                if (ImGui.Selectable(lang.Name, isSelected))
                {
                    Service.configuration.SelectedTargetLanguage = lang;
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
            () => Service.configuration.FontSize,
            value => Service.configuration.FontSize = value,
            8.0f, 48.0f, "%.1f");

        configChanged |= DrawSliderFloat("Font Scale",
            () => Service.configuration.FontScale,
            value => Service.configuration.FontScale = value,
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
