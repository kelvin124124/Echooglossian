using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Echoglossian.Utils
{
    public static class LanguageDictionary
    {
        private static readonly (string Code, string Name, string Font)[] LangArr =
        [
            ("af", "Afrikaans", "NotoSans-Medium.ttf"),
            ("sq", "Shqip; Albanian", "NotoSans-Medium.ttf"),
            ("ar", "العَرَبِيَّة Al'Arabiyyeẗ; Arabic", "NotoSansArabic-Medium.ttf"),
            // ... other languages
        ];

        public readonly record struct LanguageInfo
        {
            public string Code { get; init; }
            public string Name { get; init; }
            public string Font { get; init; }

            public string ExclusiveChars => LoadCharacterSet(Code);

            public static explicit operator CultureInfo(LanguageInfo v)
            {
                return new CultureInfo(v.Code);
            }
        }

        public static LanguageInfo[] GetLanguages() =>
            [.. LangArr.Select(x => new LanguageInfo { Code = x.Code, Name = x.Name, Font = x.Font })];

        public static LanguageInfo GetLanguage(string code)
        {
            var lang = LangArr.FirstOrDefault(x => x.Code == code);
            if (lang == default)
                throw new KeyNotFoundException($"Language code '{code}' not found.");

            return new LanguageInfo
            {
                Code = lang.Code,
                Name = lang.Name,
                Font = lang.Font
            };
        }

        private static string LoadCharacterSet(string code) =>
            code switch
            {
                "ar" => CharacterSets.Arabic,
                // ... other languages
                _ => string.Empty
            };
    }
}
