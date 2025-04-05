using Echoglossian.Properties;
using Echoglossian.Utils;
using ImGuiNET;
using System.Diagnostics;

namespace Echoglossian.UI.Windows;

public partial class MainWindow
{
    private partial bool DrawEngineSettingsTab()
    {
        bool saveConfig = false;
        bool engineChanged = false;

        saveConfig |= ImGui.Checkbox(Resources.TranslateTextsAgain, ref Service.config.TranslateAlreadyTranslatedTexts);
        ImGui.Separator();

        // Engine Selection (Logic remains the same, removed debug log)
        var currentLanguageInfo = this.languagesDictionary[Service.config.Lang];
        var supportedEngineIndices = currentLanguageInfo.SupportedEngines;
        var supportedEngineNames = supportedEngineIndices.Select(index => this.enginesList[index]).ToArray();
        int currentEngineActualIndex = Service.config.ChosenTransEngine;
        int currentEngineFilteredIndex = Array.IndexOf(supportedEngineIndices, currentEngineActualIndex);
        if (currentEngineFilteredIndex < 0 && supportedEngineIndices.Any()) { currentEngineFilteredIndex = 0; }

        if (ImGui.Combo(Resources.TranslationEngineChoose, ref currentEngineFilteredIndex, supportedEngineNames, supportedEngineNames.Length))
        {
            int selectedActualEngineIndex = supportedEngineIndices[currentEngineFilteredIndex];
            if (Service.config.ChosenTransEngine != selectedActualEngineIndex)
            {
                Service.config.ChosenTransEngine = selectedActualEngineIndex;
                // PluginLog.Debug($"Chosen translation engine: {this.enginesList[Service.config.ChosenTransEngine]} ({Service.config.ChosenTransEngine})"); // Removed
                engineChanged = true; saveConfig = true;
            }
        }
        else if (currentEngineFilteredIndex >= 0 // Ensure index is valid before accessing
            && supportedEngineIndices.Length > currentEngineFilteredIndex // Bounds check
            && supportedEngineIndices.Any()
            && Service.config.ChosenTransEngine != supportedEngineIndices[currentEngineFilteredIndex]) // Use the potentially updated index
        {
            // If the displayed engine doesn't match the config (e.g., after fallback), update config.
            Service.config.ChosenTransEngine = supportedEngineIndices[currentEngineFilteredIndex];
            // PluginLog.Debug($"Defaulted/corrected translation engine to: {this.enginesList[Service.config.ChosenTransEngine]} ({Service.config.ChosenTransEngine})"); // Removed
            engineChanged = true; saveConfig = true;
        }

        ImGui.Separator();
        ImGui.BeginGroup();
        switch (Service.config.ChosenTransEngine)
        {
            case 0: // Google
                ImGui.TextWrapped(Resources.SettingsForGTransText);
                ImGui.TextWrapped(Resources.TranslationEngineSettingsNotRequired);
                break;
            case 1: // DeepL
                ImGui.TextWrapped(Resources.SettingsForDeepLTransText);
                
                if (ImGui.Checkbox(Resources.DeepLTransAPIKey, ref Service.config.DeeplTranslatorUsingApiKey)) { engineChanged = true; saveConfig = true; }
                if (Service.config.DeeplTranslatorUsingApiKey)
                {
                    ImGui.Indent();
                    if (ImGui.Button(Resources.DeepLTranslatorAPIKeyLink)) Process.Start(new ProcessStartInfo { FileName = "https://www.deepl.com/pro-api", UseShellExecute = true });
                    ImGui.SameLine(); ImGui.Text("(opens browser)");
                    
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.7f);
                    if (ImGui.InputText(Resources.DeeplTranslatorApiKey, ref Service.config.DeeplTranslatorApiKey, 100, ImGuiInputTextFlags.Password)) { engineChanged = true; saveConfig = true; }
                    ImGui.Unindent();
                }
                break;
            case 2: // ChatGPT
                ImGui.TextWrapped(Resources.SettingsForChatGptTransText);
                
                if (ImGui.Button(Resources.ChatGPTAPIKeyLink)) Process.Start(new ProcessStartInfo { FileName = "https://platform.openai.com/settings/profile?tab=api-keys", UseShellExecute = true });
                ImGui.SameLine(); ImGui.Text("(opens browser)");
                

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.7f);
                if (ImGui.InputText(Resources.ChatGptApiKey, ref Service.config.ChatGptApiKey, 400, ImGuiInputTextFlags.Password)) { engineChanged = true; saveConfig = true; }
                
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.7f);
                if (ImGui.InputText("API Base URL", ref Service.config.ChatGPTBaseUrl, 400)) { engineChanged = true; saveConfig = true; }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Default: https://api.openai.com/v1\nUse this for compatible local models or proxies.");
                
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.7f);
                if (ImGui.InputText("LLM Model Name", ref Service.config.OpenAILlmModel, 400)) { engineChanged = true; saveConfig = true; }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("e.g., gpt-4, gpt-3.5-turbo, or custom model name.");
                
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
                if (ImGui.SliderFloat("Temperature", ref Service.config.ChatGptTemperature, 0.0f, 1.0f, "%.1f")) { saveConfig = true; }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Lower = deterministic, higher = creative/random.");
                break;
        }
        ImGui.EndGroup();

        if (engineChanged)
        {
            this.translationService = new TranslationService(Service.config, PluginLog, sanitizer);
            // PluginLog.Debug("TranslationService re-created due to engine settings change."); // Removed
        }
        return saveConfig;
    }
}
