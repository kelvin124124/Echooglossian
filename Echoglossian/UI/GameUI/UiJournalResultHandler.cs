using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Echoglossian.UI.GameUI
{
    internal partial class UiJournalHandler
    {
        private static unsafe void HandleJournalResult(AddonSetupArgs args)
        {
            if (args == null)
                return;

            var atkValues = (AtkValue*)args.AtkValues;
            if (atkValues == null)
                return;

            if (atkValues[1].Type != FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String || atkValues[1].String == null)
                return;

            var questNameText = MemoryHelper.ReadSeStringAsString(out _, (nint)atkValues[1].String.Value);
            if (string.IsNullOrEmpty(questNameText))
                return;

            TranslateQuestName(questNameText, atkValues, 1);
        }
    }
}
