namespace Echoglossian.Translate
{
    internal class TranslationHandler
    {
        private readonly Dictionary<string, string> translationCache = [];

        internal static HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
    }
}
