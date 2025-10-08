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
        private static unsafe void HandleJournalDetail()
        {
            var journalDetail = (AtkUnitBase*)Service.gameGui.GetAddonByName("JournalDetail").Address;
            if (journalDetail == null || !journalDetail->IsVisible)
                return;

            try
            {
                // Translate quest title
                var titleNode = journalDetail->GetTextNodeById(8);
                if (titleNode != null && !titleNode->NodeText.IsEmpty)
                {
                    var titleText = MemoryHelper.ReadSeStringAsString(out _, (nint)titleNode->NodeText.StringPtr.Value);
                    if (!string.IsNullOrEmpty(titleText))
                    {
                        if (TranslatedQuestNames.TryGetValue(titleText, out string translatedTitle))
                        {
                            titleNode->NodeText.SetString(translatedTitle);
                        }
                        else
                        {
                            string capturedTitle = titleText;
                            AtkTextNode* capturedNode = titleNode;

                            Task.Run(() =>
                            {
                                try
                                {
                                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                                    var toLang = Service.configuration.SelectedTargetLanguage;

                                    Dialogue dialogue = new(nameof(UiJournalHandler), fromLang, toLang, capturedTitle);
                                    string translatedTitle;

                                    if (!TranslationHandler.DialogueTranslationCache.TryGetValue(dialogue, out translatedTitle))
                                    {
                                        translatedTitle = Service.translationHandler.TranslateUI(dialogue).GetAwaiter().GetResult();
                                        TranslationHandler.DialogueTranslationCache.Add(dialogue, translatedTitle);
                                    }

                                    TranslatedQuestNames[capturedTitle] = translatedTitle;
                                    capturedNode->NodeText.SetString(translatedTitle);
                                }
                                catch (Exception e)
                                {
                                    Service.pluginLog.Error($"HandleJournalDetail title translation error: {e}");
                                }
                            });
                        }
                    }
                }

                // Translate quest description
                var descNode = journalDetail->GetTextNodeById(15);
                if (descNode != null && !descNode->NodeText.IsEmpty)
                {
                    var descText = MemoryHelper.ReadSeStringAsString(out _, (nint)descNode->NodeText.StringPtr.Value);
                    if (!string.IsNullOrEmpty(descText))
                    {
                        Dialogue dialogue = new(nameof(UiJournalHandler), GetLanguage(Service.clientState.ClientLanguage.ToString()), Service.configuration.SelectedTargetLanguage, descText);

                        if (TranslationHandler.DialogueTranslationCache.TryGetValue(dialogue, out string translatedDesc))
                        {
                            descNode->NodeText.SetString(translatedDesc);
                        }
                        else
                        {
                            string capturedDesc = descText;
                            AtkTextNode* capturedNode = descNode;

                            Task.Run(() =>
                            {
                                try
                                {
                                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                                    var toLang = Service.configuration.SelectedTargetLanguage;

                                    string translatedDesc = Service.translationHandler.TranslateUI(dialogue).GetAwaiter().GetResult();

                                    TranslationHandler.DialogueTranslationCache.Add(dialogue, translatedDesc);
                                    capturedNode->NodeText.SetString(translatedDesc);
                                }
                                catch (Exception e)
                                {
                                    Service.pluginLog.Error($"HandleJournalDetail desc translation error: {e}");
                                }
                            });
                        }
                    }
                }

                // Translate objectives
                for (uint i = 2; i <= 6; i++)
                {
                    var objectiveNode = journalDetail->GetTextNodeById(20 + i);
                    if (objectiveNode != null && !objectiveNode->NodeText.IsEmpty)
                    {
                        var objectiveText = MemoryHelper.ReadSeStringAsString(out _, (nint)objectiveNode->NodeText.StringPtr.Value);
                        if (!string.IsNullOrEmpty(objectiveText))
                        {
                            Dialogue dialogue = new(nameof(UiJournalHandler), GetLanguage(Service.clientState.ClientLanguage.ToString()), Service.configuration.SelectedTargetLanguage, objectiveText);

                            if (TranslationHandler.DialogueTranslationCache.TryGetValue(dialogue, out string translatedObj))
                            {
                                objectiveNode->NodeText.SetString(translatedObj);
                            }
                            else
                            {
                                string capturedObjective = objectiveText;
                                AtkTextNode* capturedNode = objectiveNode;

                                Task.Run(() =>
                                {
                                    try
                                    {
                                        var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                                        var toLang = Service.configuration.SelectedTargetLanguage;

                                        string translatedObj = Service.translationHandler.TranslateUI(dialogue).GetAwaiter().GetResult();

                                        TranslationHandler.DialogueTranslationCache.Add(dialogue, translatedObj);
                                        capturedNode->NodeText.SetString(translatedObj);
                                    }
                                    catch (Exception e)
                                    {
                                        Service.pluginLog.Error($"HandleJournalDetail objective translation error: {e}");
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"HandleJournalDetail error: {e}");
            }
        }

    }
}
