using Dalamud.Game.Text;
using Echoglossian.Translate;
using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian.Chat
{
    internal class Message(string sender, XivChatType type, string content, LanguageInfo? targetLanguage = null) : Dialogue
        ("chat", null, targetLanguage, content)
    {
        public string Sender { get; set; } = sender;
        public XivChatType Type { get; set; } = type;

        public string Context { get; set; } = string.Empty;
    }
}
