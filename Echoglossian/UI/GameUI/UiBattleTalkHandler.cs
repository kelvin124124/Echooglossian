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
    internal class UiBattleTalkHandler
    {
        private static string CurrentTranslatedName = string.Empty;
        private static string CurrentTranslatedText = string.Empty;
        private const short OverlayCheckDelay = 100;

        internal static unsafe void OnEvent(AddonEvent type, AddonArgs args)
        {
            if (!Service.config.BattleTalkModuleEnabled)
                return;

            switch (type)
            {
                case AddonEvent.PreReceiveEvent:
                    // Reset translation on new dialogue
                    CurrentTranslatedName = CurrentTranslatedText = string.Empty;
                    return;

                case AddonEvent.PreDraw:
                    if (!Service.config.BATTLETALK_UseImGui || Service.config.BATTLETALK_EnableImGuiTextSwap)
                        ReplaceGameText();
                    return;
            }

            // Handle new dialogue
            try
            {
                var battleTalkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("_BattleTalk");
                if (battleTalkAddon == null || !battleTalkAddon->IsVisible)
                    return;

                var nameNode = battleTalkAddon->GetTextNodeById(4);
                var textNode = battleTalkAddon->GetTextNodeById(6);
                if (textNode == null || textNode->NodeText.IsEmpty)
                    return;

                var name = nameNode != null && !nameNode->NodeText.IsEmpty
                    ? MemoryHelper.ReadSeStringAsString(out _, (nint)nameNode->NodeText.StringPtr.Value)
                    : string.Empty;

                var text = MemoryHelper.ReadSeStringAsString(out _, (nint)textNode->NodeText.StringPtr.Value);
                Service.pluginLog.Debug($"BattleTalk to translate: {name}: {text}");

                if (Service.config.BATTLETALK_UseImGui)
                    TranslateWithImGui(name, text);
                else
                    TranslateBattleTalk(name, text);
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"UiBattleTalkHandler error: {e}");
            }
        }

        private static void TranslateBattleTalk(string name, string content)
        {
            Task.Run(async () =>
            {
                try
                {
                    var fromLang = GetLanguage(Service.clientState.ClientLanguage.ToString());
                    var toLang = Service.config.SelectedTargetLanguage;

                    // Handle content translation
                    var dialogue = new Dialogue(nameof(UiBattleTalkHandler), fromLang, toLang, content);
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
                    Service.pluginLog.Error($"TranslateBattleTalk error: {e}");
                }
            });
        }

        private static unsafe void ReplaceGameText()
        {
            try
            {
                var battleTalkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("_BattleTalk");
                if (battleTalkAddon == null || !battleTalkAddon->IsVisible)
                    return;

                var nameNode = battleTalkAddon->GetTextNodeById(4);
                var textNode = battleTalkAddon->GetTextNodeById(6);
                if (textNode == null || textNode->NodeText.IsEmpty)
                    return;

                if (Service.config.BATTLETALK_TranslateNpcNames && nameNode != null && !nameNode->NodeText.IsEmpty)
                    nameNode->SetText(CurrentTranslatedName);

                var parentNode = battleTalkAddon->GetNodeById(1);
                var nineGridNode = battleTalkAddon->GetNodeById(7);
                textNode->TextFlags = (byte)((byte)TextFlags.WordWrap | (byte)TextFlags.MultiLine | (byte)TextFlags.AutoAdjustNodeSize);
                textNode->FontSize = 14;

                var timerNode = battleTalkAddon->GetNodeById(2);
                textNode->SetWidth(640);
                timerNode->SetXShort((short)(textNode->GetWidth() + 40));

                parentNode->SetWidth((ushort)(textNode->GetWidth() + 128));
                parentNode->SetHeight((ushort)(textNode->GetHeight() + 48));
                nineGridNode->SetWidth((ushort)(textNode->GetWidth() + 128));
                nineGridNode->SetHeight((ushort)(textNode->GetHeight() + 48));

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
            if (Service.config.BATTLETALK_EnableImGuiTextSwap)
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
                    var fromLang = GetLanguage(Service.clientState.ClientLanguage.ToString());
                    var toLang = Service.config.SelectedTargetLanguage;

                    var dialogue = new Dialogue(nameof(UiBattleTalkHandler), fromLang, toLang, text);
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

                    // Update UI overlay and also replace text in-game
                    Service.overlayManager.UpdateBattleTalkOverlay(name, text, nameTranslation, textTranslation);
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
                    var fromLang = GetLanguage(Service.clientState.ClientLanguage.ToString());
                    var toLang = Service.config.SelectedTargetLanguage;

                    var dialogue = new Dialogue(nameof(UiBattleTalkHandler), fromLang, toLang, text);
                    if (!Service.translationCache.TryGet(dialogue, out string textTranslation))
                    {
                        textTranslation = await Service.translationHandler.TranslateUI(dialogue);
                        Service.translationCache.Upsert(dialogue, textTranslation);
                    }

                    string nameTranslation = name;
                    if (Service.config.BATTLETALK_TranslateNpcNames && !string.IsNullOrEmpty(name))
                    {
                        string nameKey = $"name_{fromLang.Code}_{toLang.Code}_{name}";
                        if (!Service.translationCache.TryGetString(nameKey, out nameTranslation))
                        {
                            nameTranslation = await Service.translationHandler.TranslateString(name, toLang);
                            Service.translationCache.UpsertString(nameKey, nameTranslation);
                        }
                    }

                    // Update UI overlay without changing game text
                    Service.overlayManager.UpdateBattleTalkOverlay(name, text, nameTranslation, textTranslation);
                }
                catch (Exception e)
                {
                    Service.pluginLog.Error($"TranslateWithOverlayOnly error: {e}");
                }
            });
        }

        internal static unsafe void ShowBattleTalkOverlay()
        {
            Task.Run(() =>
            {
                var battleTalkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("_BattleTalk");
                if (battleTalkAddon == null)
                    return;

                while (battleTalkAddon->IsVisible)
                {
                    Service.overlayManager.SetBattleTalkOverlayVisible(true);
                    Service.overlayManager.UpdateBattleTalkOverlayPosition(battleTalkAddon);
                    Thread.Sleep(OverlayCheckDelay);
                }

                Service.overlayManager.SetBattleTalkOverlayVisible(false);
            });
        }
    }
}
