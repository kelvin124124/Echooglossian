using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Echooglossian.Chat;
using Echooglossian.Utils;
using System.Threading.Tasks;
using static Echooglossian.Utils.LanguageDictionary;

namespace Echooglossian.Ipc
{
    public static class IpcManager
    {
        // in: Original text, context, target language
        // out: Task (translated text)
        private static ICallGateProvider<string, string?, string, Task<string>>? CallGateTranslate;

        public static void Register(IDalamudPluginInterface pi)
        {
            Unregister();

            CallGateTranslate = pi.GetIpcProvider<string, string?, string, Task<string>>("Echo.Translate");
            CallGateTranslate.RegisterFunc(IpcTranslateText);
        }

        private static async Task<string> IpcTranslateText(string originalText, string? context, string toLangCode)
        {
            LanguageInfo toLang = GetLanguage(toLangCode);
            Message IpcMessage = new Message("IPC", Dalamud.Game.Text.XivChatType.Debug, originalText, toLang)
            {
                Context = context
            };

            return await Service.translationHandler.TranslateChat(IpcMessage);
        }

        public static void Unregister()
        {
            CallGateTranslate?.UnregisterFunc();
            CallGateTranslate = null;
        }
    }
}
