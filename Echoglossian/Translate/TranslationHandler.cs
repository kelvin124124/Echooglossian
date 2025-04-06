using Echoglossian.Database;
using Echoglossian.Utils;

namespace Echoglossian.Translate
{
    internal class TranslationHandler
    {
        private readonly TranslationCache translationCache = new(Path.Combine(Service.pluginInterface.AssemblyLocation.Directory?.FullName!, "faster"));

        internal static HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
    }
}
