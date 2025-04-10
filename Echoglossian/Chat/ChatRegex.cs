using System.Text.RegularExpressions;

namespace Echoglossian.Chat
{
    internal static partial class ChatRegex
    {
        [GeneratedRegex(@"\uE040\u0020(.*?)\u0020\uE041")]
        public static partial Regex AutoTranslateRegex();

        [GeneratedRegex(@"^よろしくお(願|ねが)いします[\u3002\uFF01!]*", RegexOptions.Compiled)]
        public static partial Regex JPWelcomeRegex();

        [GeneratedRegex(@"^お疲れ様でした[\u3002\uFF01!]*", RegexOptions.Compiled)]
        public static partial Regex JPByeRegex();

        [GeneratedRegex(@"\b(どまい?|ドマ|どんまい)(です)?[\u3002\uFF01!]*\b", RegexOptions.Compiled)]
        public static partial Regex JPDomaRegex();
    }
}
