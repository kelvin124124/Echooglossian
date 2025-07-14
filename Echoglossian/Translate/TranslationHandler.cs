using Dalamud.Networking.Http;
using Echoglossian.Chat;
using Echoglossian.Utils;
using LanguageDetection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Echoglossian.Utils.LanguageDictionary;

namespace Echoglossian.Translate
{
    internal class TranslationHandler : IDisposable
    {
        private static readonly Dictionary<string, string> ChatTranslationCache = [];

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

        private readonly LanguageDetector detector = new();

        public async Task<string> TranslateUI(Dialogue dialogue)
        {
            if (Service.config.UseLLMTranslation) 
            {
                var context = GetDialogueContext(dialogue);

                var translation = await OpenAITranslator.TranslateAsync(dialogue.Content, dialogue.TargetLanguage.Code, context).ConfigureAwait(false);
            }
        }

        public async Task<string> TranslateChat(Message chatMessage)
        {
            return "debug";
        }

        public async Task<string> TranslateString(string content, LanguageInfo toLanguage)
        {
            return "debug";
        }

        public async Task<LanguageInfo> DetermineLanguage(string content)
        {
            return GetLanguage(detector.Detect(content));
        }

        public void WipeCache() => ChatTranslationCache.Clear();

        public void Dispose()
        {
            this.WipeCache();
            HttpClient.Dispose();
        }
    }
}
