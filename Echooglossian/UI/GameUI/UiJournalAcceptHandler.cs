using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using Echooglossian.Translate;
using Echooglossian.Utils;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Threading.Tasks;
using static Echooglossian.Utils.LanguageDictionary;


namespace Echooglossian.UI.GameUI
{
    internal partial class UiJournalHandler
    {
        private static unsafe void HandleJournalAccept(AddonSetupArgs args)
        {
            if (args == null || args.AtkValues == IntPtr.Zero)
                return;

            var atkValues = (AtkValue*)args.AtkValues;

            try
            {
                // Translate quest name
                if (atkValues[3].Type == FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String && atkValues[3].String != null)
                {
                    var questName = MemoryHelper.ReadSeStringAsString(out _, (nint)atkValues[3].String.Value);
                    if (!string.IsNullOrEmpty(questName))
                    {
                        TranslateQuestName(questName, atkValues, 3);
                    }
                }

                // Translate quest description
                if (atkValues[4].Type == FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String && atkValues[4].String != null)
                {
                    var descText = MemoryHelper.ReadSeStringAsString(out _, (nint)atkValues[4].String.Value);
                    if (!string.IsNullOrEmpty(descText))
                    {
                        Dialogue dialogue = new(nameof(UiJournalHandler), GetLanguage(Service.clientState.ClientLanguage.ToString()), Service.configuration.SelectedTargetLanguage, descText);

                        if (TranslationHandler.DialogueTranslationCache.TryGetValue(dialogue, out string translatedDesc))
                        {
                            atkValues[4].SetManagedString(translatedDesc);
                        }
                        else
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                                    var toLang = Service.configuration.SelectedTargetLanguage;

                                    string translatedText = Service.translationHandler.TranslateUI(dialogue).GetAwaiter().GetResult();

                                    TranslationHandler.DialogueTranslationCache.Add(dialogue, translatedText);
                                    atkValues[4].SetManagedString(translatedText);
                                }
                                catch (Exception e)
                                {
                                    Service.pluginLog.Error($"HandleJournalAccept desc translation error: {e}");
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"HandleJournalAccept error: {e}");
            }
        }

    }
}
