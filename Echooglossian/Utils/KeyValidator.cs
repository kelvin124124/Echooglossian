using Dalamud.Utility;
using Echooglossian.Translate;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Echooglossian.Utils
{
    internal static class KeyValidator
    {
        public static bool APIKeyExists(string service, out string validatedKey)
        {
            if (Service.configuration.API_Keys.TryGetValue(service, out string? apiKey) && !apiKey.IsNullOrWhitespace())
            {
                validatedKey = apiKey;
                return true;
            }

            validatedKey = string.Empty;
            return false;
        }

        public static bool IsValidLLMKey(string service, out string validatedKey)
        {
            if (Service.configuration.API_Keys.TryGetValue(service, out string? apiKey) && !apiKey.IsNullOrWhitespace())
            {
                throw new NotImplementedException();
            }

            validatedKey = string.Empty;
            return false;
        }

        private static async Task<bool> ValidateLLMKey(string apiKey, string endpoint)
        {
            endpoint = endpoint.TrimEnd('/').Replace("/chat/completions", "/models");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint)
                {
                    Headers = { { "Authorization", $"Bearer {apiKey}" } }
                };

                var response = await TranslationHandler.HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                Service.pluginLog.Information($"LLM Key validation successful.");
                return true;
            }
            catch
            {
                Service.pluginLog.Warning($"API Key validation failed.");
            }

            return false;
        }
    }
}
