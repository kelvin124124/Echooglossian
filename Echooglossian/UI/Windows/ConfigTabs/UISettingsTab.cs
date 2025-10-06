using Dalamud.Bindings.ImGui;
using Echooglossian.Utils;
using System;
using System.Numerics;

namespace Echooglossian.UI.Windows.ConfigTabs;

public static class UISettingsTab
{
    public static bool Draw()
    {
        ImGui.TextUnformatted("UI Settings");
        ImGui.Separator();

        bool configChanged = false;

        // Color settings
        configChanged |= DrawColorSettings();

        ImGui.Separator();

        // Position corrections
        configChanged |= DrawPositionCorrections();

        ImGui.Separator();

        // Font multipliers
        configChanged |= DrawFontMultipliers();

        return configChanged;
    }

    private static bool DrawColorSettings()
    {
        bool configChanged = false;

        ImGui.TextUnformatted("Colors:");
        
        configChanged |= DrawColorEdit4("Talk Text Color", 
            () => Service.config.OverlayTalkTextColor, 
            value => Service.config.OverlayTalkTextColor = value);

        configChanged |= DrawColorEdit4("BattleTalk Text Color", 
            () => Service.config.OverlayBattleTalkTextColor, 
            value => Service.config.OverlayBattleTalkTextColor = value);

        return configChanged;
    }

    private static bool DrawPositionCorrections()
    {
        bool configChanged = false;

        ImGui.TextUnformatted("Position Corrections:");
        
        configChanged |= DrawSliderFloat2("Window Position Correction", 
            () => Service.config.ImGuiWindowPosCorrection, 
            value => Service.config.ImGuiWindowPosCorrection = value, 
            -500f, 500f);

        configChanged |= DrawSliderFloat2("Toast Position Correction", 
            () => Service.config.ImGuiToastWindowPosCorrection, 
            value => Service.config.ImGuiToastWindowPosCorrection = value, 
            -500f, 500f);

        return configChanged;
    }

    private static bool DrawFontMultipliers()
    {
        bool configChanged = false;

        ImGui.TextUnformatted("Font Size Multipliers:");
        
        configChanged |= DrawSliderFloat("Talk Font Multiplier", 
            () => Service.config.ImGuiTalkFontMult, 
            value => Service.config.ImGuiTalkFontMult = value, 
            0.5f, 3.0f);

        configChanged |= DrawSliderFloat("BattleTalk Font Multiplier", 
            () => Service.config.ImGuiBattleTalkFontMult, 
            value => Service.config.ImGuiBattleTalkFontMult = value, 
            0.5f, 3.0f);

        configChanged |= DrawSliderFloat("Subtitle Font Multiplier", 
            () => Service.config.ImGuiTalkSubtitleFontMult, 
            value => Service.config.ImGuiTalkSubtitleFontMult = value, 
            0.5f, 3.0f);

        configChanged |= DrawSliderFloat("Toast Font Multiplier", 
            () => Service.config.ImGuiToastFontMult, 
            value => Service.config.ImGuiToastFontMult = value, 
            0.5f, 3.0f);

        return configChanged;
    }

    private static bool DrawColorEdit4(string label, Func<Vector4> getter, Action<Vector4> setter)
    {
        var value = getter();
        if (ImGui.ColorEdit4(label, ref value))
        {
            setter(value);
            return true;
        }
        return false;
    }

    private static bool DrawSliderFloat(string label, Func<float> getter, Action<float> setter, float min, float max)
    {
        float value = getter();
        if (ImGui.SliderFloat(label, ref value, min, max))
        {
            setter(value);
            return true;
        }
        return false;
    }

    private static bool DrawSliderFloat2(string label, Func<Vector2> getter, Action<Vector2> setter, float min, float max)
    {
        var value = getter();
        if (ImGui.SliderFloat2(label, ref value, min, max))
        {
            setter(value);
            return true;
        }
        return false;
    }
}