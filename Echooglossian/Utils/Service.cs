using Dalamud.Game.Text.Sanitizer;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Echooglossian.Chat;
using Echooglossian.Translate;
using Echooglossian.UI.GameUI;
using Echooglossian.UI.Windows;

namespace Echooglossian.Utils
{
    internal class Service
    {
        [PluginService] public static IDalamudPluginInterface pluginInterface { get; set; } = null!;
        [PluginService] public static ICommandManager commandManager { get; set; } = null!;
        [PluginService] public static IContextMenu contextMenu { get; set; } = null!;
        [PluginService] public static IFramework framework { get; set; } = null!;
        [PluginService] public static IGameGui gameGui { get; set; } = null!;
        [PluginService] public static IChatGui chatGui { get; set; } = null!;
        [PluginService] public static ICondition condition { get; set; } = null!;
        [PluginService] public static IClientState clientState { get; set; } = null!;
        [PluginService] public static IToastGui toastGui { get; set; } = null!;
        [PluginService] public static IAddonLifecycle addonLifecycle { get; set; } = null!;
        [PluginService] public static IPluginLog pluginLog { get; set; } = null!;
        [PluginService] public static INotificationManager notificationManager { get; set; } = null!;

        internal static Plugin plugin { get; set; } = null!;
        internal static Configuration configuration { get; set; } = null!;
        internal static FontManager fontManager { get; set; } = null!;
        internal static ConfigWindow mainWindow { get; set; } = null!;

        internal static TranslationHandler translationHandler { get; set; } = null!;
        internal static TranslationOverlay overlayManager { get; set; } = null!;
        internal static GameUIManager gameUIManager { get; set; } = null!;

        internal static ChatHandler chatHandler { get; set; } = null!;
        internal static PFHandler pfHandler { get; set; } = null!;

        internal static Sanitizer sanitizer { get; set; } = null!;
    }
}
