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
                    if (TranslatedQuestNames.TryGetValue(questNameText, out string translatedName))
                    {
                        questName->NodeText.SetString(translatedName);
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
                            var toLang = Service.config.SelectedTargetLanguage;

                            string questNameKey = $"quest_{fromLang.Code}_{toLang.Code}_{capturedQuestText}";
                            string cachedTranslation;

                            if (!Service.translationCache.TryGetString(questNameKey, out cachedTranslation))
                            {
                                // Call async method synchronously to avoid await in this Task
                                cachedTranslation = Service.translationHandler.TranslateString(capturedQuestText, toLang)
                                    .GetAwaiter().GetResult();
                                Service.translationCache.UpsertString(questNameKey, cachedTranslation);
                            }

                            TranslatedQuestNames[capturedQuestText] = cachedTranslation;
                            capturedNode->NodeText.SetString(cachedTranslation);
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
