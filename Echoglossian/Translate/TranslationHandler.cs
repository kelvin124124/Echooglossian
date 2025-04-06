using Dalamud.Networking.Http;
using Echoglossian.Database;
using Echoglossian.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Echoglossian.Translate
{
    internal class TranslationHandler
    {
        private readonly TranslationCache translationCache = new(Path.Combine(Service.pluginInterface.AssemblyLocation.Directory?.FullName!, "faster"));

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
    }
}
