using System.Globalization;

namespace Echoglossian.Translate
{
    internal class Dialogue(string uiSource, CultureInfo sourceLanguage, CultureInfo targetLanguage, string content)
    {
        public string UISource { get; } = uiSource;
        public CultureInfo SourceLanguage { get; } = sourceLanguage;
        public CultureInfo TargetLanguage { get; } = targetLanguage;

        public string Content { get; } = content;

        public override string ToString()
        {
            return $"{UISource}\\{SourceLanguage.TwoLetterISOLanguageName}\\{TargetLanguage.TwoLetterISOLanguageName}\\{Content}";
        }
    }
}
