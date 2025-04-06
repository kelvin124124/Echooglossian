using Echoglossian.Properties;
using Echoglossian.Utils;
using ImGuiNET;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    private partial bool DrawTalkTab()
    {
        bool saveConfig = false;

        if (!Service.config.Translate)
        {
            ImGui.TextDisabled(Resources.EnableTranslationOptionToShow);
            return false;
        }

        // Enable/Disable Talk Translation
        saveConfig |= ImGui.Checkbox(Resources.TranslateTalkToggleLabel, ref Service.config.TranslateTalk);

        if (Service.config.TranslateTalk)
        {
            ImGui.Indent();

            if (Service.config.OverlayOnlyLanguage)
            {
                // Force Overlay for these languages
                saveConfig |= AssignIfChanged(ref Service.config.UseImGuiForTalk, true);
                // Ensure swap is off if overlay is forced (as swap relies on replacing game UI)
                saveConfig |= AssignIfChanged(ref Service.config.SwapTextsUsingImGui, false);
                ImGui.BeginDisabled(); // Visually disable the checkbox
                ImGui.Checkbox(Resources.OverlayToggleLabel, ref Service.config.UseImGuiForTalk);
                ImGui.EndDisabled();
                ImGui.SameLine();
                ImGui.Text($"({Resources.RequiredForThisLanguage})");
            }
            else
            {
                // Allow user choice for other languages
                saveConfig |= ImGui.Checkbox(Resources.OverlayToggleLabel, ref Service.config.UseImGuiForTalk);
            }

            // Translate NPC Names Toggle
            saveConfig |= ImGui.Checkbox(Resources.TranslateNpcNamesToggle, ref Service.config.TranslateNpcNames);

            ImGui.Spacing();
            ImGui.Separator();

            // Overlay Specific Adjustments
            if (Service.config.UseImGuiForTalk)
            {
                ImGui.Text(Resources.ImguiAdjustmentsLabel);

                // Font Scale Slider
                if (ImGui.SliderFloat(Resources.OverlayFontScaleLabel, ref Service.config.FontScale, -3f, 3f, "%.2f"))
                {
                    saveConfig = true;
                    Service.config.FontChangeTime = DateTime.Now.Ticks; // Record time for potential debouncing/delayed application
                }
                ImGui.SameLine();
                ImGui.Text(Resources.HoverTooltipIndicator);
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(Resources.OverlayFontSizeOrientations);

                // Font Color Picker
                ImGui.Text(Resources.FontColorSelectLabel);
                ImGui.SameLine();
                saveConfig |= ImGui.ColorEdit3(Resources.OverlayColorSelectName, ref Service.config.OverlayTalkTextColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel);
                ImGui.SameLine();
                ImGui.Text(Resources.HoverTooltipIndicator);
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(Resources.OverlayFontColorOrientations);

                ImGui.Spacing();
                ImGui.Separator();

                // Width/Height Multipliers
                saveConfig |= ImGui.DragFloat(Resources.OverlayWidthScrollLabel, ref Service.config.ImGuiTalkWindowWidthMult, 0.001f, 0.01f, 3f, "%.3f");
                ImGui.Separator();
                saveConfig |= ImGui.DragFloat(Resources.OverlayHeightScrollLabel, ref Service.config.ImGuiTalkWindowHeightMult, 0.001f, 0.01f, 3f, "%.3f");
                ImGui.Separator();
                ImGui.Spacing();

                // Position Adjustment
                saveConfig |= ImGui.DragFloat2(Resources.OverlayPositionAdjustmentLabel, ref Service.config.ImGuiWindowPosCorrection);
                ImGui.SameLine();
                ImGui.Text(Resources.HoverTooltipIndicator);
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(Resources.OverlayAdjustmentOrientations);
            }

            // Swap Text Option (Only if Overlay is *optional* and enabled)
            ImGui.Spacing();
            ImGui.Separator();
            if (!Service.config.OverlayOnlyLanguage && Service.config.UseImGuiForTalk)
            {
                saveConfig |= ImGui.Checkbox(Resources.SwapTranslationTextToggle, ref Service.config.SwapTextsUsingImGui);

                // Diacritics removal only relevant if swapping text
                bool langToRemoveDiacritics = IsDiacriticRemovalLanguage(Service.config.Lang);
                if (Service.config.SwapTextsUsingImGui && langToRemoveDiacritics)
                {
                    ImGui.Indent();
                    saveConfig |= ImGui.Checkbox(Resources.RemoveDiacriticsToggle, ref Service.config.RemoveDiacriticsWhenUsingReplacementTalkBTalk);
                    ImGui.Unindent();
                }
            }
            else if (Service.config.SwapTextsUsingImGui)
            {
                // If overlay became mandatory, force swap off
                saveConfig |= AssignIfChanged(ref Service.config.SwapTextsUsingImGui, false);
            }

            ImGui.Unindent(); // Unindent Talk settings
        } // End if TranslateTalk

        return saveConfig;
    }

    // Helper to check if the current language is one that might benefit from diacritic removal
    // Keep this private or move to a shared utility class if used elsewhere
    private static bool IsDiacriticRemovalLanguage(int langId)
    {
        // Original list: 24, 25, 44, 60, 61, 80, 83, 87, 91, 104, 105, 109, 110
        return langId is 24 or 25 or 44 or 60 or 61 or 80 or 83 or 87 or 91 or 104 or 105 or 109 or 110;
    }
}
