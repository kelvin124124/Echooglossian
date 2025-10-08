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

                    Dialogue dialogue = new(nameof(UiJournalHandler), (LanguageInfo)Service.clientState.ClientLanguage, Service.configuration.SelectedTargetLanguage, todoTextStr);

                    if (TranslationHandler.DialogueTranslationCache.TryGetValue(dialogue, out string translatedTodo))
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
                                var toLang = Service.configuration.SelectedTargetLanguage;

                                string translatedTodo = Service.translationHandler.TranslateUI(dialogue).GetAwaiter().GetResult();

                                TranslationHandler.DialogueTranslationCache.Add(dialogue, translatedTodo);
                                capturedNode->NodeText.SetString(translatedTodo);
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
