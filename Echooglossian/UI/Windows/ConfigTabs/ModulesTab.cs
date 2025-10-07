using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Echooglossian.Utils;
using System;
using System.Linq;
using System.Numerics;
using static Echooglossian.Utils.LanguageDictionary;

namespace Echooglossian.UI.Windows.ConfigTabs;

public static class ModulesTab
{
    private static readonly string[] ModuleNames = ["Talk", "BattleTalk", "TalkSubtitle", "Journal", "Toast", "Chat", "PF"];
    private static int selectedModuleTab = 0;

    public static bool Draw()
    {
        bool configChanged = false;

        if (ImGui.BeginChild("ModuleList", new Vector2(200, 0), true))
        {
            for (int i = 0; i < ModuleNames.Length; i++)
            {
                if (ImGui.Selectable(ModuleNames[i], selectedModuleTab == i))
                {
                    selectedModuleTab = i;
                }
            }
        }
        ImGui.EndChild();

        ImGui.SameLine();

        if (ImGui.BeginChild("ModuleConfig", new Vector2(0, 0), true))
        {
            configChanged = selectedModuleTab switch
            {
                0 => TalkModule.Draw(),
                1 => BattleTalkModule.Draw(),
                2 => TalkSubtitleModule.Draw(),
                3 => JournalModule.Draw(),
                4 => ToastModule.Draw(),
                5 => ChatModule.Draw(),
                6 => PFModule.Draw(),
                _ => false
            };
        }
        ImGui.EndChild();

        return configChanged;
    }

    private static class TalkModule
    {
        public static bool Draw()
        {
            ImGui.TextUnformatted("Talk Module Configuration");
            ImGui.Separator();

            bool configChanged = false;
            configChanged |= DrawCheckbox("Enable Talk Module",
                () => Service.configuration.TalkModuleEnabled,
                value => Service.configuration.TalkModuleEnabled = value);

            configChanged |= DrawCheckbox("Use ImGui Overlay",
                () => Service.configuration.TALK_UseImGui,
                value => Service.configuration.TALK_UseImGui = value);

            configChanged |= DrawCheckbox("Translate NPC Names",
                () => Service.configuration.TALK_TranslateNpcNames,
                value => Service.configuration.TALK_TranslateNpcNames = value);

            return configChanged;
        }
    }

    private static class BattleTalkModule
    {
        public static bool Draw()
        {
            ImGui.TextUnformatted("BattleTalk Module Configuration");
            ImGui.Separator();

            bool configChanged = false;
            configChanged |= DrawCheckbox("Enable BattleTalk Module",
                () => Service.configuration.BattleTalkModuleEnabled,
                value => Service.configuration.BattleTalkModuleEnabled = value);

            configChanged |= DrawCheckbox("Use ImGui Overlay",
                () => Service.configuration.BATTLETALK_UseImGui,
                value => Service.configuration.BATTLETALK_UseImGui = value);

            configChanged |= DrawCheckbox("Translate NPC Names",
                () => Service.configuration.BATTLETALK_TranslateNpcNames,
                value => Service.configuration.BATTLETALK_TranslateNpcNames = value);

            return configChanged;
        }
    }

    private static class TalkSubtitleModule
    {
        public static bool Draw()
        {
            ImGui.TextUnformatted("TalkSubtitle Module Configuration");
            ImGui.Separator();

            bool configChanged = false;
            configChanged |= DrawCheckbox("Enable TalkSubtitle Module",
                () => Service.configuration.TalkSubtitleModuleEnabled,
                value => Service.configuration.TalkSubtitleModuleEnabled = value);

            configChanged |= DrawCheckbox("Use ImGui Overlay",
                () => Service.configuration.SUBTITLE_UseImGui,
                value => Service.configuration.SUBTITLE_UseImGui = value);

            return configChanged;
        }
    }

    private static class JournalModule
    {
        public static bool Draw()
        {
            ImGui.TextUnformatted("Journal Module Configuration");
            ImGui.Separator();

            return DrawCheckbox("Enable Journal Module",
                () => Service.configuration.JournalModuleEnabled,
                value => Service.configuration.JournalModuleEnabled = value);
        }
    }

