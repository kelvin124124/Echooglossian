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
    internal class UiTalkSubtitleHandler
    {
        private static string CurrentTranslatedText = string.Empty;
        private const short OverlayCheckDelay = 100;

        internal static unsafe void OnEvent(AddonEvent type, AddonArgs args)
        {
            if (!Service.configuration.TalkSubtitleModuleEnabled)
                return;

            AtkValue* atkValues = null;
            string textToTranslate = string.Empty;

            if (args is AddonSetupArgs setupArgs)
                atkValues = (AtkValue*)setupArgs.AtkValues;
            else if (args is AddonRefreshArgs refreshArgs)
                atkValues = (AtkValue*)refreshArgs.AtkValues;

            if (atkValues == null)
                return;

            if (atkValues[0].Type != FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String || atkValues[0].String == null)
                return;

            textToTranslate = MemoryHelper.ReadSeStringAsString(out _, (nint)atkValues[0].String.Value);
            if (string.IsNullOrEmpty(textToTranslate))
                return;

            if (!Service.configuration.SUBTITLE_UseImGui)
                atkValues[0].SetManagedString(string.Empty);

            TranslateTalkSubtitle(textToTranslate);
        }

        private static void TranslateTalkSubtitle(string content)
        {
            Task.Run(async () =>
            {
                try
                {
                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                    var toLang = Service.configuration.SelectedTargetLanguage;

                    var dialogue = new Dialogue(nameof(UiTalkSubtitleHandler), fromLang, toLang, content);
                    if (!Service.translationCache.TryGet(dialogue, out string translatedContent))
                    {
                        translatedContent = await Service.translationHandler.TranslateUI(dialogue);
                        Service.translationCache.Upsert(dialogue, translatedContent);
                    }

                    CurrentTranslatedText = translatedContent;
                    Service.pluginLog.Debug($"Subtitle translated: {content} -> {translatedContent}");

                    if (Service.configuration.SUBTITLE_UseImGui)
                        TranslateWithImGui(content, translatedContent);
                    else
                        ReplaceGameText(translatedContent);
                }
                catch (Exception e)
                {
                    Service.pluginLog.Error($"TranslateTalkSubtitle error: {e}");
                }
            });
        }

        private static unsafe void ReplaceGameText(string translatedText)
        {
            try
            {
                var talkSubtitleAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("TalkSubtitle").Address;
                if (talkSubtitleAddon == null || !talkSubtitleAddon->IsVisible)
                    return;

                var textNode = talkSubtitleAddon->GetTextNodeById(2);
                var textNode3 = talkSubtitleAddon->GetTextNodeById(3);
                var textNode4 = talkSubtitleAddon->GetTextNodeById(4);
                if (textNode == null || textNode3 == null || textNode4 == null ||
                    textNode->NodeText.IsEmpty || textNode3->NodeText.IsEmpty || textNode4->NodeText.IsEmpty)
                    return;

                textNode->SetText(translatedText);
                textNode3->SetText(translatedText);
                textNode4->SetText(translatedText);
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"ReplaceGameText error: {e}");
            }
        }

        private static void TranslateWithImGui(string originalText, string translatedText)
        {
            Service.overlayManager.UpdateTalkSubtitleOverlay(originalText, translatedText);

            ShowTalkSubtitleOverlay();
        }

        internal static unsafe void ShowTalkSubtitleOverlay()
        {
            Task.Run(() =>
            {
                var talkSubtitleAddon = (AtkUnitBase*)Service.gameGui.GetAddonByName("TalkSubtitle").Address;
                if (talkSubtitleAddon == null)
                    return;

                while (talkSubtitleAddon->IsVisible)
                {
                    Service.overlayManager.SetTalkSubtitleOverlayVisible(true);
                    Service.overlayManager.UpdateTalkSubtitleOverlayPosition(talkSubtitleAddon);
                    Thread.Sleep(OverlayCheckDelay);
                }

                Service.overlayManager.SetTalkSubtitleOverlayVisible(false);
            });
        }
    }
}
