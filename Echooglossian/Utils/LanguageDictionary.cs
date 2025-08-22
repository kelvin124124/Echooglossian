using Dalamud.Game;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Echooglossian.Utils
{
    public static class LanguageDictionary
    {
        private static readonly (string Code, string Name, string Font)[] LangArr =
        [
            // Popular
            ("en", "English", "NotoSans-Medium.ttf"),
            ("zh-CN", "簡體中文; Chinese Simplified", "NotoSansCJKsc-Regular.otf"),
            ("zh-TW", "繁體中文; Chinese Traditional", "NotoSansCJKtc-Regular.otf"),
            ("hi", "हिन्दी Hindī; Hindi", "NotoSansDevanagari-Medium.ttf"),
            ("es", "Español; Castellano; Spanish; Castilian", "NotoSans-Medium.ttf"),
            ("fr", "Français; French", "NotoSans-Medium.ttf"),
            ("ar", "العَرَبِيَّة Al'Arabiyyeẗ; Arabic", "NotoSansArabic-Medium.ttf"),
            ("pt", "Português Brasileiro; Brazilian Portuguese", "NotoSans-Medium.ttf"),
            ("pt-PT", "Português; Portuguese", "NotoSans-Medium.ttf"),
            ("bn", "বাংলা Bāŋlā; Bengali", "NotoSansBengali-Medium.ttf"),
            ("ru", "Русский Язык Russkiĭ Âzık; Russian", "NotoSans-Medium.ttf"),
            ("de", "Deutsch; German", "NotoSans-Medium.ttf"),
            ("ja", "日本語 Nihongo; Japanese", "NotoSansCJKjp-Regular.otf"),

            // Europe
            ("it", "Italiano; Lingua Italiana; Italian", "NotoSans-Medium.ttf"),
            ("ro", "Limba Română; Romanian; Moldavian; Moldovan", "NotoSans-Medium.ttf"),
            ("ca", "Català; Valencià; Catalan; Valencian", "NotoSans-Medium.ttf"),
            ("nl", "Nederlands; Vlaams; Dutch; Flemish", "NotoSans-Medium.ttf"),
            ("sv", "Svenska; Swedish", "NotoSans-Medium.ttf"),
            ("da", "Dansk; Danish", "NotoSans-Medium.ttf"),
            ("no", "Norsk; Norwegian", "NotoSans-Medium.ttf"),
            ("pl", "Język Polski; Polish", "NotoSans-Medium.ttf"),
            ("uk", "Українська Мова; Українська; Ukrainian", "NotoSans-Medium.ttf"),
            ("cs", "Čeština; Český Jazyk; Czech", "NotoSans-Medium.ttf"),
            ("sk", "Slovenčina; Slovenský Jazyk; Slovak", "NotoSans-Medium.ttf"),
            ("bg", "Български Език Bălgarski Ezik; Bulgarian", "NotoSans-Medium.ttf"),
            ("sr", "Српски ; Srpski; Serbian", "NotoSans-Medium.ttf"),
            ("hr", "Hrvatski; Croatian", "NotoSans-Medium.ttf"),
            ("bs", "Bosanski; Bosnian", "NotoSans-Medium.ttf"),
            ("sl", "Slovenski Jezik; Slovenščina; Slovenian", "NotoSans-Medium.ttf"),
            ("mk", "Македонски Јазик Makedonski Jazik; Macedonian", "NotoSans-Medium.ttf"),
            ("be", "Беларуская Мова Belaruskaâ Mova; Belarusian", "NotoSans-Medium.ttf"),
            ("el", "Νέα Ελληνικά Néa Ellêniká; Greek Modern (1453-)", "NotoSans-Medium.ttf"),
            ("hu", "Magyar Nyelv; Hungarian", "NotoSans-Medium.ttf"),
            ("fi", "Suomen Kieli; Finnish", "NotoSans-Medium.ttf"),
            ("sq", "Shqip; Albanian", "NotoSans-Medium.ttf"),
            ("lt", "Lietuvių Kalba; Lithuanian", "NotoSans-Medium.ttf"),
            ("lv", "Latviešu Valoda; Latvian", "NotoSans-Medium.ttf"),
            ("et", "Eesti Keel; Estonian", "NotoSans-Medium.ttf"),
            ("is", "Íslenska; Icelandic", "NotoSans-Medium.ttf"),
            ("ga", "Gaeilge; Irish", "NotoSans-Medium.ttf"),
            ("cy", "Cymraeg; Y Gymraeg; Welsh", "NotoSans-Medium.ttf"),
            ("yi", "ייִדיש; יידיש; אידיש Yidiš; Yiddish", "NotoSansHebrew-Medium.ttf"),

            // Asia
            ("ko", "한국어 Han'Gug'Ô; Korean", "NotoSansCJKkr-Regular.otf"),
            ("mn", "Монгол Хэл Mongol Xel; ᠮᠣᠩᠭᠣᠯ ᠬᠡᠯᠡ; Mongolian", "NotoSansMongolian-Regular.ttf"),
            ("id", "Bahasa Indonesia; Indonesian", "NotoSans-Medium.ttf"),
            ("vi", "TiếNg ViệT; Vietnamese", "NotoSans-Medium.ttf"),
            ("th", "ภาษาไทย Phasathay; Thai", "NotoSansThai-Medium.ttf"),
            ("ms", "Bahasa Melayu; Malay", "NotoSans-Medium.ttf"),
            ("tl", "Wikang Tagalog; Tagalog", "NotoSans-Regular.ttf"),
            ("my", "မြန်မာစာ Mrãmācā; မြန်မာစကား Mrãmākā; Burmese", "NotoSansMyanmar-Medium.ttf"),
            ("km", "ភាសាខ្មែរ Phiəsaakhmær; Central Khmer", "NotoSansKhmer-Medium.ttf"),
            ("ur", "اُردُو Urduw; Urdu", "NotoSansArabic-Medium.ttf"),
            ("pa", "ਪੰਜਾਬੀ ; پنجابی Pãjābī; Panjabi; Punjabi", "NotoSansGurmukhi-Medium.ttf"),
            ("ta", "தமிழ் Tamił; Tamil", "NotoSansTamil-Medium.ttf"),

            // Middle East & Central Asia
            ("tr", "Türkçe; Turkish", "NotoSans-Medium.ttf"),
            ("fa", "فارسی Fārsiy; Persian", "NotoSansArabic-Medium.ttf"),
            ("az", "Azərbaycan Dili; آذربایجان دیلی; Азәрбајҹан Дили; Azerbaijani", "NotoSansArabic-Medium.ttf"),
            ("ku", "Kurdî ; کوردی; Kurdish", "NotoSansArabic-Medium.ttf"),
            ("he", "עברית 'Ivriyþ; Hebrew", "NotoSansHebrew-Medium.ttf"),
            ("hy", "Հայերէն Hayerèn; Հայերեն Hayeren; Armenian", "NotoSansArmenian-Medium.ttf"),
            ("ka", "ᲥᲐᲠᲗᲣᲚᲘ Kharthuli; Georgian", "NotoSansGeorgian-Medium.ttf"),
            ("kk", "Қазақ Тілі Qazaq Tili; Қазақша Qazaqşa; Kazakh", "NotoSans-Medium.ttf"),
            ("ug", "ئۇيغۇرچە; ئۇيغۇر تىلى; Uighur; Uyghur", "NotoSansArabic-Medium.ttf"),

            // Africa
            ("sw", "Kiswahili; Swahili", "NotoSans-Medium.ttf"),
            ("af", "Afrikaans", "NotoSans-Medium.ttf"),
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

            public static explicit operator LanguageInfo(ClientLanguage v)
            {
                return v switch
                {
                    ClientLanguage.English => GetLanguage("en"),
                    ClientLanguage.French => GetLanguage("fr"),
                    ClientLanguage.German => GetLanguage("de"),
                    ClientLanguage.Japanese => GetLanguage("ja"),
                    _ => GetLanguage("en"),
                };
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
                "hy" => CharacterSets.Armenian,
                "az" => CharacterSets.Arabic,
                "bn" => CharacterSets.Bengali,
                "my" => CharacterSets.Burmese,
                "km" => CharacterSets.Khmer,
                "ka" => CharacterSets.Georgian,
                "el" => CharacterSets.Greek,
                "he" => CharacterSets.Hebrew,
                "hi" => CharacterSets.Devanagari,
                "kk" => CharacterSets.Kazakh,
                "ko" => CharacterSets.Korean,
                "ku" => CharacterSets.Arabic,
                "lv" => CharacterSets.LatvianLithuanian,
                "lt" => CharacterSets.LatvianLithuanian,
                "mn" => CharacterSets.Mongolian,
                "pa" => CharacterSets.Gurmukhi,
                "fa" => CharacterSets.Arabic,
                "ru" => CharacterSets.Russian,
                "ta" => CharacterSets.Tamil,
                "th" => CharacterSets.Thai,
                "tr" => CharacterSets.Turkish,
                "ug" => CharacterSets.Arabic,
                "ur" => CharacterSets.Arabic,
                "yi" => CharacterSets.Hebrew,
                _ => string.Empty
            };
    }
}
