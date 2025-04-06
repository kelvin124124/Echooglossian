using Dalamud.Configuration;
using Echoglossian.Utils;
using System;
using System.Collections.Generic;
using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian
{
    [Serializable]
    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public string PluginVersion { get; set; } = "0";
        public bool isAssetPresent { get; set; } = false;

        public bool PluginEnabled { get; set; } = true;

        public bool ShowInCutscenes { get; set; } = true;
        public LanguageInfo SelectedPluginLanguage { get; set; } = GetLanguage("en");

        public LanguageInfo SelectedTargetLanguage { get; set; } = GetLanguage("en");
        public Dictionary<string, string> API_Keys { get; set; } = [];

        public List<LLMPreset> LLMPresets { get; set; } = [];
        public LLMPreset SelectedLLMPreset { get; set; } = null!;

        public bool UseContext { get; internal set; }

        public void Save()
        {
            Service.pluginInterface.SavePluginConfig(this);
        }
    }
}
