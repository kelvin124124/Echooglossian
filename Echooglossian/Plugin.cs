using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Echooglossian.Chat;
using Echooglossian.Translate;
using Echooglossian.UI.GameUI;
using Echooglossian.UI.Windows;
using Echooglossian.Utils;
using System;
using System.Reflection;

namespace Echooglossian
{
    public class Plugin : IDalamudPlugin
    {
        private const string CommandName = "/egloo";

        public WindowSystem WindowSystem { get; } = new("Echooglossian");

        public Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commandManager)
        {
            _ = pluginInterface.Create<Service>();

            Service.pluginInterface = pluginInterface;
            Service.commandManager = commandManager;

            Service.configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            Service.plugin = this;

            Service.mainWindow = new ConfigWindow(this);
            WindowSystem.AddWindow(Service.mainWindow);

            pluginInterface.UiBuilder.Draw += DrawUI;
            pluginInterface.UiBuilder.OpenMainUi += DrawMainUI;
            pluginInterface.UiBuilder.DisableCutsceneUiHide = Service.configuration.ShowInCutscenes;

            Service.commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open Echoglossian main window."
            });

            Service.fontManager = new FontManager();
            Service.translationHandler = new TranslationHandler();
            Service.overlayManager = new TranslationOverlay();
            Service.gameUIManager = new GameUIManager();
            Service.chatHandler = new ChatHandler();
            Service.pfHandler = new PFHandler();

            Service.configuration.PluginVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString()!;
            if (Service.configuration.Version < 5)
            {
                MigrateConfig();
            }
        }

        private static void MigrateConfig()
        {
            throw new NotImplementedException();
        }

        private void OnCommand(string command, string args)
        {
            Service.mainWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            WindowSystem.Draw();

            // draw overlays
            Service.overlayManager?.Draw();
        }

        public static void DrawMainUI()
        {
            Service.mainWindow.IsOpen = true;
        }

        public void Dispose()
        {
            Service.commandManager?.RemoveHandler(CommandName);
            WindowSystem?.RemoveAllWindows();

            Service.chatHandler?.Dispose();
            Service.pfHandler?.Dispose();

            Service.gameUIManager?.Dispose();
            Service.overlayManager?.Dispose();

            Service.translationHandler?.Dispose();
            Service.fontManager?.Dispose();
        }
    }
}
