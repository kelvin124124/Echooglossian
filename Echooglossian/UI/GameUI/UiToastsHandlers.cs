using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;
using Echooglossian.Translate;
using Echooglossian.Utils;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Echooglossian.Utils.LanguageDictionary;

namespace Echooglossian.UI.GameUI
{
    internal class UiToastsHandlers : IDisposable
    {
        private readonly Dictionary<ToastType, bool> toastDisplayFlags = new()
        {
            { ToastType.Normal, false },
            { ToastType.Error, false },
            { ToastType.Quest, false },
            { ToastType.ClassChange, false },
            { ToastType.Area, false },
            { ToastType.WideText, false }
        };

        // Track active translations
        private readonly Dictionary<ToastType, string> currentTranslations = [];
        private readonly Dictionary<ToastType, int> translationIds = [];

        // Store pending SeString references
        private readonly Dictionary<ToastType, SeStringRef> pendingRefs = [];

        private enum ToastType
        {
            Normal,
            Error,
            Quest,
            ClassChange,
            Area,
            WideText
        }

        private struct SeStringRef
        {
            public SeString Message;
            public bool IsHandled;
            public ToastOptions? Options;
            public QuestToastOptions? QuestOptions;
        }

        public UiToastsHandlers()
        {
            Service.toastGui.Toast += OnToast;
            Service.toastGui.ErrorToast += OnErrorToast;
            Service.toastGui.QuestToast += OnQuestToast;
            Service.framework.Update += OnFrameworkUpdate;
        }

        public void Dispose()
        {
            Service.toastGui.Toast -= OnToast;
            Service.toastGui.ErrorToast -= OnErrorToast;
            Service.toastGui.QuestToast -= OnQuestToast;
            Service.framework.Update -= OnFrameworkUpdate;
        }

        private void OnFrameworkUpdate(Dalamud.Plugin.Services.IFramework framework)
        {
            if (!Service.configuration.ToastModuleEnabled || !Service.clientState.IsLoggedIn)
                return;

            CheckToastVisibility("_TextError", ToastType.Error);
            CheckToastVisibility("_WideText", ToastType.WideText);
            CheckToastVisibility("_TextClassChange", ToastType.ClassChange);
            CheckToastVisibility("_AreaText", ToastType.Area);
            CheckToastVisibility("_QuestToast", ToastType.Quest);
        }

        private unsafe void CheckToastVisibility(string addonName, ToastType toastType)
        {
            IntPtr addonPtr = Service.gameGui.GetAddonByName(addonName);
            if (addonPtr == IntPtr.Zero)
            {
                toastDisplayFlags[toastType] = false;
                return;
            }

            AtkUnitBase* addon = (AtkUnitBase*)addonPtr;
            toastDisplayFlags[toastType] = addon->IsVisible;

            if (addon->IsVisible)
            {
                Service.translationOverlay.UpdateToastOverlayPosition(
                    toastType.ToString(),
                    addon->RootNode->X,
                    addon->RootNode->Y,
                    addon->RootNode->Width * addon->Scale,
                    addon->RootNode->Height * addon->Scale);
            }
            else
            {
                Service.translationOverlay.SetToastOverlayVisible(toastType.ToString(), false);
            }
        }

        private void OnToast(ref SeString message, ref ToastOptions options, ref bool isHandled)
        {
            if (!Service.configuration.ToastModuleEnabled || !Service.configuration.TOAST_TranslateRegular)
                return;

            pendingRefs[ToastType.Normal] = new SeStringRef
            {
                Message = message,
                IsHandled = isHandled,
                Options = options
            };

            HandleToast(ToastType.Normal, message.TextValue, ref message);
        }

        private void OnErrorToast(ref SeString message, ref bool isHandled)
        {
            if (!Service.configuration.ToastModuleEnabled || !Service.configuration.TOAST_TranslateError)
                return;

            pendingRefs[ToastType.Error] = new SeStringRef
            {
                Message = message,
                IsHandled = isHandled
            };

            HandleToast(ToastType.Error, message.TextValue, ref message);
        }

        private void OnQuestToast(ref SeString message, ref QuestToastOptions options, ref bool isHandled)
        {
            if (!Service.configuration.ToastModuleEnabled || !Service.configuration.TOAST_TranslateQuest)
                return;

            pendingRefs[ToastType.Quest] = new SeStringRef
            {
                Message = message,
                IsHandled = isHandled,
                QuestOptions = options
            };

            HandleToast(ToastType.Quest, message.TextValue, ref message);
        }

        private void HandleToast(ToastType toastType, string messageText, ref SeString originalMessage)
        {
            try
            {
                var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                var toLang = Service.configuration.SelectedTargetLanguage;

                Dialogue dialogue = new(nameof(UiToastsHandlers), fromLang, toLang, messageText);

                // Check cache first
                if (TranslationHandler.DialogueTranslationCache.TryGetValue(dialogue, out string translatedMessage))
                {
                    ApplyToastTranslation(toastType, messageText, translatedMessage, ref originalMessage);
                    return;
                }

                // Start new translation
                int translationId = Environment.TickCount;
                translationIds[toastType] = translationId;
                currentTranslations[toastType] = "Waiting for translation...";

                // Handle immediate modifications for non-ImGui
                if (!Service.configuration.TOAST_UseImGui)
                {
                    // Store original message reference
                    pendingRefs[toastType] = new SeStringRef
                    {
                        Message = originalMessage,
                        IsHandled = pendingRefs[toastType].IsHandled
                    };
                }
                else
                {
                    // Show overlay for ImGui
                    Service.translationOverlay.UpdateToastOverlay(
                        toastType.ToString(),
                        messageText,
                        currentTranslations[toastType]);
                }

                StartToastTranslation(toastType, dialogue, translationId);
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"HandleToast error: {e}");
            }
        }

        private void StartToastTranslation(ToastType toastType, Dialogue dialogue, int translationId)
        {
            Task.Run(async () =>
            {
                try
                {
                    string translatedText = await Service.translationHandler.TranslateUI(dialogue);

                    // Only update if this is still the active toast
                    if (translationIds[toastType] == translationId)
                    {
                        currentTranslations[toastType] = translatedText;
                        TranslationHandler.DialogueTranslationCache.Add(dialogue, translatedText);

                        if (Service.configuration.TOAST_UseImGui)
                        {
                            Service.translationOverlay.UpdateToastOverlay(
                                toastType.ToString(),
                                dialogue.Content,
                                translatedText);
                            Service.translationOverlay.SetToastOverlayVisible(toastType.ToString(), true);
                        }
                    }
                }
                catch (Exception e)
                {
                    Service.pluginLog.Error($"Toast translation error ({toastType}): {e}");
                }
            });
        }

        private void ApplyToastTranslation(ToastType toastType, string originalText, string translatedText, ref SeString originalMessage)
        {
            currentTranslations[toastType] = translatedText;

            if (Service.configuration.TOAST_UseImGui)
            {
                Service.translationOverlay.UpdateToastOverlay(
                    toastType.ToString(),
                    originalText,
                    translatedText);

                Service.translationOverlay.SetToastOverlayVisible(toastType.ToString(), true);
            }
            else
            {
                // Replace the message directly
                originalMessage = (SeString)translatedText;
            }
        }
    }
}
