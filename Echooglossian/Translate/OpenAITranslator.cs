using Dalamud.Utility;
using Echooglossian.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Echooglossian.Translate
{
    internal static class OpenAITranslator
    {
        private const string DefaultContentType = "application/json";

        public static async Task<string> Translate(Dialogue dialogue, LLMPreset llm)
        {
            string content = dialogue.Content;
            string targetLanguage = dialogue.TargetLanguage.Code;

            if (!KeyValidator.IsValidAPIKey(llm.Name, out string apiKey))
            {
                Service.pluginLog.Warning("LLM API Key is invalid. Please check your configuration. Falling back to machine translation.");
                return await MachineTranslator.Translate(dialogue);
            }

            var prompt = BuildPrompt(targetLanguage, Service.config.CHAT_UseContext ? GetContext() : null);

            var promptLength = prompt.Length;
            var userMsg = $"Translate to: {targetLanguage}\n#### Original Text\n{content}";
            var requestData = new
            {
                llm.Model,
                llm.Temperature,
                max_tokens = Math.Max(promptLength, 200),
                messages = new[]
                {
                    new { role = "system", content = prompt },
                    new { role = "user", content = userMsg }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, llm.LLM_API_Endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, DefaultContentType),
                Headers = { { HttpRequestHeader.Authorization.ToString(), $"Bearer {apiKey}" } }
            };

            try
            {
                var response = await TranslationHandler.HttpClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var translated = JObject.Parse(jsonResponse)["choices"]?[0]?["message"]?["content"]?.ToString().Trim();

                if (translated.IsNullOrWhitespace())
                {
                    throw new Exception("Translation not found in the expected structure.");
                }

                if (translated == content)
                {
                    Service.pluginLog.Warning("Message was not translated. Falling back to machine translate.");
                    return await MachineTranslator.Translate(dialogue);
                }

                var translationMatch = Regex.Match(translated, @"#### Translation\s*\n(.+)$", RegexOptions.Singleline);
                translated = translationMatch.Success ? translationMatch.Groups[1].Value.Trim() : translated;

                return translated;
            }
            catch (Exception ex)
            {
                Service.pluginLog.Warning($"OpenAI Translate failed to translate. Falling back to machine translation.\n{ex.Message}");
                return await MachineTranslator.Translate(dialogue);
            }
        }

        public static string BuildPrompt(string toLanguage, string? context)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"You are a precise translator for FFXIV game content into {toLanguage}.\n");

            sb.AppendLine("TRANSLATION RULES:");
            sb.AppendLine("1. Be mindful of FFXIV-specific terms, but translate all content appropriately");
            sb.AppendLine("2. Preserve all formatting, including spaces and punctuation");
            sb.AppendLine("3. Maintain the exact meaning and tone of the original text\n");

            sb.AppendLine("OUTPUT RULES:");
            sb.AppendLine("1. First, in a \"#### Reasoning\" section, briefly:");
            sb.AppendLine("   - Identify any FFXIV-specific terms and their meanings");
            sb.AppendLine("   - Consider multiple possible translations");
            sb.AppendLine("   - Explain your final translation choice");
            sb.AppendLine("2. Your response must then include \"#### Translation\".");
            sb.AppendLine("3. Write only the translated text after this header.");
            sb.AppendLine("4. Do not include the original text.");
            sb.AppendLine("5. Do not add any explanations or notes after the translation.\n");

            sb.AppendLine("Example response format:");
            sb.AppendLine("#### Reasoning");
            sb.AppendLine("{Your analysis and translation process}");
            sb.AppendLine("");
            sb.AppendLine("#### Translation");
            sb.AppendLine("{Only the translated text goes here}");

            if (context != null)
            {
                sb.AppendLine("\nCONTEXT:");
                sb.AppendLine("Use the following context information if relevant (provided in XML tags):");
                sb.AppendLine("<context>");
                sb.AppendLine(context);
                sb.AppendLine("</context>");
            }

            return sb.ToString();
        }

        private static string GetContext()
        {
            throw new NotImplementedException();
        }
    }
}
