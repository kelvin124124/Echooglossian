using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Echoglossian.Chat;
using Echoglossian.Database;
using Echoglossian.Translate;
using Echoglossian.UI.GameUI;
using Echoglossian.UI.Windows;
using Echoglossian.Utils;
using System;
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

            Service.assetManager = new AssetManager();
            Service.translationCache = new TranslationCache();
            Service.translationHandler = new TranslationHandler();
            Service.overlayManager = new OverlayManager();
            Service.gameUIManager = new GameUIManager();
            Service.chatHandler = new ChatHandler();
            Service.pfHandler = new PFHandler();

            Service.config.PluginVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString()!;
            if (Service.config.Version < 5)
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
            Service.translationCache?.Dispose();
        }
    }
}
