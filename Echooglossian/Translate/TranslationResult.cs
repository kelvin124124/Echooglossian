using GTranslate;
using GTranslate.Results;

namespace Echooglossian.Translate
{
    internal class TranslationResult(
        string translation, string source, string service, string srcLangCode, string targetLangCode) : ITranslationResult
    {
        public string Translation { get; } = translation;
        public string Source { get; } = source;
        public string Service { get; } = service;
        public ILanguage SourceLanguage { get; } =
            srcLangCode == string.Empty ? null! : Language.GetLanguage(srcLangCode);
        public ILanguage TargetLanguage { get; } =
            targetLangCode == string.Empty ? null! : Language.GetLanguage(targetLangCode);
    }
}
