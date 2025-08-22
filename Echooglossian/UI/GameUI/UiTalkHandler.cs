using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using Echooglossian.Translate;
using Echooglossian.Utils;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Echooglossian.Utils.LanguageDictionary;

namespace Echooglossian.UI.GameUI
{
    internal class UiTalkHandler
    {
        private static string CurrentTranslatedName = string.Empty;
        private static string CurrentTranslatedText = string.Empty;
        private const short OverlayCheckDelay = 100;

        internal static unsafe void OnEvent(AddonEvent type, AddonArgs args)
        {
            if (!Service.config.TalkModuleEnabled)
                return;

            switch (type)
            {
                case AddonEvent.PreReceiveEvent:
                    var talkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("Talk").Address;
                    if (talkAddon == null || !talkAddon->IsVisible)
                        return;

                    var textNode = talkAddon->GetTextNodeById(3);
                    if (textNode == null || textNode->NodeText.IsEmpty)
                        return;

                    ShowTalkOverlay();  // Show the talk overlay if the addon is visible

                    var textNodeText = MemoryHelper.ReadSeStringAsString(out _, (nint)textNode->NodeText.StringPtr.Value);
                    if (textNodeText == CurrentTranslatedText)
                        CurrentTranslatedName = CurrentTranslatedText = string.Empty;
                    return;

                case AddonEvent.PreDraw:
                    if (!Service.config.TALK_UseImGui)
                        ReplaceGameText();  // Replace the game text with CurrentTranslatedName and CurrentTranslatedText
                    return;
            }

            if (args is not AddonRefreshArgs refreshArgs || (AtkValue*)refreshArgs.AtkValues == null)
                return;

            try
            {
                var atkValues = (AtkValue*)refreshArgs.AtkValues;
                string name = atkValues[1].String != null
                    ? MemoryHelper.ReadSeStringAsString(out _, (nint)atkValues[1].String.Value)
                    : string.Empty;

                string content = MemoryHelper.ReadSeStringAsString(out _, (nint)atkValues[0].String.Value);
                Service.pluginLog.Debug($"Talk to translate: {name}: {content}");

                CurrentTranslatedName = CurrentTranslatedText = string.Empty;

                HandleTalkTranslation(name, content);
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"UiTalkHandler error: {e}");
            }
        }

        private static void HandleTalkTranslation(string name, string text)
        {
            Task.Run(async () =>
            {
                try
                {
                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                    var toLang = Service.config.SelectedTargetLanguage;

                    // Translate text
                    var dialogue = new Dialogue(nameof(UiTalkHandler), fromLang, toLang, text);
                    if (!Service.translationCache.TryGet(dialogue, out string translatedText))
                    {
                        translatedText = await Service.translationHandler.TranslateUI(dialogue);
                        Service.translationCache.Upsert(dialogue, translatedText);
                        Service.pluginLog.Debug($"Translated content: {translatedText}");
                    }

                    // Translate name
                    string translatedName = name; // Default to original name
                    if (Service.config.TALK_TranslateNpcNames && !string.IsNullOrEmpty(name))
                    {
                        string nameKey = $"name_{fromLang.Code}_{toLang.Code}_{name}";
                        if (!Service.translationCache.TryGetString(nameKey, out translatedName))
                        {
                            translatedName = await Service.translationHandler.TranslateString(name, toLang);
                            Service.translationCache.UpsertString(nameKey, translatedName);
                        }
                    }

                    if (Service.config.TALK_UseImGui)
                    {
                        Service.overlayManager.UpdateTalkOverlay(name, text, translatedName, translatedText);
                    }
                    else
                    {
                        CurrentTranslatedText = translatedText;
                        CurrentTranslatedName = Service.config.TALK_TranslateNpcNames ? translatedName : name;
                    }
                }
                catch (Exception e)
                {
                    Service.pluginLog.Error($"HandleTalkTranslation error: {e}");
                }
            });
        }

        private static unsafe void ReplaceGameText()
        {
            try
            {
                var talkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("Talk").Address;
                if (talkAddon == null || !talkAddon->IsVisible)
                    return;

                var textNode = talkAddon->GetTextNodeById(3);
                if (textNode == null || textNode->NodeText.IsEmpty)
                    return;

                var nameNode = talkAddon->GetTextNodeById(2);
                if (Service.config.TALK_TranslateNpcNames && nameNode != null && !nameNode->NodeText.IsEmpty)
                    nameNode->SetText(CurrentTranslatedName);

                var parentNode = talkAddon->GetNodeById(10);

                textNode->TextFlags = TextFlags.WordWrap | TextFlags.MultiLine | TextFlags.AutoAdjustNodeSize;

                textNode->FontSize = (byte)(CurrentTranslatedText.Length >= 350 ? 11 :
                                           (CurrentTranslatedText.Length >= 256 ? 12 : 14));
                textNode->SetWidth(parentNode->GetWidth());
                textNode->SetText(CurrentTranslatedText);
                textNode->ResizeNodeForCurrentText();
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"ReplaceGameText error: {e}");
            }
        }

        internal static unsafe void ShowTalkOverlay()
        {
            Task.Run(() =>
            {
                var talkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("Talk").Address;
                if (talkAddon == null)
                    return;

                while (talkAddon->IsVisible)
                {
                    Service.overlayManager.SetTalkOverlayVisible(true);
                    Service.overlayManager.UpdateTalkOverlayPosition(talkAddon);
                    Thread.Sleep(OverlayCheckDelay);
                }

                Service.overlayManager.SetTalkOverlayVisible(false);
            });
        }
    }
}
