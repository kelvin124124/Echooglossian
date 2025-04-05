using Echoglossian.Properties;
using Echoglossian.Utils;
using ImGuiNET;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    private partial bool DrawJournalTab()
    {
        bool saveConfig = false;
        if (!Service.config.Translate) { ImGui.TextDisabled(Resources.EnableTranslationOptionToShow); return false; }

        saveConfig |= ImGui.Checkbox(Resources.TranslateJournalToggle, ref Service.config.TranslateJournal);

        bool langToRemoveDiacritics = IsDiacriticRemovalLanguage(Service.config.Lang);
        if (Service.config.TranslateJournal && langToRemoveDiacritics)
        {
            ImGui.Indent();
            saveConfig |= ImGui.Checkbox(Resources.RemoveDiacriticsToggle, ref Service.config.RemoveDiacriticsWhenUsingReplacementQuest);
            ImGui.Unindent();
        }
        else if (!langToRemoveDiacritics && Service.config.RemoveDiacriticsWhenUsingReplacementQuest)
        {
            saveConfig |= AssignIfChanged(ref Service.config.RemoveDiacriticsWhenUsingReplacementQuest, false);
        }
        return saveConfig;
    }
}
