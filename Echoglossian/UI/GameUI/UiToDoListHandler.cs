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
        private static unsafe void HandleToDoList()
        {
            var toDoList = (AtkUnitBase*)Service.gameGui.GetAddonByName("_ToDoList").Address;
            if (toDoList == null || !toDoList->IsVisible)
                return;

            try
            {
                // Handle ToDo list entries
                var listNode = toDoList->GetNodeById(4);
                if (listNode == null || !listNode->IsVisible())
                    return;

                var listComponent = listNode->GetAsAtkComponentNode()->Component;
                for (var i = 0; i < listComponent->UldManager.NodeListCount; i++)
                {
                    if (!listComponent->UldManager.NodeList[i]->IsVisible())
                        continue;

                    var itemNode = listComponent->UldManager.NodeList[i]->GetAsAtkComponentNode();
                    var textNode = itemNode->Component->UldManager.SearchNodeById(3);
                    if (textNode == null || !textNode->IsVisible() || textNode->Type != NodeType.Text)
                        continue;

                    var todoText = textNode->GetAsAtkTextNode();
                    if (todoText->NodeText.IsEmpty)
                        continue;

                    var todoTextStr = MemoryHelper.ReadSeStringAsString(out _, (nint)todoText->NodeText.StringPtr.Value);
                    if (string.IsNullOrEmpty(todoTextStr))
                        continue;

                    string todoKey = $"todo_{GetLanguage(Service.clientState.ClientLanguage.ToString()).Code}_{Service.config.SelectedTargetLanguage.Code}_{todoTextStr.GetHashCode()}";

                    if (Service.translationCache.TryGetString(todoKey, out string translatedTodo))
                    {
                        todoText->NodeText.SetString(translatedTodo);
                    }
                    else
                    {
                        string capturedText = todoTextStr;
                        AtkTextNode* capturedNode = todoText;

                        Task.Run(() =>
                        {
                            try
                            {
                                var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                                var toLang = Service.config.SelectedTargetLanguage;

                                string cachedTranslation = Service.translationHandler.TranslateString(capturedText, toLang)
                                    .GetAwaiter().GetResult();

                                string finalKey = $"todo_{fromLang.Code}_{toLang.Code}_{capturedText.GetHashCode()}";
                                Service.translationCache.UpsertString(finalKey, cachedTranslation);
                                capturedNode->NodeText.SetString(cachedTranslation);
                            }
                            catch (Exception e)
                            {
                                Service.pluginLog.Error($"HandleToDoList translation error: {e}");
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"HandleToDoList error: {e}");
            }
        }

    }
}
