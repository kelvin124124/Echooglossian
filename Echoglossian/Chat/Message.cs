using Dalamud.Game.Text;
using Echoglossian.Translate;
using System.Globalization;

namespace Echoglossian.Chat
{
    internal class Message(string content, CultureInfo targetLanguage) : Dialogue
        ("chat", null!, targetLanguage, content)
    {
        XivChatType type { get; set; } = XivChatType.Say;
    }
}
