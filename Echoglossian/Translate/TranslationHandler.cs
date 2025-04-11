using Dalamud.Networking.Http;
using Echoglossian.Chat;
using Echoglossian.Database;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian.Translate
{
    internal class TranslationHandler : IDisposable
    {
        private readonly TranslationCache translationCache = new();

        internal static readonly HttpClient HttpClient = new(new SocketsHttpHandler
        {
            ConnectCallback = new HappyEyeballsCallback().ConnectCallback,
            AutomaticDecompression = DecompressionMethods.All
        })
        {
            DefaultRequestVersion = HttpVersion.Version30,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
            Timeout = TimeSpan.FromSeconds(20)
        };

        public async Task<string> TranslateUI(Dialogue dialogue)
        {
            throw new NotImplementedException();
        }

        public async Task<string> TranslateChat(Message chatMessage)
        {
            throw new NotImplementedException();
        }

        public async Task<string> TranslateString(string content, LanguageInfo toLanguage)
        {
            throw new NotImplementedException();
        }

        public async Task<LanguageInfo> DetermineLanguage(string content)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
