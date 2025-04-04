using GTranslate;
using GTranslate.Translators;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Echoglossian.Translate
{
    internal static class GTranslatorExtensions
    {
        public static async Task<TranslationResult> TranslateWithKeyAsync(
            this GoogleTranslator translator, string text, string toLanguage)
        {
            string DefaultContentType = "application/json";
            string baseUrl = "https://translation.googleapis.com/language/translate/v2";

            var requestData = new
            {
                q = new[] { text },
                target = Language.GetLanguage(toLanguage).ISO6391,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, DefaultContentType),
                Headers =
                    {
                        { "x-goog-user-project", "projectID" },
                        { HttpRequestHeader.Authorization.ToString(), $"Bearer {accessToken}" },
                    }
            };

            var response = await TranslationHandler.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var jsonObject = JObject.Parse(jsonResponse);
            var translatedText = jsonObject["data"]?["translations"]?[0]?["translatedText"]?.ToString();
            var detectedSourceLanguage = jsonObject["data"]?["translations"]?[0]?["detectedSourceLanguage"]?.ToString();

            if (translatedText == text)
            {
                throw new Exception("GTranslator (with API key): Message was not translated.");
            }

            return new TranslationResult(
                translation: translatedText,
                source: text,
                service: nameof(GoogleTranslator),
                srcLangCode: detectedSourceLanguage,
                targetLangCode: toLanguage
            );
        }
    }
}
