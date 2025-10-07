using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;
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
                Service.overlayManager.UpdateToastOverlayPosition(
                    toastType.ToString(),
                    addon->RootNode->X,
                    addon->RootNode->Y,
                    addon->RootNode->Width * addon->Scale,
                    addon->RootNode->Height * addon->Scale);
            }
            else
            {
                Service.overlayManager.SetToastOverlayVisible(toastType.ToString(), false);
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
                string cacheKey = $"toast_{fromLang.Code}_{toLang.Code}_{toastType}_{messageText.GetHashCode()}";

                // Check cache first
                if (Service.translationCache.TryGetString(cacheKey, out string cachedTranslation))
                {
                    ApplyToastTranslation(toastType, messageText, cachedTranslation, ref originalMessage);
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
                    Service.overlayManager.UpdateToastOverlay(
                        toastType.ToString(),
                        messageText,
                        currentTranslations[toastType]);
                }

                StartToastTranslation(toastType, messageText, cacheKey, translationId, toLang);
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"HandleToast error: {e}");
            }
        }

        private void StartToastTranslation(ToastType toastType, string messageText, string cacheKey, int translationId, LanguageInfo toLang)
        {
            Task.Run(async () =>
            {
                try
                {
                    string translatedText = await Service.translationHandler.TranslateString(messageText, toLang);

                    // Only update if this is still the active toast
                    if (translationIds[toastType] == translationId)
                    {
                        currentTranslations[toastType] = translatedText;
                        Service.translationCache.UpsertString(cacheKey, translatedText);

                        if (Service.configuration.TOAST_UseImGui)
                        {
                            Service.overlayManager.UpdateToastOverlay(
                                toastType.ToString(),
                                messageText,
                                translatedText);
                            Service.overlayManager.SetToastOverlayVisible(toastType.ToString(), true);
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
                Service.overlayManager.UpdateToastOverlay(
                    toastType.ToString(),
                    originalText,
                    translatedText);

                Service.overlayManager.SetToastOverlayVisible(toastType.ToString(), true);
            }
            else
            {
                // Replace the message directly
                originalMessage = (SeString)translatedText;
            }
        }
    }
}
