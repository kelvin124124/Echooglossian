using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Echooglossian.Translate;
using Echooglossian.Utils;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Echooglossian.Utils.LanguageDictionary;

namespace Echooglossian.UI.GameUI
{
    internal partial class UiJournalHandler
    {
        private static readonly Dictionary<string, string> TranslatedQuestNames = [];

        internal static unsafe void OnEvent(AddonEvent type, AddonArgs args)
        {
            if (!Service.configuration.JournalModuleEnabled || args == null)
                return;

            try
            {
                switch (args.AddonName)
                {
                    case "JournalResult":
                        HandleJournalResult((AddonSetupArgs)args);
                        break;
                    case "RecommendList":
                        if (type == AddonEvent.PostReceiveEvent)
                            TranslateRecommendList();
                        else if (type == AddonEvent.PostRequestedUpdate)
                            Task.Delay(200).ContinueWith(_ => TranslateRecommendList());
                        break;
                    case "AreaMap":
                        HandleAreaMap((AddonRefreshArgs)args);
                        break;
                    case "ScenarioTree":
                        HandleScenarioTree((AddonRefreshArgs)args);
                        break;
                    case "Journal":
                        if (type == AddonEvent.PreUpdate)
                            HandleJournalQuest();
                        else if (type == AddonEvent.PostRequestedUpdate)
                            HandleJournalDetail();
                        break;
                    case "JournalDetail":
                        if (type == AddonEvent.PreRequestedUpdate)
                            HandleJournalDetail();
                        break;
                    case "JournalAccept":
                        HandleJournalAccept((AddonSetupArgs)args);
                        break;
                    case "_ToDoList":
                        if (type == AddonEvent.PostRequestedUpdate)
                            HandleToDoList();
                        break;
                }
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"UiJournalHandler error: {e}");
            }
        }

        private static unsafe void TranslateQuestName(string questName, AtkValue* atkValues, int valueIndex)
        {
            if (TranslatedQuestNames.TryGetValue(questName, out string? translatedName))
            {
                atkValues[valueIndex].SetManagedString(translatedName);
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                    var toLang = Service.configuration.SelectedTargetLanguage;

                    Dialogue dialogue = new(nameof(UiJournalHandler), fromLang, toLang, questName);

                    if (!TranslationHandler.DialogueTranslationCache.TryGetValue(dialogue, out string translatedQuestName))
                    {
                        translatedQuestName = Service.translationHandler.TranslateUI(dialogue).GetAwaiter().GetResult();
                        TranslationHandler.DialogueTranslationCache.Add(dialogue, translatedQuestName);
                    }

                    TranslatedQuestNames[questName] = translatedQuestName;

                    var addon = Service.gameGui.GetAddonByName(atkValues[valueIndex].Type.ToString());
                    if (addon == IntPtr.Zero)
                        return;

                    atkValues[valueIndex].SetManagedString(translatedQuestName);
                }
                catch (Exception e)
                {
                    Service.pluginLog.Error($"TranslateQuestName error: {e}");
                }
            });
        }

    }
}
