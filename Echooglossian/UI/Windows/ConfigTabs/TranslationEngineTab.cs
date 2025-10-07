using Dalamud.Bindings.ImGui;
using Echooglossian.Utils;
using System;
using System.Linq;
using System.Numerics;

namespace Echooglossian.UI.Windows.ConfigTabs;

public static class TranslationEngineTab
{
    private static readonly string[] ApiServices = ["OpenAI", "Google", "DeepL", "Microsoft", "Yandex", "OpenRouter", "DeepSeek"];

    // State for custom preset creation/editing
    private static bool showCreatePresetPopup = false;
    private static bool showEditPresetPopup = false;
    private static LLMPreset? presetBeingEdited = null;
    private static string newPresetName = "";
    private static string newPresetEndpoint = "";
    private static string newPresetModel = "";
    private static float newPresetTemperature = 1.0f;

    public static bool Draw()
    {
        bool configChanged = false;

        configChanged |= DrawCheckbox("Use LLM Translation",
            () => Service.configuration.UseLLMTranslation,
            value => Service.configuration.UseLLMTranslation = value);

        ImGui.Separator();

        // API Keys section
        configChanged |= DrawApiKeysSection();

        ImGui.Separator();

        // LLM Presets section
        if (Service.configuration.UseLLMTranslation)
        {
            configChanged |= DrawLLMPresetsSection();
        }

        // Handle popups
        DrawCreatePresetPopup(ref configChanged);
        DrawEditPresetPopup(ref configChanged);

        return configChanged;
    }

    private static bool DrawApiKeysSection()
    {
        bool configChanged = false;

        ImGui.TextUnformatted("API Keys:");

        foreach (var service in ApiServices)
        {
            string currentKey = Service.configuration.API_Keys.TryGetValue(service, out var key) ? key : "";
            if (ImGui.InputText($"{service} API Key", ref currentKey, 256, ImGuiInputTextFlags.Password))
            {
                if (string.IsNullOrWhiteSpace(currentKey))
                    Service.configuration.API_Keys.Remove(service);
                else
                    Service.configuration.API_Keys[service] = currentKey;
                configChanged = true;
            }
        }

        return configChanged;
    }

