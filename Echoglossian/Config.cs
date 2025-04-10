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
        #endregion

        #region BATTLETALK_MODULE
        #endregion

        #region TOAST_MODULE
        #endregion

        #region JOURNAL_MODULE
        #endregion

        #region TALK_SUBTITLE_MODULE
        #endregion

        #region CHAT_MODULE
        public bool UseContext { get; set; } = false;

        public List<XivChatType> SelectedChatTypes { get; set; } = [];
        public List<LanguageInfo> SelectedSourceLanguages { get; set; } = [];
        #endregion

        public void Save()
        {
            Service.pluginInterface.SavePluginConfig(this);
        }
    }
}
