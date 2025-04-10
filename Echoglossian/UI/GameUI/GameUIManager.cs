using Dalamud.Game.Addon.Lifecycle;
using Echoglossian.Utils;
using System;

namespace Echoglossian.UI.GameUI
{
    internal class GameUIManager : IDisposable
    {
        // Register UI addon handlers
        public GameUIManager()
        {
            // Talk
            Service.addonLifecycle.RegisterListener(AddonEvent.PreRefresh, "Talk", UiTalkHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PreDraw, "Talk", UiTalkHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "Talk", UiTalkHandler.OnEvent);

            // BattleTalk
            Service.addonLifecycle.RegisterListener(AddonEvent.PreRefresh, "_BattleTalk", UiBattleTalkHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PreDraw, "_BattleTalk", UiBattleTalkHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "_BattleTalk", UiBattleTalkHandler.OnEvent);

            // Toast

            // TalkSubtitle
            Service.addonLifecycle.RegisterListener(AddonEvent.PreSetup, "TalkSubtitle", UiTalkSubtitleHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PreRefresh, "TalkSubtitle", UiTalkSubtitleHandler.OnEvent);

            // Journal
            Service.addonLifecycle.RegisterListener(AddonEvent.PreSetup, "JournalResult", UiJournalHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "RecommendList", UiJournalHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "RecommendList", UiJournalHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PreRefresh, "AreaMap", UiJournalHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PreRefresh, "ScenarioTree", UiJournalHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PreUpdate, "Journal", UiJournalHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "Journal", UiJournalHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "JournalDetail", UiJournalHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PreSetup, "JournalAccept", UiJournalHandler.OnEvent);
            Service.addonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_ToDoList", UiJournalHandler.OnEvent);
        }

        public void Dispose()
        {
            Service.addonLifecycle.UnregisterListener(
                UiTalkHandler.OnEvent,
                UiBattleTalkHandler.OnEvent,
                UiTalkSubtitleHandler.OnEvent,
                UiJournalHandler.OnEvent);
        }
    }
}
