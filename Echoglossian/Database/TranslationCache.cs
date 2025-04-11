using Echoglossian.Translate;
using Echoglossian.Utils;
using FASTER.core;
using System;
using System.IO;

namespace Echoglossian.Database
{
    internal class TranslationCache : IDisposable
    {
        private readonly FasterKV<string, string> store;
        private readonly ClientSession<string, string, string, string, Empty, SimpleFunctions<string, string>> session;

        public TranslationCache()
        {
            string dbPath = Path.Combine(Service.pluginInterface.AssemblyLocation.Directory?.FullName!, "FASTER");

            // 160 MB memory used for the cache
            var settings = new FasterKVSettings<string, string>(dbPath)
            {
                IndexSize = 32L << 20,  // 32MB
                MemorySize = 96L << 20,  // 96MB

                PageSize = 4L << 20,  // 4MB
                SegmentSize = 32L << 20,  // 32MB
                MutableFraction = 0.95,

                ReadCacheEnabled = true,
                ReadCacheMemorySize = 32L << 20,  // 64MB
                ReadCachePageSize = 2L << 20,  // 2MB
                ReadCacheSecondChanceFraction = 0.2,
            };
            store = new FasterKV<string, string>(settings);
            session = store.For(new SimpleFunctions<string, string>()).NewSession<SimpleFunctions<string, string>>();
        }

        public bool TryGet(Dialogue dialogue, out string value)
        {
            return TryGetString(dialogue.ToString(), out value);
        }

        // proposed cache key: "{UI_source}\{srcLang}\{targetLang}\{originalContent}"
        public void Upsert(Dialogue dialogue, string value)
        {
            UpsertString(dialogue.ToString(), value);
        }

        public bool TryGetString(string key, out string value)
        {
            var status = session.Read(key, out value);
            return status.Found;
        }

        public void UpsertString(string key, string value)
        {
            session.Upsert(key, value);
        }

        public void WipeCache()
        {
            session.Dispose();
            store.Dispose();

            string dbPath = Path.Combine(Service.pluginInterface.AssemblyLocation.Directory?.FullName!, "FASTER");
            if (Directory.Exists(dbPath))
            {
                Directory.Delete(dbPath, true);
            }

            Service.translationCache = new TranslationCache();
        }

        public void Dispose()
        {
            session?.Dispose();
            store?.Dispose();
        }
    }
}
