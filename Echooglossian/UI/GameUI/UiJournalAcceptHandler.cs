using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using Echooglossian.Utils;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Threading.Tasks;
using static Echooglossian.Utils.LanguageDictionary;


namespace Echooglossian.UI.GameUI
{
    internal partial class UiJournalHandler
    {
        private static unsafe void HandleJournalAccept(AddonSetupArgs args)
        {
            if (args == null || args.AtkValues == IntPtr.Zero)
                return;

            var atkValues = (AtkValue*)args.AtkValues;

            try
            {
                // Translate quest name
                if (atkValues[3].Type == FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String && atkValues[3].String != null)
                {
                    var questName = MemoryHelper.ReadSeStringAsString(out _, (nint)atkValues[3].String.Value);
                    if (!string.IsNullOrEmpty(questName))
                    {
                        TranslateQuestName(questName, atkValues, 3);
                    }
                }

                // Translate quest description
                if (atkValues[4].Type == FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String && atkValues[4].String != null)
                {
                    var descText = MemoryHelper.ReadSeStringAsString(out _, (nint)atkValues[4].String.Value);
                    if (!string.IsNullOrEmpty(descText))
                    {
                        string descKey = $"desc_{GetLanguage(Service.clientState.ClientLanguage.ToString()).Code}_{Service.configuration.SelectedTargetLanguage.Code}_{descText.GetHashCode()}";

                        if (Service.translationCache.TryGetString(descKey, out string translatedDesc))
                        {
                            atkValues[4].SetManagedString(translatedDesc);
                        }
                        else
                        {
                            string capturedDesc = descText;

                            Task.Run(() =>
                            {
                                try
                                {
                                    var fromLang = (LanguageInfo)Service.clientState.ClientLanguage;
                                    var toLang = Service.configuration.SelectedTargetLanguage;

                                    string cachedTranslation = Service.translationHandler.TranslateString(capturedDesc, toLang)
                                        .GetAwaiter().GetResult();

                                    string finalKey = $"desc_{fromLang.Code}_{toLang.Code}_{capturedDesc.GetHashCode()}";
                                    Service.translationCache.UpsertString(finalKey, cachedTranslation);
                                    atkValues[4].SetManagedString(cachedTranslation);
                                }
                                catch (Exception e)
                                {
                                    Service.pluginLog.Error($"HandleJournalAccept desc translation error: {e}");
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Service.pluginLog.Error($"HandleJournalAccept error: {e}");
            }
        }

    }
}
