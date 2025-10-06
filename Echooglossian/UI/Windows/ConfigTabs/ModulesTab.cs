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
                () => Service.config.TalkModuleEnabled, 
                value => Service.config.TalkModuleEnabled = value);

            configChanged |= DrawCheckbox("Use ImGui Overlay", 
                () => Service.config.TALK_UseImGui, 
                value => Service.config.TALK_UseImGui = value);

            configChanged |= DrawCheckbox("Translate NPC Names", 
                () => Service.config.TALK_TranslateNpcNames, 
                value => Service.config.TALK_TranslateNpcNames = value);

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
                () => Service.config.BattleTalkModuleEnabled, 
                value => Service.config.BattleTalkModuleEnabled = value);

            configChanged |= DrawCheckbox("Use ImGui Overlay", 
                () => Service.config.BATTLETALK_UseImGui, 
                value => Service.config.BATTLETALK_UseImGui = value);

            configChanged |= DrawCheckbox("Translate NPC Names", 
                () => Service.config.BATTLETALK_TranslateNpcNames, 
                value => Service.config.BATTLETALK_TranslateNpcNames = value);

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
                () => Service.config.TalkSubtitleModuleEnabled, 
                value => Service.config.TalkSubtitleModuleEnabled = value);

            configChanged |= DrawCheckbox("Use ImGui Overlay", 
                () => Service.config.SUBTITLE_UseImGui, 
                value => Service.config.SUBTITLE_UseImGui = value);

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
                () => Service.config.JournalModuleEnabled, 
                value => Service.config.JournalModuleEnabled = value);
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
                () => Service.config.ToastModuleEnabled, 
                value => Service.config.ToastModuleEnabled = value);

            configChanged |= DrawCheckbox("Use ImGui Overlay", 
                () => Service.config.TOAST_UseImGui, 
                value => Service.config.TOAST_UseImGui = value);

            configChanged |= DrawCheckbox("Translate Regular Toasts", 
                () => Service.config.TOAST_TranslateRegular, 
                value => Service.config.TOAST_TranslateRegular = value);

            configChanged |= DrawCheckbox("Translate Error Toasts", 
                () => Service.config.TOAST_TranslateError, 
                value => Service.config.TOAST_TranslateError = value);

            configChanged |= DrawCheckbox("Translate Quest Toasts", 
                () => Service.config.TOAST_TranslateQuest, 
                value => Service.config.TOAST_TranslateQuest = value);

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
                () => Service.config.ChatModuleEnabled, 
                value => Service.config.ChatModuleEnabled = value);

            configChanged |= DrawCheckbox("Use Context for Translation", 
                () => Service.config.CHAT_UseContext, 
                value => Service.config.CHAT_UseContext = value);

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
                    bool isSelected = Service.config.CHAT_SelectedChatTypes.Contains(chatType);
                    if (ImGui.Checkbox(chatType.ToString(), ref isSelected))
                    {
                        if (isSelected)
                            Service.config.CHAT_SelectedChatTypes.Add(chatType);
                        else
                            Service.config.CHAT_SelectedChatTypes.Remove(chatType);
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
                    bool isSelected = Service.config.CHAT_SelectedSourceLanguages.Any(l => l.Code == lang.Code);
                    if (ImGui.Checkbox(lang.Name, ref isSelected))
                    {
                        if (isSelected)
                            Service.config.CHAT_SelectedSourceLanguages.Add(lang);
                        else
                            Service.config.CHAT_SelectedSourceLanguages.RemoveAll(l => l.Code == lang.Code);
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
                () => Service.config.PFModuleEnabled, 
                value => Service.config.PFModuleEnabled = value);
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