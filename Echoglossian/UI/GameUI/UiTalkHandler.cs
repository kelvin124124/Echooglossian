using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using Echoglossian.Translate;
using Echoglossian.Utils;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian.UI.GameUI
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
                    var talkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("Talk");
                    if (talkAddon == null || !talkAddon->IsVisible)
                        return;

                    var textNode = talkAddon->GetTextNodeById(3);
                    if (textNode == null || textNode->NodeText.IsEmpty)
                        return;

                    var textNodeText = MemoryHelper.ReadSeStringAsString(out _, (nint)textNode->NodeText.StringPtr.Value);
                    if (textNodeText == CurrentTranslatedText)
                        CurrentTranslatedName = CurrentTranslatedText = string.Empty;
                    return;

                case AddonEvent.PreDraw:
                    if (!Service.config.TALK_UseImGui || Service.config.TALK_EnableImGuiTextSwap)
                        ReplaceGameText();
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

                if (Service.config.TALK_UseImGui)
                    TranslateWithImGui(name, content);
                else
                    TranslateTalk(name, content);
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"UiTalkHandler error: {e}");
            }
        }

        private static void TranslateTalk(string name, string content)
        {
            Task.Run(async () =>
            {
                try
                {
                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                    var toLang = Service.config.SelectedTargetLanguage;

                    // Handle content translation
                    var dialogue = new Dialogue(nameof(UiTalkHandler), fromLang, toLang, content);
                    if (!Service.translationCache.TryGet(dialogue, out string translatedContent))
                    {
                        translatedContent = await Service.translationHandler.TranslateUI(dialogue);
                        Service.translationCache.Upsert(dialogue, translatedContent);
                    }
                    CurrentTranslatedText = translatedContent;

                    // Handle name translation if needed
                    if (string.IsNullOrEmpty(name))
                        return;

                    string nameKey = $"name_{fromLang.Code}_{toLang.Code}_{name}";
                    if (Service.translationCache.TryGetString(nameKey, out string cachedName))
                        CurrentTranslatedName = cachedName;
                    else
                    {
                        string translatedName = await Service.translationHandler.TranslateString(name, toLang);
                        Service.translationCache.UpsertString(nameKey, translatedName);
                        CurrentTranslatedName = translatedName;
                    }
                }
                catch (Exception e)
                {
                    Service.pluginLog.Error($"TranslateTalk error: {e}");
                }
            });
        }

        private static unsafe void ReplaceGameText()
        {
            try
            {
                var talkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("Talk");
                if (talkAddon == null || !talkAddon->IsVisible)
                    return;

                var textNode = talkAddon->GetTextNodeById(3);
                if (textNode == null || textNode->NodeText.IsEmpty)
                    return;

                var nameNode = talkAddon->GetTextNodeById(2);
                if (Service.config.TALK_TranslateNpcNames && nameNode != null && !nameNode->NodeText.IsEmpty)
                    nameNode->SetText(CurrentTranslatedName);

                var parentNode = talkAddon->GetNodeById(10);
                textNode->TextFlags = (byte)((byte)TextFlags.WordWrap | (byte)TextFlags.MultiLine | (byte)TextFlags.AutoAdjustNodeSize);

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

        private static void TranslateWithImGui(string name, string text)
        {
            if (Service.config.TALK_EnableImGuiTextSwap)
            {
                TranslateWithOverlayAndSwap(name, text);
                return;
            }
            TranslateWithOverlayOnly(name, text);
        }

        private static void TranslateWithOverlayAndSwap(string name, string text)
        {
            Task.Run(async () =>
            {
                try
                {
                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                    var toLang = Service.config.SelectedTargetLanguage;

                    var dialogue = new Dialogue(nameof(UiTalkHandler), fromLang, toLang, text);
                    bool needTextTranslation = !Service.translationCache.TryGet(dialogue, out string textTranslation);

                    string nameTranslation = string.Empty;
                    bool needNameTranslation = !string.IsNullOrEmpty(name);
                    if (needNameTranslation)
                    {
                        string nameKey = $"name_{fromLang.Code}_{toLang.Code}_{name}";
                        if (Service.translationCache.TryGetString(nameKey, out nameTranslation))
                            needNameTranslation = false;
                    }

                    if (needTextTranslation)
                    {
                        textTranslation = await Service.translationHandler.TranslateUI(dialogue);
                        Service.translationCache.Upsert(dialogue, textTranslation);
                    }

                    if (needNameTranslation)
                    {
                        nameTranslation = await Service.translationHandler.TranslateString(name, toLang);
                        string nameKey = $"name_{fromLang.Code}_{toLang.Code}_{name}";
                        Service.translationCache.UpsertString(nameKey, nameTranslation);
                    }

                    CurrentTranslatedText = textTranslation;
                    CurrentTranslatedName = nameTranslation;

                    // Update UI overlay
                    Service.overlayManager.UpdateTalkOverlay(name, text, nameTranslation, textTranslation);
                }
                catch (Exception e)
                {
                    Service.pluginLog.Error($"TranslateWithOverlayAndSwap error: {e}");
                }
            });
        }

        private static void TranslateWithOverlayOnly(string name, string text)
        {
            Task.Run(async () =>
            {
                try
                {
                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                    var toLang = Service.config.SelectedTargetLanguage;

                    var dialogue = new Dialogue(nameof(UiTalkHandler), fromLang, toLang, text);
                    if (!Service.translationCache.TryGet(dialogue, out string textTranslation))
                    {
                        textTranslation = await Service.translationHandler.TranslateUI(dialogue);
                        Service.translationCache.Upsert(dialogue, textTranslation);
                    }

                    string nameTranslation = name;
                    if (Service.config.TALK_TranslateNpcNames && !string.IsNullOrEmpty(name))
                    {
                        string nameKey = $"name_{fromLang.Code}_{toLang.Code}_{name}";
                        if (!Service.translationCache.TryGetString(nameKey, out nameTranslation))
                        {
                            nameTranslation = await Service.translationHandler.TranslateString(name, toLang);
                            Service.translationCache.UpsertString(nameKey, nameTranslation);
                        }
                    }

                    // Update UI overlay without changing game text
                    Service.overlayManager.UpdateTalkOverlay(name, text, nameTranslation, textTranslation);
                }
                catch (Exception e)
                {
                    Service.pluginLog.Error($"TranslateWithOverlayOnly error: {e}");
                }
            });
        }

        internal static unsafe void ShowTalkOverlay()
        {
            Task.Run(() =>
            {
                var talkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("Talk");
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
