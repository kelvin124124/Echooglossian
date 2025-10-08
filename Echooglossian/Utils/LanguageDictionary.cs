using Dalamud.Game;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Echooglossian.Utils
{
    public static class LanguageDictionary
    {
        private static readonly Dictionary<string, string> LangDict = new()
        {
            // Popular
            ["en"] = "English",
            ["zh-CN"] = "簡體中文; Chinese Simplified",
            ["zh-TW"] = "繁體中文; Chinese Traditional",
            ["hi"] = "हिन्दी Hindī; Hindi",
            ["es"] = "Español; Castellano; Spanish; Castilian",
            ["fr"] = "Français; French",
            ["ar"] = "العَرَبِيَّة Al'Arabiyyeẗ; Arabic",
            ["pt"] = "Português Brasileiro; Brazilian Portuguese",
            ["pt-PT"] = "Português; Portuguese",
            ["bn"] = "বাংলা Bāŋlā; Bengali",
            ["ru"] = "Русский Язык Russkiĭ Âzık; Russian",
            ["de"] = "Deutsch; German",
            ["ja"] = "日本語 Nihongo; Japanese",

            // Europe
            ["it"] = "Italiano; Lingua Italiana; Italian",
            ["ro"] = "Limba Română; Romanian; Moldavian; Moldovan",
            ["ca"] = "Català; Valencià; Catalan; Valencian",
            ["nl"] = "Nederlands; Vlaams; Dutch; Flemish",
            ["sv"] = "Svenska; Swedish",
            ["da"] = "Dansk; Danish",
            ["no"] = "Norsk; Norwegian",
            ["pl"] = "Język Polski; Polish",
            ["uk"] = "Українська Мова; Українська; Ukrainian",
            ["cs"] = "Čeština; Český Jazyk; Czech",
            ["sk"] = "Slovenčina; Slovenský Jazyk; Slovak",
            ["bg"] = "Български Език Bălgarski Ezik; Bulgarian",
            ["sr"] = "Српски ; Srpski; Serbian",
            ["hr"] = "Hrvatski; Croatian",
            ["bs"] = "Bosanski; Bosnian",
            ["sl"] = "Slovenski Jezik; Slovenščina; Slovenian",
            ["mk"] = "Македонски Јазик Makedonski Jazik; Macedonian",
            ["be"] = "Беларуская Мова Belaruskaâ Mova; Belarusian",
            ["el"] = "Νέα Ελληνικά Néa Ellêniká; Greek Modern (1453-)",
            ["hu"] = "Magyar Nyelv; Hungarian",
            ["fi"] = "Suomen Kieli; Finnish",
            ["sq"] = "Shqip; Albanian",
            ["lt"] = "Lietuvių Kalba; Lithuanian",
            ["lv"] = "Latviešu Valoda; Latvian",
            ["et"] = "Eesti Keel; Estonian",
            ["is"] = "Íslenska; Icelandic",
            ["ga"] = "Gaeilge; Irish",
            ["cy"] = "Cymraeg; Y Gymraeg; Welsh",
            ["yi"] = "ייִדיש; יידיש; אידיש Yidiš; Yiddish",

            // Asia
            ["ko"] = "한국어 Han'Gug'Ô; Korean",
            ["mn"] = "Монгол Хэл Mongol Xel; ᠮᠣᠩᠭᠣᠯ ᠬᠡᠯᠡ; Mongolian",
            ["id"] = "Bahasa Indonesia; Indonesian",
            ["vi"] = "TiếNg ViệT; Vietnamese",
            ["th"] = "ภาษาไทย Phasathay; Thai",
            ["ms"] = "Bahasa Melayu; Malay",
            ["tl"] = "Wikang Tagalog; Tagalog",
            ["my"] = "မြန်မာစာ Mrãmācā; မြန်မာစကား Mrãmākā; Burmese",
            ["km"] = "ភាសាខ្មែរ Phiəsaakhmær; Central Khmer",
            ["ur"] = "اُردُو Urduw; Urdu",
            ["pa"] = "ਪੰਜਾਬੀ ; پنجابی Pãjābī; Panjabi; Punjabi",
            ["ta"] = "தமிழ் Tamił; Tamil",

            // Middle East & Central Asia
            ["tr"] = "Türkçe; Turkish",
            ["fa"] = "فارسی Fārsiy; Persian",
            ["az"] = "Azərbaycan Dili; آذربایجان دیلی; Азәрбајҹан Дили; Azerbaijani",
            ["ku"] = "Kurdî ; کوردی; Kurdish",
            ["he"] = "עברית 'Ivriyþ; Hebrew",
            ["hy"] = "Հայերէն Hayerèn; Հայերեն Hayeren; Armenian",
            ["ka"] = "ᲥᲐᲠᲗᲣᲚᲘ Kharthuli; Georgian",
            ["kk"] = "Қазақ Тілі Qazaq Tili; Қазақша Qazaqşa; Kazakh",
            ["ug"] = "ئۇيغۇرچە; ئۇيغۇر تىلى; Uighur; Uyghur",

            // Africa
            ["sw"] = "Kiswahili; Swahili",
            ["af"] = "Afrikaans",
        };

        public readonly record struct LanguageInfo
        {
            public string Code { get; init; }
            public string Name { get; init; }

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
            [.. LangDict.Select(x => new LanguageInfo { Code = x.Key, Name = x.Value })];

        public static LanguageInfo GetLanguage(string code)
        {
            if (!LangDict.TryGetValue(code, out var name))
                throw new KeyNotFoundException($"Language code '{code}' not found.");

            return new LanguageInfo
            {
                Code = code,
                Name = name,
            };
        }
    }
}
