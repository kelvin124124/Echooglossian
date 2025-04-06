using Dalamud.Utility;

namespace Echoglossian.Utils
{
    internal static class KeyValidator
    {
        public static bool IsValidAPIKey(string service, out string validatedKey)
        {
            if (Service.config.API_Keys.TryGetValue(service, out string? apiKey) && !apiKey.IsNullOrWhitespace())
            {
                validatedKey = apiKey;
                return true;
            }

            validatedKey = string.Empty;
            return false;
        }
    }
}
