using Dalamud.Configuration;
using Dalamud.Game.Text;
using Echoglossian.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian
{
    [Serializable]
    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        public string PluginVersion { get; set; } = "0";

        public bool AssetPresent { get; set; } = false;
        public bool FirstTimeSetupComplete { get; set; } = false;

        #region General Settings
        public bool PluginEnabled { get; set; } = true;

        public LanguageInfo SelectedPluginLanguage { get; set; } = LanguageDictionary.GetLanguage("en");
        public LanguageInfo SelectedTargetLanguage { get; set; } = LanguageDictionary.GetLanguage("en");

        public float FontSize { get; set; } = 18.0f;
        public float FontScale { get; set; } = 1.0f;
        public bool ShowInCutscenes { get; set; } = true;

        public Dictionary<string, string> API_Keys { get; set; } = [];

        public List<LLMPreset> LLMPresets { get; set; } = [];
        public LLMPreset SelectedLLMPreset { get; set; } = null!;
        #endregion

        #region Module Toggles
        public bool TalkModuleEnabled { get; set; } = true;
        public bool BattleTalkModuleEnabled { get; set; } = true;
        public bool TalkSubtitleModuleEnabled { get; set; } = true;
        public bool JournalModuleEnabled { get; set; } = true;
        public bool ToastModuleEnabled { get; set; } = true;
        public bool ChatModuleEnabled { get; set; } = true;
        public bool PFModuleEnabled { get; set; } = true;
        #endregion

        #region UI Settings
        // Color settings
        public Vector4 OverlayTalkTextColor { get; set; } = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        public Vector4 OverlayBattleTalkTextColor { get; set; } = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        // Position correction
        public Vector2 ImGuiWindowPosCorrection { get; set; } = Vector2.Zero;
        public Vector2 ImGuiToastWindowPosCorrection { get; set; } = Vector2.Zero;

        // Window size multipliers
        public float ImGuiTalkWindowWidthMult { get; set; } = 1.5f;
        public float ImGuiTalkWindowHeightMult { get; set; } = 2.0f;
        public float ImGuiBattleTalkWindowWidthMult { get; set; } = 1.5f;
        public float ImGuiBattleTalkWindowHeightMult { get; set; } = 2.0f;
        public float ImGuiTalkSubtitleWindowWidthMult { get; set; } = 1.5f;
        public float ImGuiTalkSubtitleWindowHeightMult { get; set; } = 2.0f;
        public float ImGuiToastWindowWidthMult { get; set; } = 1.5f;
        #endregion

        #region Talk Settings
        public bool TALK_UseImGui { get; set; } = true;
        public bool TALK_TranslateNpcNames { get; set; } = true;
        #endregion

        #region BattleTalk Settings
        public bool BATTLETALK_UseImGui { get; set; } = true;
        public bool BATTLETALK_TranslateNpcNames { get; set; } = true;
        #endregion

        #region Subtitle Settings
        public bool SUBTITLE_UseImGui { get; set; } = true;
        #endregion

        #region Toast Settings
        public bool TOAST_UseImGui { get; set; } = true;
        public bool TOAST_TranslateRegular { get; set; } = true;
        public bool TOAST_TranslateError { get; set; } = true;
        public bool TOAST_TranslateQuest { get; set; } = true;
        #endregion

        #region CHAT_MODULE
        public bool CHAT_UseContext { get; set; } = true;

        public List<XivChatType> CHAT_SelectedChatTypes { get; set; } = [];
        public List<LanguageInfo> CHAT_SelectedSourceLanguages { get; set; } = [];
        #endregion

        #region PF_MODULE
        #endregion

        // Save configuration
        public void Save()
        {
            Service.pluginInterface.SavePluginConfig(this);
        }
    }
}
