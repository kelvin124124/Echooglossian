using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Echoglossian.UI.GameUI
{
    internal partial class UiJournalHandler
    {
        private static unsafe void HandleAreaMap(AddonRefreshArgs args)
        {
            if (args == null)
                return;

            var atkValues = (AtkValue*)args.AtkValues;
            if (atkValues == null)
                return;

            if (atkValues[142].Type != ValueType.String || atkValues[142].String == null)
                return;

            var questNameText = MemoryHelper.ReadSeStringAsString(out _, (nint)atkValues[142].String.Value);
            if (string.IsNullOrEmpty(questNameText))
                return;

            TranslateQuestName(questNameText, atkValues, 142);
        }
    }
}
