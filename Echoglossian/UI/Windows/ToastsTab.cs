using Echoglossian.Properties;
using Echoglossian.Utils;
using ImGuiNET;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    private partial bool DrawToastsTab()
    {
        bool saveConfig = false;
        if (!Service.config.Translate) { ImGui.TextDisabled(Resources.EnableTranslationOptionToShow); return false; }

        saveConfig |= ImGui.Checkbox(Resources.TranslateToastToggleText, ref Service.config.TranslateToast);

        if (Service.config.TranslateToast)
        {
            ImGui.Indent();
            if (Service.config.OverlayOnlyLanguage)
            {
                saveConfig |= AssignIfChanged(ref Service.config.UseImGuiForToasts, true);
                ImGui.BeginDisabled(); ImGui.Checkbox(Resources.UseImGuiForToastsToggle, ref Service.config.UseImGuiForToasts); ImGui.EndDisabled();
                ImGui.SameLine(); ImGui.Text($"({Resources.RequiredForThisLanguage})");
            }
            else { saveConfig |= ImGui.Checkbox(Resources.UseImGuiForToastsToggle, ref Service.config.UseImGuiForToasts); }

            ImGui.Separator(); // Keep separator before toast types
            ImGui.Text(Resources.WhichToastsToTranslate);
            ImGui.Indent();
            saveConfig |= ImGui.Checkbox(Resources.TranslateErrorToastToggleText, ref Service.config.TranslateErrorToast);
            saveConfig |= ImGui.Checkbox(Resources.TranslateQuestToastToggleText, ref Service.config.TranslateQuestToast);
            saveConfig |= ImGui.Checkbox(Resources.TranslateAreaToastToggleText, ref Service.config.TranslateAreaToast);
            saveConfig |= ImGui.Checkbox(Resources.TranslateClassChangeToastToggleText, ref Service.config.TranslateClassChangeToast);
            saveConfig |= ImGui.Checkbox(Resources.TranslateScreenInfoToastToggleText, ref Service.config.TranslateWideTextToast);
            ImGui.Unindent();
            ImGui.Separator(); // Keep separator after toast types

            if (Service.config.UseImGuiForToasts)
            {
                ImGui.Text(Resources.ImguiAdjustmentsLabel);
                saveConfig |= ImGui.DragFloat(Resources.ToastOverlayWidthScrollLabel, ref Service.config.ImGuiToastWindowWidthMult, 0.001f, 0.01f, 3f, "%.3f");
                ImGui.SameLine(); ImGui.Text(Resources.HoverTooltipIndicator); if (ImGui.IsItemHovered()) ImGui.SetTooltip(Resources.ToastOverlayWidthMultiplierOrientations);
            }
            ImGui.Unindent();
        }
        return saveConfig;
    }
}
