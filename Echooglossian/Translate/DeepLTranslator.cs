using Dalamud.Utility;
using Echooglossian.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Echooglossian.Translate
{
    internal class DeepLTranslator
    {
        internal static HttpClient HttpClient { get; set; } = null!;
        private const string DefaultContentType = "application/json";

        public string Name => nameof(DeepLTranslator);

        private Random random = null!;

        public DeepLTranslator(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<TranslationResult> TranslateAsync(string text, string toLanguage)
        {
            random ??= new Random();

            const string BaseUrl = "https://www2.deepl.com/jsonrpc";

            if (!TryGetLanguageCode(toLanguage, out var languageCode))
            {
                throw new Exception("Target language not supported by DeepL.");
            }

            var id = ((ulong)random.Next(8300000, 8400000) * 1000) + 1;
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var iCount = text.Count(c => c == 'i');
            var adjustedTimestamp = iCount == 0 ? timestamp : timestamp - (timestamp % (iCount + 1)) + iCount + 1;

            var requestBody = new
            {
                jsonrpc = "2.0",
                method = "LMT_handle_jobs",
                @params = new
                {
                    commonJobParams = new
                    {
                        mode = "translate",
                        formality = "undefined",
                        transcribe_as = "romanize",
                        advancedMode = false,
                        textType = "plaintext",
                        wasSpoken = false,
                        regionalVariant = toLanguage switch
                        {
                            "Chinese (Simplified)" => "ZH-HANS",
                            "Chinese (Traditional)" => "ZH-HANT",
                            _ => default
                        }
                    },
                    lang = new
                    {
                        source_lang_user_selected = "auto",
                        target_lang = languageCode,
                        source_lang_computed = "AUTO",
                    },
                    jobs = new[]
                    {
                        new
                        {
                            kind = "default",
                            preferred_num_beams = 4,
                            raw_en_context_before = Array.Empty<string>(),
                            raw_en_context_after = Array.Empty<string>(),
                            sentences = new[]
                            {
                                new { prefix = "", text, id = 0 }
                            }
                        }
                    },
                    timestamp = adjustedTimestamp
                },
                id
            };

            var postDataJson = JsonSerializer.Serialize(requestBody);
            postDataJson = postDataJson.Replace("\"method\":\"", (id + 5) % 29 == 0 || (id + 3) % 13 == 0 ? "\"method\" : \"" : "\"method\": \"");

            using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
            {
                Content = new StringContent(postDataJson, Encoding.UTF8, "application/json")
            };

            SetHeaders(request);

            var response = await TranslationHandler.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var contentEncoding = response.Content.Headers.ContentEncoding;
            if (contentEncoding.Contains("gzip", StringComparer.OrdinalIgnoreCase))
            {
                using var gzipStream = new System.IO.Compression.GZipStream(responseStream, System.IO.Compression.CompressionMode.Decompress);
                using var streamReader = new System.IO.StreamReader(gzipStream, Encoding.UTF8);
                postDataJson = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
            else if (contentEncoding.Contains("deflate", StringComparer.OrdinalIgnoreCase))
            {
                using var deflateStream = new System.IO.Compression.DeflateStream(responseStream, System.IO.Compression.CompressionMode.Decompress);
                using var streamReader = new System.IO.StreamReader(deflateStream, Encoding.UTF8);
                postDataJson = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
            else if (contentEncoding.Contains("br", StringComparer.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Brotli encoding is not supported.");
            }
            else
            {
                postDataJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }


            using var jsonDoc = JsonDocument.Parse(postDataJson);
            var translated = jsonDoc.RootElement
                .GetProperty("result")
                .GetProperty("translations")[0]
                .GetProperty("beams")[0]
                .GetProperty("sentences")[0]
                .GetProperty("text")
                .GetString();

            if (translated.IsNullOrWhitespace())
            {
                throw new Exception("Translation not found in the expected JSON structure.");
            }

            if (translated == text)
            {
                throw new Exception("Translation is the same as the original text.");
            }

            return new TranslationResult(
                translation: translated,
                source: text,
                service: nameof(DeepLTranslator),
                srcLangCode: string.Empty,
                targetLangCode: string.Empty
            );
        }

        public async Task<TranslationResult> TranslateWithKeyAsync(string text, string toLanguage)
        {
            if (!KeyValidator.APIKeyExists(nameof(DeepLTranslator), out string authKey))
            {
                throw new Exception("Translate with key is enabled but key is not set.");
            }

            if (!TryGetLanguageCode(toLanguage, out var languageCode))
            {
                throw new Exception("Target language not supported by DeepL.");
            }

            var requestBody = new { text = new[] { text }, target_lang = languageCode, context = "FFXIV, MMORPG" };
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api-free.deepl.com/v2/translate")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, DefaultContentType),
                Headers = { { HttpRequestHeader.Authorization.ToString(), $"DeepL-Auth-Key {authKey}" } }
            };

            var response = await TranslationHandler.HttpClient.SendAsync(requestMessage).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var translated = JObject.Parse(jsonResponse)["translations"]?[0]?["text"]?.ToString().Trim();

            if (translated.IsNullOrWhitespace())
            {
                throw new Exception("Translation not found in the expected JSON structure.");
            }

            return new TranslationResult(
                translation: translated,
                source: text,
                service: nameof(DeepLTranslator),
                srcLangCode: string.Empty,
                targetLangCode: toLanguage
            );
        }

        private static bool TryGetLanguageCode(string language, out string? languageCode)
        {
            languageCode = language switch
            {
                "English" => "EN",
                "Japanese" => "JA",
                "German" => "DE",
                "French" => "FR",
                "Chinese (Simplified)" => "ZH",
                "Chinese (Traditional)" => "ZH",
                "Korean" => "KO",
                "Spanish" => "ES",
                "Arabic" => "AR",
                "Bulgarian" => "BG",
                "Czech" => "CS",
                "Danish" => "DA",
                "Dutch" => "NL",
                "Estonian" => "ET",
                "Finnish" => "FI",
                "Greek" => "EL",
                "Hungarian" => "HU",
                "Indonesian" => "ID",
                "Italian" => "IT",
                "Latvian" => "LV",
                "Lithuanian" => "LT",
                "Norwegian Bokmal" => "NB",
                "Polish" => "PL",
                "Portuguese" => "PT",
                "Romanian" => "RO",
                "Russian" => "RU",
                "Slovak" => "SK",
                "Slovenian" => "SL",
                "Swedish" => "SV",
                "Turkish" => "TR",
                "Ukrainian" => "UK",
                _ => null
            };

            return !string.IsNullOrEmpty(languageCode);
        }

        private static void SetHeaders(HttpRequestMessage request)
        {
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var headers = new Dictionary<string, string>
            {
                { "Accept-Language", "en-US,en;q=0.9" },
                { "User-Agent", "DeepL/1627620 CFNetwork/3826.500.62.2.1 Darwin/24.4.0" },
                { "Content-Type", "application/json" },
                { "X-App-Os-Name", "iOS" },
                { "X-App-Os-Version", "18.4.0" },
                { "Accept-Encoding", "gzip, deflate" }, // br is removed for simplicity
                { "X-App-Device", "iPhone16,2" },
                { "X-Product", "translator" },
                { "X-App-Build", "1627620" },
                { "X-App-Version", "25.1" }
            };

            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }
}
