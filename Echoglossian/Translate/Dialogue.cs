using Echoglossian.Utils;
using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian.Translate
{
    internal class Dialogue(string source, LanguageInfo? sourceLanguage, LanguageInfo? targetLanguage, string content)
    {
        public string Source { get; } = source;
        public LanguageInfo? SourceLanguage { get; } = sourceLanguage;
        public LanguageInfo TargetLanguage { get; } = targetLanguage ?? Service.config.SelectedTargetLanguage;
        public string Content { get; } = content;

        public override string ToString()
        {
            return $"{Source}\\{SourceLanguage?.Code}\\{TargetLanguage.Code}\\{Content}";
        }
    }
}
