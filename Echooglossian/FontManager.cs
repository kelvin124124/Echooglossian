using Dalamud.Interface.ManagedFontAtlas;
using Echooglossian.Utils;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using static Echooglossian.Utils.LanguageDictionary;

namespace Echooglossian
{
    public class FontManager : IDisposable
    {
        private readonly byte[] fontData;

        public IFontHandle GeneralFontHandle { get; private set; }
        public IFontHandle LanguageFontHandle { get; private set; }

        public FontManager()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Echooglossian.GoNotoKurrent-Regular.ttf.gz");
            using var gzip = new GZipStream(stream, CompressionMode.Decompress);
            using var memory = new MemoryStream();

            gzip.CopyTo(memory);
            fontData = memory.ToArray();

            GeneralFontHandle = Service.pluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(e =>
            {
                e.OnPreBuild(tk =>
                {
                    tk.AddFontFromMemory(fontData, new SafeFontConfig
                    {
                        SizePx = 16.0f,
                        GlyphRanges = ConfigUIGlyphRanges
                    }, "Echo.GeneralFontHandle");
                });
            });

            BuildLanguageFont(Service.configuration.SelectedTargetLanguage);
        }

        public void BuildLanguageFont(LanguageInfo targetLanguage)
        {
            LanguageFontHandle?.Dispose();

            LanguageFontHandle = Service.pluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(e =>
            {
                e.OnPreBuild(tk =>
                {
                    tk.AddFontFromMemory(fontData, new SafeFontConfig
                    {
                        SizePx = 16.0f,
                        GlyphRanges = GetGlyphRanges(targetLanguage)
                    }, "Echo.LanguageFontHandle");
                });
            });
        }

        /// <summary>
        /// Placeholder: Returns null to load all glyphs.
        /// TODO: Implement language-specific glyph ranges for font subsetting.
        /// </summary>
        private ushort[] GetGlyphRanges(LanguageInfo language) => null;
        private ushort[] ConfigUIGlyphRanges = null; // TODO: Define glyph ranges for UI elements.

        public void Dispose()
        {
            GeneralFontHandle?.Dispose();
            LanguageFontHandle?.Dispose();
        }
    }
}
