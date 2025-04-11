using Dalamud.Configuration;
using Dalamud.Game.Text;
using Echoglossian.Utils;
using System;
using System.Collections.Generic;
using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian
{
    [Serializable]
    public class Config : IPluginConfiguration
    {
        #region INTERNAL
        public int Version { get; set; } = 0;
        public string PluginVersion { get; set; } = "0";
        public bool isAssetPresent { get; set; } = false;
        #endregion

        #region MODULE_TOGGLES
        public bool PluginEnabled { get; set; } = true;
        public bool TalkModuleEnabled { get; set; } = true;
        public bool BattleTalkModuleEnabled { get; set; } = true;
        public bool ToastModuleEnabled { get; set; } = true;
        public bool JournalModuleEnabled { get; set; } = true;
        public bool TalkSubtitleModuleEnabled { get; set; } = true;
        public bool ChatModuleEbanled { get; set; } = true;
        public bool PFModuleEnabled { get; set; } = true;
        #endregion

        #region GENERAL
        public bool ShowInCutscenes { get; set; } = true;
        public LanguageInfo SelectedPluginLanguage { get; set; } = GetLanguage("en");

        public LanguageInfo SelectedTargetLanguage { get; set; } = GetLanguage("en");
        public Dictionary<string, string> API_Keys { get; set; } = [];

        public List<LLMPreset> LLMPresets { get; set; } = [];
        public LLMPreset SelectedLLMPreset { get; set; } = null!;
        #endregion

        #region TALK_MODULE
        public bool TALK_UseImGui { get; set; } = true;
        public bool TALK_EnableImGuiTextSwap { get; set; } = true;
        public bool TALK_TranslateNpcNames { get; set; } = true;
        #endregion

        #region BATTLETALK_MODULE
        public bool BATTLETALK_UseImGui { get; set; } = true;
        public bool BATTLETALK_EnableImGuiTextSwap { get; set; } = true;
        public bool BATTLETALK_TranslateNpcNames { get; set; } = true;
        #endregion

        #region TOAST_MODULE
        #endregion

        #region JOURNAL_MODULE
        #endregion

        #region TALK_SUBTITLE_MODULE
        public bool SUBTITLE_UseImGui { get; set; } = true;
        public bool SUBTITLE_EnableImGuiTextSwap { get; set; } = true;
        #endregion

        #region CHAT_MODULE
        public bool CHAT_UseContext { get; set; } = false;

        public List<XivChatType> CHAT_SelectedChatTypes { get; set; } = [];
        public List<LanguageInfo> CHAT_SelectedSourceLanguages { get; set; } = [];
        #endregion

        #region PF_MODULE
        public bool PF_UseContext { get; set; } = true;
        #endregion

        public void Save()
        {
            Service.pluginInterface.SavePluginConfig(this);
        }
    }
}
