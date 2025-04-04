using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Command;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Echoglossian.Properties;
using System.Globalization;
using System.Reflection;

namespace Echoglossian
{
    public class Echoglossian : IDalamudPlugin
    {
        private const string CommandName = "/eglo";

        public WindowSystem WindowSystem { get; } = new("Echoglossian");

        public Echoglossian(IDalamudPluginInterface pluginInterface, ICommandManager commandManager)
        {
            _ = pluginInterface.Create<Service>();

            Service.pluginInterface = pluginInterface;
            Service.commandManager = commandManager;

            Service.config = pluginInterface.GetPluginConfig() as Config ?? new Config();

            Service.plugin = this;

            Service.mainWindow = new MainWindow(this);
            WindowSystem.AddWindow(Service.mainWindow);

            pluginInterface.UiBuilder.Draw += DrawUI;
            pluginInterface.UiBuilder.OpenMainUi += DrawMainUI;
            pluginInterface.UiBuilder.DisableCutsceneUiHide = Service.config.ShowInCutscenes;

            Service.commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open Echoglossian main window."
            });

            IDalamudTextureWrap pixImage = Service.textureProvider.CreateFromImageAsync(Resources.pix).Result;
            IDalamudTextureWrap choiceImage = Service.textureProvider.CreateFromImageAsync(Resources.choice).Result;
            IDalamudTextureWrap cutsceneChoiceImage = Service.textureProvider.CreateFromImageAsync(Resources.cutscenechoice).Result;
            IDalamudTextureWrap talkImage = Service.textureProvider.CreateFromImageAsync(Resources.prttws).Result;
            IDalamudTextureWrap logo = Service.textureProvider.CreateFromImageAsync(Resources.logo).Result;

            CultureInfo cultureInfo = new(Service.config.DefaultPluginCulture);

            Service.assetManager = new AssetManager();

            CreateOrUseDb();

            Service.config.PluginVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString()!;
            if (Service.config.Version < 5)
            {
                MigrateConfig();
            }
        }

        private void OnCommand(string command, string args)
        {
            Service.mainWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        public static void DrawMainUI()
        {
            Service.mainWindow.IsOpen = true;
        }

        public void Dispose()
        {

        }

        private void EgloAddonHandler()
        {
            if (this.configuration.TranslateTalk)
            {
                // this.EgloNeutralAddonHandler("Talk", new string[] {  /* "PreUpdate", "PostUpdate",*/ "PreDraw",/* "PostDraw",  "PreReceiveEvent", "PostReceiveEvent", "PreRequestedUpdate", "PostRequestedUpdate" ,*/ "PreRefresh",/* "PostRefresh"*/ });
                AddonLifecycle.RegisterListener(AddonEvent.PreRefresh, "Talk", this.UiTalkAsyncHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "Talk", this.UiTalkAsyncHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "Talk", this.UiTalkAsyncHandler);
            }

            if (this.configuration.TranslateBattleTalk)
            {
                // this.EgloNeutralAddonHandler("_BattleTalk", new string[] { /* "PreUpdate", "PostUpdate",*/ "PreDraw",/* "PostDraw",  "PreReceiveEvent", "PostReceiveEvent", "PreRequestedUpdate", "PostRequestedUpdate" ,*/ "PreRefresh",/* "PostRefresh"*/});
                AddonLifecycle.RegisterListener(AddonEvent.PreRefresh, "_BattleTalk", this.UiBattleTalkAsyncHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "_BattleTalk", this.UiBattleTalkAsyncHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "_BattleTalk", this.UiBattleTalkAsyncHandler);
            }

            if (this.configuration.TranslateTalkSubtitle)
            {
                // this.EgloNeutralAddonHandler("TalkSubtitle", new string[] {/* "PreUpdate", "PostUpdate",*/ "PreDraw",/* "PostDraw",  "PreReceiveEvent", "PostReceiveEvent", "PreRequestedUpdate", "PostRequestedUpdate" ,*/ "PreRefresh",/* "PostRefresh"*/});
                AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "TalkSubtitle", this.UiTalkSubtitleAsyncHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PreRefresh, "TalkSubtitle", this.UiTalkSubtitleAsyncHandler);
            }

            if (this.configuration.TranslateJournal)
            {
                AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "JournalResult", this.UiJournalResultHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "RecommendList", this.UiRecommendListHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "RecommendList", this.UiRecommendListHandlerAsync);
                AddonLifecycle.RegisterListener(AddonEvent.PreRefresh, "AreaMap", this.UiAreaMapHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PreRefresh, "ScenarioTree", this.UiScenarioTreeHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PreUpdate, "Journal", this.UiJournalQuestHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "Journal", this.UiJournalDetailHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "JournalDetail", this.UiJournalDetailHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "JournalAccept", this.UiJournalAcceptHandler);
                AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_ToDoList", this.UiToDoListHandler);
            }

            /*"PreSetup","PostSetup", "PreUpdate", "PostUpdate", "PreDraw", "PostDraw", "PreFinalize", "PreReceiveEvent", "PostReceiveEvent", "PreRequestedUpdate", "PostRequestedUpdate", "PreRefresh", "PostRefresh" */
        }
    }
}
