using Echoglossian.Utils;
using GTranslate.Translators;

namespace Echoglossian.Translate
{
    internal static class MachineTranslator
    {
        // DeepL Translator
        private static readonly Lazy<DeepLTranslator> LazyDeepLTranslator = new(() => new DeepLTranslator(TranslationHandler.HttpClient));
        public static DeepLTranslator DeepLTranslator => LazyDeepLTranslator.Value;

        // GTrasnlate Translators
        private static readonly Lazy<GoogleTranslator> LazyGTranslator = new(() => new GoogleTranslator(TranslationHandler.HttpClient));
        private static readonly Lazy<MicrosoftTranslator> LazyMicrosoftTranslator = new(() => new MicrosoftTranslator(TranslationHandler.HttpClient));
        private static readonly Lazy<YandexTranslator> LazyYandexTranslator = new(() => new YandexTranslator(TranslationHandler.HttpClient));
        public static GoogleTranslator GTranslator => LazyGTranslator.Value;
        public static MicrosoftTranslator MicrosoftTranslator => LazyMicrosoftTranslator.Value;
        public static YandexTranslator YandexTranslator => LazyYandexTranslator.Value;

        public static async Task<string> Translate(string text, string targetLanguage)
        {
            // Fallback sequence
            // TODO: make it customizable
            foreach (var translator in new dynamic[]
            {
                DeepLTranslator,
                MicrosoftTranslator,
                GTranslator,
                YandexTranslator
            })
            {
                try
                {
                    var result = await translator.TranslateAsync(text, targetLanguage).ConfigureAwait(false);
                    string resultText = result.Translation;

                    if (string.IsNullOrWhiteSpace(resultText) || resultText == text)
                        throw new Exception($"{translator.Name} Translate returned an invalid translation.");

                    return resultText;
                }
                catch (Exception ex)
                {
                    Service.pluginLog.Warning($"{translator.Name} failed to translate dialogue.\n{ex.Message}");
                }
            }

            return text;
        }

        public static async Task<string> Translate(Dialogue dialogue)
        {
            return await Translate(dialogue.Content, dialogue.TargetLanguage.EnglishName).ConfigureAwait(false);
        }
    }
}