    private static class ToastModule
    {
        public static bool Draw()
        {
            ImGui.TextUnformatted("Toast Module Configuration");
            ImGui.Separator();

            bool configChanged = false;
            configChanged |= DrawCheckbox("Enable Toast Module",
                () => Service.configuration.ToastModuleEnabled,
                value => Service.configuration.ToastModuleEnabled = value);

            configChanged |= DrawCheckbox("Use ImGui Overlay",
                () => Service.configuration.TOAST_UseImGui,
                value => Service.configuration.TOAST_UseImGui = value);

            configChanged |= DrawCheckbox("Translate Regular Toasts",
                () => Service.configuration.TOAST_TranslateRegular,
                value => Service.configuration.TOAST_TranslateRegular = value);

            configChanged |= DrawCheckbox("Translate Error Toasts",
                () => Service.configuration.TOAST_TranslateError,
                value => Service.configuration.TOAST_TranslateError = value);

            configChanged |= DrawCheckbox("Translate Quest Toasts",
                () => Service.configuration.TOAST_TranslateQuest,
                value => Service.configuration.TOAST_TranslateQuest = value);

            return configChanged;
        }
    }

    private static class ChatModule
    {
        public static bool Draw()
        {
            ImGui.TextUnformatted("Chat Module Configuration");
            ImGui.Separator();

            bool configChanged = false;
            configChanged |= DrawCheckbox("Enable Chat Module",
                () => Service.configuration.ChatModuleEnabled,
                value => Service.configuration.ChatModuleEnabled = value);

            configChanged |= DrawCheckbox("Use Context for Translation",
                () => Service.configuration.CHAT_UseContext,
                value => Service.configuration.CHAT_UseContext = value);

            configChanged |= DrawChatTypesSection();
            configChanged |= DrawSourceLanguagesSection();

            return configChanged;
        }

        private static bool DrawChatTypesSection()
        {
            bool configChanged = false;

            ImGui.TextUnformatted("Selected Chat Types:");
            if (ImGui.BeginChild("ChatTypes", new Vector2(0, 150), true))
            {
                var allChatTypes = System.Enum.GetValues<XivChatType>().Where(c => c != XivChatType.None).ToArray();
                foreach (var chatType in allChatTypes)
                {
                    bool isSelected = Service.configuration.CHAT_SelectedChatTypes.Contains(chatType);
                    if (ImGui.Checkbox(chatType.ToString(), ref isSelected))
                    {
                        if (isSelected)
                            Service.configuration.CHAT_SelectedChatTypes.Add(chatType);
                        else
                            Service.configuration.CHAT_SelectedChatTypes.Remove(chatType);
                        configChanged = true;
                    }
                }
            }
            ImGui.EndChild();

            return configChanged;
        }

        private static bool DrawSourceLanguagesSection()
        {
            bool configChanged = false;

            ImGui.TextUnformatted("Source Languages:");
            if (ImGui.BeginChild("SourceLanguages", new Vector2(0, 150), true))
            {
                foreach (var lang in GetLanguages())
                {
                    bool isSelected = Service.configuration.CHAT_SelectedSourceLanguages.Any(l => l.Code == lang.Code);
                    if (ImGui.Checkbox(lang.Name, ref isSelected))
                    {
                        if (isSelected)
                            Service.configuration.CHAT_SelectedSourceLanguages.Add(lang);
                        else
                            Service.configuration.CHAT_SelectedSourceLanguages.RemoveAll(l => l.Code == lang.Code);
                        configChanged = true;
                    }
                }
            }
            ImGui.EndChild();

            return configChanged;
        }
    }

    private static class PFModule
    {
        public static bool Draw()
        {
            ImGui.TextUnformatted("Party Finder Module Configuration");
            ImGui.Separator();

            return DrawCheckbox("Enable Party Finder Module",
                () => Service.configuration.PFModuleEnabled,
                value => Service.configuration.PFModuleEnabled = value);
        }
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
}
