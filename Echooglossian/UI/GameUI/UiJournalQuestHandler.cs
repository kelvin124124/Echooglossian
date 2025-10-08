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
        private static unsafe void HandleJournalQuest()
        {
            var journal = (AtkUnitBase*)Service.gameGui.GetAddonByName("Journal").Address;
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
                                var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                                var toLang = Service.configuration.SelectedTargetLanguage;

                                Dialogue dialogue = new(nameof(UiJournalHandler), fromLang, toLang, capturedText);

                                if (!TranslationHandler.DialogueTranslationCache.TryGetValue(dialogue, out string translatedQuestName))
                                {
                                    translatedQuestName = Service.translationHandler.TranslateUI(dialogue).GetAwaiter().GetResult();
                                    TranslationHandler.DialogueTranslationCache.Add(dialogue, translatedQuestName);
                                }

                                TranslatedQuestNames[capturedText] = translatedQuestName;
                                capturedNode->NodeText.SetString(translatedQuestName);
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
