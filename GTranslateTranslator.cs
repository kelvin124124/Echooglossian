using System;
using System.Threading.Tasks;

using Dalamud.Plugin.Services;
using GTranslate;
using GTranslate.Translators;

namespace Echoglossian
{
  public class GTranslateTranslator : ITranslator
  {
    private readonly IPluginLog pluginLog;
    private readonly Config config;
    private readonly AggregateTranslator translator;

    public GTranslateTranslator(IPluginLog pluginLog, Config config)
    {
      this.pluginLog = pluginLog;
      this.config = config;
      this.translator = new AggregateTranslator(); // Switch to GoogleTranslator() if you want to force only Google
    }

    public string Translate(string text, string sourceLanguage, string targetLanguage)
    {
      this.pluginLog.Debug("GTranslate sync translate requested.");
      return this.TranslateAsync(text, sourceLanguage, targetLanguage).Result;
    }

    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
      if (string.IsNullOrWhiteSpace(text))
      {
        return string.Empty;
      }

      string fixedText = this.FixText(text);
      this.pluginLog.Debug($"GTranslate input: {fixedText}");

      try
      {
        var result = await this.translator.TranslateAsync(fixedText, sourceLanguage, targetLanguage);
        string cleaned = this.FixText(result.Translation);
        this.pluginLog.Debug($"GTranslate result: {cleaned}");
        return cleaned;
      }
      catch (Exception ex)
      {
        this.pluginLog.Warning($"GTranslate error: {ex}");
        return string.Empty;
      }
    }

    private string FixText(string text)
    {
      string fixedText = text
          .Replace("\u200B", "")
          .Replace("\u005C\u0022", "\"")
          .Replace("\u005C\u002F", "/")
          .Replace("\\u003C", "<")
          .Replace("&#39;", "'");

      return System.Text.RegularExpressions.Regex.Replace(fixedText, @"(?<=.)(─)(?=.)", " \u2015 ");
    }
  }
}