    private static bool DrawLLMPresetsSection()
    {
        bool configChanged = false;

        ImGui.TextUnformatted("LLM Presets:");

        // Selected preset dropdown
        var selectedPreset = Service.configuration.SelectedLLMPreset;
        string presetName = selectedPreset?.Name ?? "None";
        if (ImGui.BeginCombo("Selected Preset", presetName))
        {
            foreach (var preset in Service.configuration.LLMPresets)
            {
                bool isSelected = preset == selectedPreset;
                if (ImGui.Selectable(preset.Name, isSelected))
                {
                    Service.configuration.SelectedLLMPreset = preset;
                    configChanged = true;
                }
                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        // Preset management buttons
        ImGui.Spacing();

        if (ImGui.Button("Create New Preset"))
        {
            ResetPresetFields();
            showCreatePresetPopup = true;
        }

        ImGui.SameLine();
        if (ImGui.Button("Edit Selected") && selectedPreset != null)
        {
            LoadPresetForEditing(selectedPreset);
            showEditPresetPopup = true;
        }

        ImGui.SameLine();
        if (ImGui.Button("Delete Selected") && selectedPreset != null && !IsDefaultPreset(selectedPreset))
        {
            Service.configuration.LLMPresets.Remove(selectedPreset);
            Service.configuration.SelectedLLMPreset = Service.configuration.LLMPresets.FirstOrDefault();
            configChanged = true;
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset to Defaults"))
        {
            Service.configuration.LLMPresets.Clear();
            Service.configuration.LLMPresets.AddRange(DefaultLLMPresets.Default);
            Service.configuration.SelectedLLMPreset = Service.configuration.LLMPresets.FirstOrDefault();
            configChanged = true;
        }

        return configChanged;
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

    private static void ResetPresetFields()
    {
        newPresetName = "";
        newPresetEndpoint = "";
        newPresetModel = "";
        newPresetTemperature = 1.0f;
        presetBeingEdited = null;
    }

    private static void LoadPresetForEditing(LLMPreset preset)
    {
        presetBeingEdited = preset;
        newPresetName = preset.Name;
        newPresetEndpoint = preset.LLM_API_Endpoint;
        newPresetModel = preset.Model;
        newPresetTemperature = preset.Temperature;
    }

    private static bool IsDefaultPreset(LLMPreset preset)
    {
        return DefaultLLMPresets.Default.Any(p => p.Name == preset.Name &&
                                                  p.LLM_API_Endpoint == preset.LLM_API_Endpoint &&
                                                  p.Model == preset.Model);
    }

    private static void DrawCreatePresetPopup(ref bool configChanged)
    {
        if (!showCreatePresetPopup) return;

        ImGui.OpenPopup("Create New LLM Preset");

        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(400, 250));

        if (ImGui.BeginPopupModal("Create New LLM Preset", ref showCreatePresetPopup, ImGuiWindowFlags.NoResize))
        {
            ImGui.TextUnformatted("Create a new LLM preset:");
            ImGui.Spacing();

            ImGui.TextUnformatted("Preset Name:");
            ImGui.InputText("##PresetName", ref newPresetName, 100);

            ImGui.TextUnformatted("API Endpoint:");
            ImGui.InputText("##PresetEndpoint", ref newPresetEndpoint, 256);

            ImGui.TextUnformatted("Model:");
            ImGui.InputText("##PresetModel", ref newPresetModel, 100);

            ImGui.TextUnformatted("Temperature:");
            ImGui.SliderFloat("##PresetTemperature", ref newPresetTemperature, 0.0f, 2.0f, "%.2f");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            bool canCreate = !string.IsNullOrWhiteSpace(newPresetName) &&
                           !string.IsNullOrWhiteSpace(newPresetEndpoint) &&
                           !string.IsNullOrWhiteSpace(newPresetModel);

            if (!canCreate)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("Create Preset"))
            {
                var newPreset = new LLMPreset(newPresetName, newPresetEndpoint, newPresetModel, newPresetTemperature);
                Service.configuration.LLMPresets.Add(newPreset);
                Service.configuration.SelectedLLMPreset = newPreset;
                configChanged = true;
                showCreatePresetPopup = false;
                ResetPresetFields();
            }

            if (!canCreate)
            {
                ImGui.EndDisabled();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                showCreatePresetPopup = false;
                ResetPresetFields();
            }

            ImGui.EndPopup();
        }
    }

    private static void DrawEditPresetPopup(ref bool configChanged)
    {
        if (!showEditPresetPopup || presetBeingEdited == null) return;

        ImGui.OpenPopup("Edit LLM Preset");

        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(400, 250));

        if (ImGui.BeginPopupModal("Edit LLM Preset", ref showEditPresetPopup, ImGuiWindowFlags.NoResize))
        {
            ImGui.TextUnformatted($"Editing preset: {presetBeingEdited.Name}");
            ImGui.Spacing();

            bool isDefault = IsDefaultPreset(presetBeingEdited);
            if (isDefault)
            {
                ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.0f, 1.0f), "Note: This is a default preset. Changes will create a copy.");
                ImGui.Spacing();
            }

            ImGui.TextUnformatted("Preset Name:");
            ImGui.InputText("##PresetName", ref newPresetName, 100);

            ImGui.TextUnformatted("API Endpoint:");
            ImGui.InputText("##PresetEndpoint", ref newPresetEndpoint, 256);

            ImGui.TextUnformatted("Model:");
            ImGui.InputText("##PresetModel", ref newPresetModel, 100);

            ImGui.TextUnformatted("Temperature:");
            ImGui.SliderFloat("##PresetTemperature", ref newPresetTemperature, 0.0f, 2.0f, "%.2f");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            bool canSave = !string.IsNullOrWhiteSpace(newPresetName) &&
                          !string.IsNullOrWhiteSpace(newPresetEndpoint) &&
                          !string.IsNullOrWhiteSpace(newPresetModel);

            if (!canSave)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("Save Changes"))
            {
                if (isDefault)
                {
                    // Create new preset for default presets
                    var newPreset = new LLMPreset(newPresetName, newPresetEndpoint, newPresetModel, newPresetTemperature);
                    Service.configuration.LLMPresets.Add(newPreset);
                    Service.configuration.SelectedLLMPreset = newPreset;
                }
                else
                {
                    // Update existing custom preset
                    presetBeingEdited.Name = newPresetName;
                    presetBeingEdited.LLM_API_Endpoint = newPresetEndpoint;
                    presetBeingEdited.Model = newPresetModel;
                    presetBeingEdited.Temperature = newPresetTemperature;
                }

                configChanged = true;
                showEditPresetPopup = false;
                ResetPresetFields();
            }

            if (!canSave)
            {
                ImGui.EndDisabled();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                showEditPresetPopup = false;
                ResetPresetFields();
            }

            ImGui.EndPopup();
        }
    }
}
