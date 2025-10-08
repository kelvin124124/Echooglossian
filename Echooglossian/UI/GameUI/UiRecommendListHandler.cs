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
        private static unsafe void TranslateRecommendList()
        {
            var atkStage = AtkStage.Instance();
            var recommendList = atkStage->RaptureAtkUnitManager->GetAddonByName("RecommendList");
            if (recommendList == null || !recommendList->IsVisible)
                return;

            try
            {
                // Process quest list nodes
                var questListNode = recommendList->GetNodeById(5);
                if (questListNode == null || !questListNode->IsVisible())
                    return;

                var questListComponent = questListNode->GetAsAtkComponentNode()->Component;
                for (var i = 0; i < questListComponent->UldManager.NodeListCount; i++)
                {
                    if (!questListComponent->UldManager.NodeList[i]->IsVisible())
                        continue;

                    if (questListComponent->UldManager.NodeList[i]->Type == NodeType.Collision ||
                        questListComponent->UldManager.NodeList[i]->Type == NodeType.Res)
                        continue;

                    var questItemNode = questListComponent->UldManager.NodeList[i]->GetAsAtkComponentNode();
                    var questNameNode = questItemNode->Component->UldManager.SearchNodeById(5);
                    if (questNameNode == null || !questNameNode->IsVisible() || questNameNode->Type != NodeType.Text)
                        continue;

                    var questName = questNameNode->GetAsAtkTextNode();
                    if (questName->NodeText.IsEmpty)
                        continue;

                    var questNameText = MemoryHelper.ReadSeStringAsString(out _, (nint)questName->NodeText.StringPtr.Value);

                    // Check cache first
                    if (TranslatedQuestNames.TryGetValue(questNameText, out string translatedQuestName))
                    {
                        questName->NodeText.SetString(translatedQuestName);
                        continue;
                    }

                    // Store the text for use in the Task
                    string capturedQuestText = questNameText;
                    AtkTextNode* capturedNode = questName;

                    // Use Task.Run without await inside unsafe context
                    Task.Run(() =>
                    {
                        try
                        {
                            var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                            var toLang = Service.configuration.SelectedTargetLanguage;

                            Dialogue dialogue = new(nameof(UiJournalHandler), fromLang, toLang, capturedQuestText);

                            if (!TranslationHandler.DialogueTranslationCache.TryGetValue(dialogue, out string translatedQuestText))
                            {
                                // Call async method synchronously to avoid await in this Task
                                translatedQuestText = Service.translationHandler.TranslateUI(dialogue).GetAwaiter().GetResult();
                                TranslationHandler.DialogueTranslationCache.Add(dialogue, translatedQuestText);
                            }

                            TranslatedQuestNames[capturedQuestText] = translatedQuestText;
                            capturedNode->NodeText.SetString(translatedQuestText);
                        }
                        catch (Exception e)
                        {
                            Service.pluginLog.Error($"TranslateRecommendList quest translation error: {e}");
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"TranslateRecommendList error: {e}");
            }
        }

    }
}
