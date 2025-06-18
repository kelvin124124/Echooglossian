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
        private static string LastOriginalText = string.Empty;
        private const short OverlayCheckDelay = 100;

        internal static unsafe void OnEvent(AddonEvent type, AddonArgs args)
        {
            if (!Service.config.BattleTalkModuleEnabled)
                return;

            switch (type)
            {
                case AddonEvent.PreReceiveEvent:
                    // Reset translation on new dialogue
                    CurrentTranslatedName = CurrentTranslatedText = LastOriginalText = string.Empty;
                    ShowBattleTalkOverlay();
                    return;

                case AddonEvent.PreDraw:
                    if (!Service.config.BATTLETALK_UseImGui)
                        ReplaceGameText();
                    return;
            }

            try
            {
                var battleTalkAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("_BattleTalk");
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
                    var toLang = Service.config.SelectedTargetLanguage;

                    // Translate text
                    var dialogue = new Dialogue(nameof(UiBattleTalkHandler), fromLang, toLang, text);
                    if (!Service.translationCache.TryGet(dialogue, out string translatedText))
                    {
                        translatedText = await Service.translationHandler.TranslateUI(dialogue);
                        Service.translationCache.Upsert(dialogue, translatedText);
                    }

                    // Translate name
                    string translatedName = name;
                    if (Service.config.BATTLETALK_TranslateNpcNames && !string.IsNullOrEmpty(name))
                    {
                        string nameKey = $"name_{fromLang.Code}_{toLang.Code}_{name}";
                        if (!Service.translationCache.TryGetString(nameKey, out translatedName))
                        {
                            translatedName = await Service.translationHandler.TranslateString(name, toLang);
                            Service.translationCache.UpsertString(nameKey, translatedName);
                        }
                    }

                    if (Service.config.BATTLETALK_UseImGui)
                    {
                        Service.overlayManager.UpdateBattleTalkOverlay(name, text, translatedName, translatedText);
                    }
                    else
                    {
                        CurrentTranslatedText = translatedText;
                        CurrentTranslatedName = Service.config.BATTLETALK_TranslateNpcNames ? translatedName : name;
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
