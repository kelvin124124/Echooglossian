using GTranslate.Translators;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Echoglossian.Translate
{
    internal static class YandexTranslatorExtensions
    {
        public static async Task<TranslationResult> TranslateWithKeyAsync(
            this YandexTranslator translator, string text, string toLanguage)
        {
            string DefaultContentType = "application/json";
            string baseUrl = "https://translate.api.cloud.yandex.net/translate/v2/translate";

            var requestData = new
            {
                texts = new[] { text },
                targetLanguageCode = toLanguage,
                format = "PLAIN_TEXT"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, DefaultContentType),
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {IAM_Token}" }
                }
            };

            var response = await TranslationHandler.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var jsonObject = JObject.Parse(jsonResponse);
            var translatedText = jsonObject["translations"]?[0]?["text"]?.ToString();
            var detectedLanguage = jsonObject["translations"]?[0]?["detectedLanguageCode"]?.ToString();

            if (translatedText == text)
            {
                throw new Exception("YandexTranslator (with API key): Message was not translated.");
            }

            return new TranslationResult(
                translation: translatedText,
                source: text,
                service: nameof(YandexTranslator),
                srcLangCode: detectedLanguage,
                targetLangCode: toLanguage
            );
        }
    }
}
