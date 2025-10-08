using Dalamud.Networking.Http;
using Echooglossian.Chat;
using FastPersistentDictionary;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Echooglossian.Utils.LanguageDictionary;

namespace Echooglossian.Translate
{
    internal class TranslationHandler : IDisposable
    {
        public static readonly Dictionary<string, string> ChatTranslationCache = [];
        public static readonly FastPersistentDictionary<Dialogue, string> DialogueTranslationCache = [];

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
            return "debug";

            //if (Service.config.UseLLMTranslation) 
            //{
            //    var context = GetDialogueContext(dialogue);

            //    var translation = await OpenAITranslator.TranslateAsync(dialogue.Content, dialogue.TargetLanguage.Code, context).ConfigureAwait(false);
            //}
        }

        public async Task<string> TranslateChat(Message chatMessage)
        {
            return "debug";
        }

        public async Task<string> TranslateString(string content, LanguageInfo toLanguage)
        {
            return "debug";
        }

        public async Task<string> TranslateName(string content, LanguageInfo toLanguage)
        {
            return "debug";
        }

        public async Task<LanguageInfo> DetermineLanguage(string text) 
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            ChatTranslationCache.Clear();
            HttpClient.Dispose();
        }
    }
}
