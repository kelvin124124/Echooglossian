using Dalamud.Memory;
using Echoglossian.Utils;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Threading.Tasks;
using static Echoglossian.Utils.LanguageDictionary;


namespace Echoglossian.UI.GameUI
{
    internal partial class UiJournalHandler
    {
        private static unsafe void HandleJournalQuest()
        {
            var journal = (AtkUnitBase*)Service.gameGui.GetAddonByName("Journal");
            if (journal == null || !journal->IsVisible)
                return;

            try
            {
                // Handle quest list entries
                var questListNode = journal->GetNodeById(11);
                if (questListNode == null || !questListNode->IsVisible())
                    return;

                var questListComponent = questListNode->GetAsAtkComponentNode()->Component;
                for (var i = 0; i < questListComponent->UldManager.NodeListCount; i++)
                {
                    if (!questListComponent->UldManager.NodeList[i]->IsVisible())
                        continue;

                    var questItemNode = questListComponent->UldManager.NodeList[i]->GetAsAtkComponentNode();
                    var questNameNode = questItemNode->Component->UldManager.SearchNodeById(3);
                    if (questNameNode == null || !questNameNode->IsVisible() || questNameNode->Type != NodeType.Text)
                        continue;

                    var questName = questNameNode->GetAsAtkTextNode();
                    if (questName->NodeText.IsEmpty)
                        continue;

                    var questNameText = MemoryHelper.ReadSeStringAsString(out _, (nint)questName->NodeText.StringPtr.Value);
                    if (string.IsNullOrEmpty(questNameText))
                        continue;

                    if (TranslatedQuestNames.TryGetValue(questNameText, out string translatedName))
                    {
                        questName->NodeText.SetString(translatedName);
                    }
                    else
                    {
                        string capturedText = questNameText;
                        AtkTextNode* capturedNode = questName;

                        Task.Run(() =>
                        {
                            try
                            {
                                var fromLang = GetLanguage(Service.clientState.ClientLanguage.ToString());
                                var toLang = Service.config.SelectedTargetLanguage;

                                string questNameKey = $"quest_{fromLang.Code}_{toLang.Code}_{capturedText}";
                                string cachedTranslation;

                                if (!Service.translationCache.TryGetString(questNameKey, out cachedTranslation))
                                {
                                    cachedTranslation = Service.translationHandler.TranslateString(capturedText, toLang)
                                        .GetAwaiter().GetResult();
                                    Service.translationCache.UpsertString(questNameKey, cachedTranslation);
                                }

                                TranslatedQuestNames[capturedText] = cachedTranslation;
                                capturedNode->NodeText.SetString(cachedTranslation);
                            }
                            catch (Exception e)
                            {
                                Service.pluginLog.Error($"HandleJournalQuest translation error: {e}");
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"HandleJournalQuest error: {e}");
            }
        }

    }
}
