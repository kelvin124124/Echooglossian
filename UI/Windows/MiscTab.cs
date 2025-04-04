using Echoglossian.Properties;
using ImGuiNET;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    private partial bool DrawMiscTab()
    {
        bool saveConfig = false;
        ImGui.TextWrapped(Resources.ConfigTab9Text);
        ImGui.Separator();

        saveConfig |= ImGui.Checkbox(Resources.ConfigTab9CheckboxClipboardText, ref Service.config.CopyTranslationToClipboard);
        ImGui.Indent(); ImGui.TextWrapped(Resources.ConfigTab9CheckboxClipboardTooltipText); ImGui.Unindent();
        return saveConfig;
    }
}
