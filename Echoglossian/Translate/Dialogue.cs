using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian.Translate
{
    internal class Dialogue(string uiSource, LanguageInfo sourceLanguage, LanguageInfo targetLanguage, string content)
    {
        public string UISource { get; } = uiSource;
        public LanguageInfo SourceLanguage { get; } = sourceLanguage;
        public LanguageInfo TargetLanguage { get; } = targetLanguage;

        public string Content { get; } = content;

        public override string ToString()
        {
            return $"{UISource}\\{SourceLanguage.Code}\\{TargetLanguage.Code}\\{Content}";
        }
    }
}
