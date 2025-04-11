using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Echoglossian.UI.GameUI
{
    internal partial class UiJournalHandler
    {
        private static unsafe void HandleScenarioTree(AddonRefreshArgs args)
        {
            if (args == null)
                return;

            var atkValues = (AtkValue*)args.AtkValues;
            if (atkValues == null)
                return;

            // Translate MSQ
            TranslateQuestOnScenarioTree(atkValues, 7);

            // Translate SubQuest
            TranslateQuestOnScenarioTree(atkValues, 2);
        }

        private static unsafe void TranslateQuestOnScenarioTree(AtkValue* atkValues, int valueIndex)
        {
            if (atkValues[valueIndex].Type != FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String ||
                atkValues[valueIndex].String == null)
                return;

            var questNameText = MemoryHelper.ReadSeStringAsString(out _, (nint)atkValues[valueIndex].String.Value);
            if (string.IsNullOrEmpty(questNameText))
                return;

            TranslateQuestName(questNameText, atkValues, valueIndex);
        }
    }
}
