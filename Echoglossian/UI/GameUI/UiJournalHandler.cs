using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Echoglossian.Utils;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian.UI.GameUI
{
    internal partial class UiJournalHandler
    {
        private static readonly Dictionary<string, string> TranslatedQuestNames = new();

        internal static unsafe void OnEvent(AddonEvent type, AddonArgs args)
        {
            if (!Service.config.JournalModuleEnabled || args == null)
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
            if (TranslatedQuestNames.TryGetValue(questName, out string translatedName))
            {
                atkValues[valueIndex].SetManagedString(translatedName);
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    var fromLang = GetLanguage(Service.clientState.ClientLanguage.ToString());
                    var toLang = Service.config.SelectedTargetLanguage;

                    string questNameKey = $"quest_{fromLang.Code}_{toLang.Code}_{questName}";
                    string cachedTranslation;

                    if (!Service.translationCache.TryGetString(questNameKey, out cachedTranslation))
                    {
                        cachedTranslation = Service.translationHandler.TranslateString(questName, toLang).GetAwaiter().GetResult();
                        Service.translationCache.UpsertString(questNameKey, cachedTranslation);
                    }

                    TranslatedQuestNames[questName] = cachedTranslation;

                    var addon = Service.gameGui.GetAddonByName(atkValues[valueIndex].Type.ToString());
                    if (addon == IntPtr.Zero)
                        return;

                    atkValues[valueIndex].SetManagedString(cachedTranslation);
                }
                catch (Exception e)
                {
                    Service.pluginLog.Error($"TranslateQuestName error: {e}");
                }
            });
        }

    }
}
