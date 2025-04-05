using Echoglossian.Translate;
using FASTER.core;

namespace Echoglossian.Database
{
    internal class TranslationCache : IDisposable
    {
        private readonly FasterKV<string, string> store;
        private readonly ClientSession<string, string, string, string, Empty, SimpleFunctions<string, string>> session;

        public TranslationCache(string dbPath)
        {
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
            var status = session.Read(dialogue.ToString(), out value);
            return status.Found;
        }

        // proposed cache key: "{UI_source}\{srcLang}\{targetLang}\{originalContent}"
        public void Upsert(Dialogue dialogue, string value)
        {
            session.Upsert(dialogue.ToString(), value);
        }

        public void Dispose()
        {
            session?.Dispose();
            store?.Dispose();
        }
    }
}
