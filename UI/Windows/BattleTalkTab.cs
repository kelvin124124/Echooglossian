using Echoglossian.Properties;
using ImGuiNET;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    private partial bool DrawBattleTalkTab()
    {
        bool saveConfig = false;
        if (!Service.config.Translate) { ImGui.TextDisabled(Resources.EnableTranslationOptionToShow); return false; }

        saveConfig |= ImGui.Checkbox(Resources.TransLateBattletalkToggle, ref Service.config.TranslateBattleTalk);

        if (Service.config.TranslateBattleTalk)
        {
            ImGui.Indent();
            if (Service.config.OverlayOnlyLanguage)
            {
                ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), Resources.FeatureNotAvailableForLanguage);
                saveConfig |= AssignIfChanged(ref Service.config.TranslateBattleTalk, false); // Force disable
                ImGui.Unindent(); return saveConfig;
            }
            /* else { saveConfig |= ImGui.Checkbox(Resources.OverlayToggleLabel, ref Service.config.UseImGuiForBattleTalk); } */ // Keep commented if feature not ready

            saveConfig |= ImGui.Checkbox(Resources.TranslateNpcNamesToggle, ref Service.config.TranslateNpcNames);
            // Removed ImGui.Spacing()
            ImGui.Separator();

            // Keep commented overlay settings block if feature is planned but not ready
            /* if (Service.config.UseImGuiForBattleTalk) { ... } */
            if (Service.config.UseImGuiForBattleTalk) // Check if the config flag exists and is true
            { ImGui.TextDisabled("BattleTalk Overlay settings are not implemented yet."); }

            ImGui.Unindent();
        }
        return saveConfig;
    }
}
