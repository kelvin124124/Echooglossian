using Dalamud.Configuration;
using Echoglossian.Utils;
using System.Collections.Generic;
using System.Globalization;

namespace Echoglossian;

[Serializable]
public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public string PluginVersion { get; set; } = "0";
    public bool isAssetPresent { get; set; } = false;

    public bool ShowInCutscenes { get; set; } = true;
    public CultureInfo SelectedPluginLanguage { get; set; } = new CultureInfo("English");

    public CultureInfo SelectedTargetLanguage { get; set; } = new CultureInfo("English");
    public Dictionary<string, string> API_Keys { get; set; } = [];

    public List<LLMPreset> LLMPresets { get; set; } = [];
    public LLMPreset SelectedLLMPreset { get; set; } = null!;

    public bool UseContext { get; internal set; }

    public void Save()
    {
        Service.pluginInterface.SavePluginConfig(this);
    }
}
