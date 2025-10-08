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
    internal class UiBattleTalkHandler
    {
        private static string CurrentTranslatedName = string.Empty;
        private static string CurrentTranslatedText = string.Empty;
        private static string LastOriginalText = string.Empty;
        private const short OverlayCheckDelay = 100;

        internal static unsafe void OnEvent(AddonEvent type, AddonArgs args)
        {
            if (!Service.configuration.BattleTalkModuleEnabled)
                return;

            switch (type)
            {
                case AddonEvent.PreReceiveEvent:
                    // Reset translation on new dialogue
                    CurrentTranslatedName = CurrentTranslatedText = LastOriginalText = string.Empty;
                    ShowBattleTalkOverlay();
                    return;

                case AddonEvent.PreDraw:
                    if (!Service.configuration.BATTLETALK_UseImGui)
                        ReplaceGameText();
                    return;
            }

            try
            {
                var battleTalkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("_BattleTalk").Address;
                if (battleTalkAddon == null || !battleTalkAddon->IsVisible)
                    return;

                var nameNode = battleTalkAddon->GetTextNodeById(4);
                var textNode = battleTalkAddon->GetTextNodeById(6);
                if (textNode == null || textNode->NodeText.IsEmpty)
                    return;

                var text = MemoryHelper.ReadSeStringAsString(out _, (nint)textNode->NodeText.StringPtr.Value);
                if (text == LastOriginalText || string.IsNullOrEmpty(text))
                    return;

                LastOriginalText = text;
                var name = nameNode != null && !nameNode->NodeText.IsEmpty
                    ? MemoryHelper.ReadSeStringAsString(out _, (nint)nameNode->NodeText.StringPtr.Value)
                    : string.Empty;

                Service.pluginLog.Debug($"BattleTalk to translate: {name}: {text}");

                HandleBattleTalkTranslation(name, text);
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"UiBattleTalkHandler error: {e}");
            }
        }

        private static void HandleBattleTalkTranslation(string name, string text)
        {
            Task.Run(async () =>
            {
                try
                {
                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                    var toLang = Service.configuration.SelectedTargetLanguage;

                    // Translate text
                    var dialogue = new Dialogue(nameof(UiBattleTalkHandler), fromLang, toLang, text);
                    if (!TranslationHandler.DialogueTranslationCache.TryGetValue(dialogue, out string translatedText))
                    {
                        translatedText = await Service.translationHandler.TranslateUI(dialogue);
                        TranslationHandler.DialogueTranslationCache.Add(dialogue, translatedText);
                    }

                    // Translate name
                    string translatedName = name;
                    if (Service.configuration.BATTLETALK_TranslateNpcNames && !string.IsNullOrEmpty(name))
                    {
                        translatedName = await Service.translationHandler.TranslateName(name, toLang); // Do not cache names
                    }

                    if (Service.configuration.BATTLETALK_UseImGui)
                    {
                        Service.translationOverlay.UpdateBattleTalkOverlay(name, text, translatedName, translatedText);
                    }
                    else
                    {
                        CurrentTranslatedText = translatedText;
                        CurrentTranslatedName = Service.configuration.BATTLETALK_TranslateNpcNames ? translatedName : name;
                    }
                }
                catch (Exception e)
                {
                    Service.pluginLog.Error($"HandleBattleTalkTranslation error: {e}");
                }
            });
        }

        private static unsafe void ReplaceGameText()
        {
            if (string.IsNullOrEmpty(CurrentTranslatedText))
                return;

            try
            {
                var battleTalkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("_BattleTalk").Address;
                if (battleTalkAddon == null || !battleTalkAddon->IsVisible)
                    return;

                var nameNode = battleTalkAddon->GetTextNodeById(4);
                var textNode = battleTalkAddon->GetTextNodeById(6);
                if (textNode == null || textNode->NodeText.IsEmpty)
                    return;

                if (Service.configuration.BATTLETALK_TranslateNpcNames && nameNode != null && !nameNode->NodeText.IsEmpty)
                    nameNode->SetText(CurrentTranslatedName);

                var parentNode = battleTalkAddon->GetNodeById(1);
                var nineGridNode = battleTalkAddon->GetNodeById(7);

                textNode->TextFlags = TextFlags.WordWrap | TextFlags.MultiLine | TextFlags.AutoAdjustNodeSize;
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

        internal static unsafe void ShowBattleTalkOverlay()
        {
            Task.Run(() =>
            {
                var battleTalkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("_BattleTalk").Address;
                if (battleTalkAddon == null)
                    return;

                while (battleTalkAddon->IsVisible)
                {
                    Service.translationOverlay.SetBattleTalkOverlayVisible(true);
                    Service.translationOverlay.UpdateBattleTalkOverlayPosition(battleTalkAddon);
                    Thread.Sleep(OverlayCheckDelay);
                }

                Service.translationOverlay.SetBattleTalkOverlayVisible(false);
            });
        }
    }
}
