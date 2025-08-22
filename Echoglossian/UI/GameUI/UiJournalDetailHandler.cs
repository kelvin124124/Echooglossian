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
                                    var toLang = Service.config.SelectedTargetLanguage;

                                    string titleKey = $"quest_{fromLang.Code}_{toLang.Code}_{capturedTitle}";
                                    string cachedTranslation;

                                    if (!Service.translationCache.TryGetString(titleKey, out cachedTranslation))
                                    {
                                        cachedTranslation = Service.translationHandler.TranslateString(capturedTitle, toLang)
                                            .GetAwaiter().GetResult();
                                        Service.translationCache.UpsertString(titleKey, cachedTranslation);
                                    }

                                    TranslatedQuestNames[capturedTitle] = cachedTranslation;
                                    capturedNode->NodeText.SetString(cachedTranslation);
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
                        string descKey = $"desc_{GetLanguage(Service.clientState.ClientLanguage.ToString()).Code}_{Service.config.SelectedTargetLanguage.Code}_{descText.GetHashCode()}";

                        if (Service.translationCache.TryGetString(descKey, out string translatedDesc))
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
                                    var toLang = Service.config.SelectedTargetLanguage;

                                    string cachedTranslation = Service.translationHandler.TranslateString(capturedDesc, toLang)
                                        .GetAwaiter().GetResult();

                                    string finalKey = $"desc_{fromLang.Code}_{toLang.Code}_{capturedDesc.GetHashCode()}";
                                    Service.translationCache.UpsertString(finalKey, cachedTranslation);
                                    capturedNode->NodeText.SetString(cachedTranslation);
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
                            string objectiveKey = $"obj_{GetLanguage(Service.clientState.ClientLanguage.ToString()).Code}_{Service.config.SelectedTargetLanguage.Code}_{objectiveText.GetHashCode()}";

                            if (Service.translationCache.TryGetString(objectiveKey, out string translatedObjective))
                            {
                                objectiveNode->NodeText.SetString(translatedObjective);
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
                                        var toLang = Service.config.SelectedTargetLanguage;

                                        string cachedTranslation = Service.translationHandler.TranslateString(capturedObjective, toLang)
                                            .GetAwaiter().GetResult();

                                        string finalKey = $"obj_{fromLang.Code}_{toLang.Code}_{capturedObjective.GetHashCode()}";
                                        Service.translationCache.UpsertString(finalKey, cachedTranslation);
                                        capturedNode->NodeText.SetString(cachedTranslation);
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
