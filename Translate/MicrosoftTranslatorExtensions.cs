using GTranslate.Translators;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace Echoglossian.Translate
{
    internal static class MicrosoftTranslatorExtensions
    {
        public static async Task<TranslationResult> TranslateWithKeyAsync(
            this MicrosoftTranslator translator, string text, string toLanguage)
        {
            string DefaultContentType = "application/json";
            string baseUrl = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0";

            var requestData = new[]
            {
                new { Text = text }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, DefaultContentType),
                Headers =
                {
                    { "Ocp-Apim-Subscription-Key", secretKey }
                }
            };

            var response = await TranslationHandler.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var jsonArray = JArray.Parse(jsonResponse);
            var translatedText = jsonArray[0]?["translations"]?[0]?["text"]?.ToString();
            var detectedLanguage = jsonArray[0]?["detectedLanguage"]?["language"]?.ToString();

            if (translatedText == text)
            {
                throw new Exception("MicrosoftTranslator (with API key): Message was not translated.");
            }

            return new TranslationResult(
                translation: translatedText,
                source: text,
                service: nameof(MicrosoftTranslator),
                srcLangCode: detectedLanguage,
                targetLangCode: toLanguage
            );
        }
    }
}
