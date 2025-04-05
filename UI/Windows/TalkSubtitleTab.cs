using Echoglossian.Properties;
using Echoglossian.Utils;
using ImGuiNET;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    private bool DrawTalkSubtitleTab()
    {
        bool saveConfig = false;
        if (!Service.config.Translate) { ImGui.TextDisabled(Resources.EnableTranslationOptionToShow); return false; }

        saveConfig |= ImGui.Checkbox(Resources.TranslateTalkSubtitleToggleLabel, ref Service.config.TranslateTalkSubtitle);

        if (Service.config.TranslateTalkSubtitle)
        {
            ImGui.Indent();
            if (Service.config.OverlayOnlyLanguage)
            {
                saveConfig |= AssignIfChanged(ref Service.config.UseImGuiForTalkSubtitle, true);
                saveConfig |= AssignIfChanged(ref Service.config.SwapTextsUsingImGui, false); // Assuming same swap flag
                ImGui.BeginDisabled(); ImGui.Checkbox(Resources.OverlayToggleLabel, ref Service.config.UseImGuiForTalkSubtitle); ImGui.EndDisabled();
                ImGui.SameLine(); ImGui.Text($"({Resources.RequiredForThisLanguage})");
            }
            else { saveConfig |= ImGui.Checkbox(Resources.OverlayToggleLabel, ref Service.config.UseImGuiForTalkSubtitle); }

            ImGui.Separator();

            if (Service.config.UseImGuiForTalkSubtitle)
            {
                ImGui.Text(Resources.ImguiAdjustmentsLabel);
                if (ImGui.SliderFloat(Resources.OverlayFontScaleLabel, ref Service.config.FontScale, -3f, 3f, "%.2f")) // Same font scale
                { saveConfig = true; Service.config.FontChangeTime = DateTime.Now.Ticks; }
                ImGui.SameLine(); ImGui.Text(Resources.HoverTooltipIndicator); if (ImGui.IsItemHovered()) ImGui.SetTooltip(Resources.OverlayFontSizeOrientations);

                ImGui.Text(Resources.FontColorSelectLabel); ImGui.SameLine();
                saveConfig |= ImGui.ColorEdit3(Resources.OverlayColorSelectName, ref Service.config.OverlayTalkTextColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); // Same color
                ImGui.SameLine(); ImGui.Text(Resources.HoverTooltipIndicator); if (ImGui.IsItemHovered()) ImGui.SetTooltip(Resources.OverlayFontColorOrientations);

                ImGui.Separator();
                saveConfig |= ImGui.DragFloat(Resources.OverlayWidthScrollLabel, ref Service.config.ImGuiTalkSubtitleWindowWidthMult, 0.001f, 0.01f, 3f, "%.3f");
                ImGui.Separator();
                saveConfig |= ImGui.DragFloat(Resources.OverlayHeightScrollLabel, ref Service.config.ImGuiTalkSubtitleWindowHeightMult, 0.001f, 0.01f, 3f, "%.3f");
                ImGui.Separator();
                saveConfig |= ImGui.DragFloat2(Resources.OverlayPositionAdjustmentLabel, ref Service.config.ImGuiTalkSubtitleWindowPosCorrection);
                ImGui.SameLine(); ImGui.Text(Resources.HoverTooltipIndicator); if (ImGui.IsItemHovered()) ImGui.SetTooltip(Resources.OverlayAdjustmentOrientations);
            }

            ImGui.Separator();
            if (!Service.config.OverlayOnlyLanguage && Service.config.UseImGuiForTalkSubtitle)
            {
                saveConfig |= ImGui.Checkbox(Resources.SwapTranslationTextToggle, ref Service.config.SwapTextsUsingImGui); // Assuming same swap
                bool langToRemoveDiacritics = IsDiacriticRemovalLanguage(Service.config.Lang);
                if (Service.config.SwapTextsUsingImGui && langToRemoveDiacritics)
                {
                    ImGui.Indent();
                    saveConfig |= ImGui.Checkbox(Resources.RemoveDiacriticsToggle, ref Service.config.RemoveDiacriticsWhenUsingReplacementTalkBTalk); // Assuming same flag
                    ImGui.Unindent();
                }
            }
            else if (Service.config.SwapTextsUsingImGui) { saveConfig |= AssignIfChanged(ref Service.config.SwapTextsUsingImGui, false); }
            ImGui.Unindent();
        }
        return saveConfig;
    }
}
