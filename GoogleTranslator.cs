// <copyright file="GoogleTranslator.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using Dalamud.Plugin.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Echoglossian
{
  public class GoogleTranslator : ITranslator
  {
    private readonly IPluginLog pluginLog;
    private readonly HttpClient httpClient;
    private readonly Config? config;

    private readonly Dictionary<int, (string Url, Dictionary<string, string> Headers, Dictionary<string, string> QueryParams)> translateEndpoints =
        new()
        {
                {
                    0,
                    ("https://translate.google.com/m", new Dictionary<string, string>
                    {
                        { "User-Agent", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.6998.108 Mobile Safari/537.36" },
                    },
                    new Dictionary<string, string>())
                },
                {
                    1,
                    ("https://translate-pa.googleapis.com/v1/translateHtml", new Dictionary<string, string>
                    {
                        { "User-Agent", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.6998.108 Mobile Safari/537.36" },
                        { "X-Goog-API-Key", "AIzaSyATBXajvzQLTDHEQbcpq0Ihe0vWDHmO520" },
                    },
                    new Dictionary<string, string>())
                },
                {
                    2,
                    ("https://dictionaryextension-pa.googleapis.com/v1/dictionaryExtensionData", new Dictionary<string, string>
                    {
                        { "x-referer", "chrome-extension://mgijmajocgfcbeboacabfgobmjgjcoja" },
                    },
                    new Dictionary<string, string>
                    {
                        { "strategy", "2" },
                        { "key", "AIzaSyA6EEtrDCfBkHV8uU2lgGY-N383ZgAOo7Y" },
                    })
                },
        };

#nullable enable

    public GoogleTranslator(IPluginLog pluginLog, Config? config)
    {
      this.pluginLog = pluginLog;
      this.config = config;
      this.httpClient = new HttpClient();

    }

    string ITranslator.Translate(string text, string sourceLanguage, string targetLanguage)
    {
      this.pluginLog.Debug("inside GoogleTranslator translate method");

      try
      {
        string parsedText = this.FixText(text);
        string url;
        Dictionary<string, string> headers;
        Dictionary<string, string> queryParams;

        this.pluginLog.Debug($"GoogleTranslateVersion: {this.config.GoogleTranslateVersion}");

        switch (this.config.GoogleTranslateVersion)
        {
          case 1:
            (url, headers, queryParams) = this.translateEndpoints[1];
            // Implement specific logic for endpoint 1
            this.pluginLog.Debug($"Using Google Translate V1 API: {url}");

            this.httpClient.DefaultRequestHeaders.Add("User-Agent", queryParams["User-Agent"]);
            // Custom logic here (if needed)

            /* payload to send
             * [
                [
                  [
                    "Assuming that Rishushu's associates at the Baert Trading Company are in possession of the same document, I suspect that the next attack on our supply caravan will strike right there─in the Central Shroud, due southwest from the White Wolf Gate."
                  ],
                  "auto",
                  "pt-BR"
                ],
                "wt_lib"
              ] 
             */

            /* example json response
             * [
                [
                  "Supondo que os associados de Rishushu na Companhia Comercial Baert estejam de posse do mesmo documento, suspeito que o próximo ataque à nossa caravana de suprimentos ocorrerá bem ali, no Sudário Central, a sudoeste do Portão do Lobo Branco."
                ],
                [
                  "en"
                ]
              ]
             * */
            break;

          case 2:
            (url, headers, queryParams) = this.translateEndpoints[2];
            // Implement specific logic for endpoint 2
            this.pluginLog.Debug($"Using Google Dictionary API: {url}");
            // Custom logic here (if needed)
            string endpoint = $"{url}?language={targetLanguage}&key={queryParams["key"]}&term={Uri.EscapeDataString(parsedText)}&strategy={queryParams["strategy"]}";

            this.pluginLog.Debug($"Endpoint: {endpoint}");

            this.httpClient.DefaultRequestHeaders.Clear();

            foreach (var header in headers)
            {
              this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            this.pluginLog.Debug($"Request Headers: {this.httpClient.DefaultRequestHeaders}");

            var apiResponse = this.httpClient.GetAsync(endpoint).Result;

            this.pluginLog.Debug($"Request Result: {apiResponse}");

            var translation = apiResponse.Content.ReadAsStringAsync().Result;

            this.pluginLog.Debug($"Translation: {translation}");

            try
            {
              // Parse the JSON response as a JObject
              JObject json = JObject.Parse(translation);

              // Use LINQ to find the translation text
              string? translatedText = json.SelectToken("translateResponse.translateText")?.ToString();

              if (!string.IsNullOrEmpty(translatedText))
              {
                this.pluginLog.Debug("Translated Text: " + translatedText);
                return translatedText;
              }
              else
              {
                this.pluginLog.Warning("Translation not found in response.");
                return string.Empty;
              }
            }
            catch (JsonException ex)
            {
              this.pluginLog.Warning("Error parsing JSON: " + ex.Message);
              return string.Empty;
            }

            /* example json response
             * {
             *   "status": 200,
             *   "translateResponse": {
             *     "translateText": "Supondo que os associados de Rishushu na Baert Trading Company estejam de posse do mesmo documento, suspeito que o próximo ataque à nossa caravana de suprimentos ocorrerá ali mesmo - no Sudário Central, a sudoeste do Portão do Lobo Branco.",
             *     "detectedSourceLanguage": "en",
             *     "outputLanguage": "pt-BR",
             *      "sourceText": "Assuming that Rishushu's associates at the Baert Trading Company are in possession of the same document, I suspect that the next attack on our supply caravan will strike right there─in the Central Shroud, due southwest from the White Wolf Gate."
             *    }
             *  } */

            break;

          default:
            (url, headers, queryParams) = this.translateEndpoints[0];
            url = $"{url}?hl=en&sl={sourceLanguage}&tl={targetLanguage}&q={Uri.EscapeDataString(parsedText)}";

            this.pluginLog.Debug($"URL: {url}");

            this.httpClient.DefaultRequestHeaders.Add("User-Agent", queryParams["User-Agent"]);

            var requestResult = this.httpClient.GetStreamAsync(url).Result;
            StreamReader reader = new(requestResult ?? throw new Exception());

            return this.FormatStreamReader(reader.ReadToEnd());
        }

        throw new NotImplementedException("The selected translation version has not been implemented yet.");
      }
      catch (Exception e)
      {
        this.pluginLog.Warning(e.ToString());
        throw;
      }
    }

    async Task<string> ITranslator.TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
      this.pluginLog.Debug("inside GoogleTranslator translateAsync method");

      try
      {
        string parsedText = this.FixText(text);
        string url;
        Dictionary<string, string> headers;
        Dictionary<string, string> queryParams;

        switch (this.config.GoogleTranslateVersion)
        {
          case 1:
            (url, headers, queryParams) = this.translateEndpoints[1];
            // Implement specific logic for endpoint 1
            this.pluginLog.Debug($"Using Google Translate V1 API: {url}");
            // Custom logic here (if needed)
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", queryParams["User-Agent"]);
            break;

          case 2:
            (url, headers, queryParams) = this.translateEndpoints[2];
            // Implement specific logic for endpoint 2
            this.pluginLog.Debug($"Using Google Dictionary API: {url}");
            // Custom logic here (if needed)
            string endpoint = $"{url}?language={targetLanguage}&key={queryParams["key"]}&term={Uri.EscapeDataString(parsedText)}&strategy={queryParams["strategy"]}";

            this.pluginLog.Debug($"Endpoint: {endpoint}");

            this.httpClient.DefaultRequestHeaders.Add("x-referer", headers["x-referer"]);

            var apiResponse = this.httpClient.GetAsync(endpoint).Result;

            this.pluginLog.Debug($"Request Result: {apiResponse}");

            /* example json response
             * {
             *   "status": 200,
             *   "translateResponse": {
             *     "translateText": "Supondo que os associados de Rishushu na Baert Trading Company estejam de posse do mesmo documento, suspeito que o próximo ataque à nossa caravana de suprimentos ocorrerá ali mesmo - no Sudário Central, a sudoeste do Portão do Lobo Branco.",
             *     "detectedSourceLanguage": "en",
             *     "outputLanguage": "pt-BR",
             *      "sourceText": "Assuming that Rishushu's associates at the Baert Trading Company are in possession of the same document, I suspect that the next attack on our supply caravan will strike right there─in the Central Shroud, due southwest from the White Wolf Gate."
             *    }
             *  } */

            break;

          default:
            (url, headers, queryParams) = this.translateEndpoints[0];
            url = $"{url}?hl=en&sl={sourceLanguage}&tl={targetLanguage}&q={Uri.EscapeDataString(parsedText)}";

            this.pluginLog.Debug($"URL: {url}");

            this.httpClient.DefaultRequestHeaders.Add("User-Agent", queryParams["User-Agent"]);

            var requestResult = await this.httpClient.GetStreamAsync(url);
            StreamReader reader = new(requestResult ?? throw new Exception());

            return this.FormatStreamReader(reader.ReadToEnd());
        }

        throw new NotImplementedException("The selected translation version has not been implemented yet.");
      }
      catch (Exception e)
      {
        this.pluginLog.Warning(e.ToString());
        throw;
      }
    }

    private string FixText(string text)
    {
      string fixedText = text;
      fixedText = fixedText.Replace("\u200B", string.Empty);
      fixedText = fixedText.Replace("\u005C\u0022", "\u0022");
      fixedText = fixedText.Replace("\u005C\u002F", "\u002F");
      fixedText = fixedText.Replace("\\u003C", "<");
      fixedText = fixedText.Replace("&#39;", "\u0027");
      fixedText = Regex.Replace(fixedText, @"(?<=.)(─)(?=.)", " \u2015 ");

      this.pluginLog.Debug($"Fixed Text: {fixedText}");
      return fixedText;
    }

    private string FormatStreamReader(string read)
    {
      string finalText;
      if (read.StartsWith("[\""))
      {
        char[] start = { '[', '\"' };
        char[] end = { '\"', ']' };
        var dialogueText = read.TrimStart(start);
        finalText = dialogueText.TrimEnd(end);
      }
      else
      {
        finalText = this.ParseHtml(read);
      }

      finalText = this.FixText(finalText);
      this.pluginLog.Debug($"FinalTranslatedText: {finalText}");

      return finalText;
    }

    private string ParseHtml(string html)
    {
      using StringWriter stringWriter = new();

      HtmlAgilityPack.HtmlDocument doc = new();
      doc.LoadHtml(html);

      var text = doc.DocumentNode.Descendants()
          .Where(n => n.HasClass("result-container")).ToList();

      var parsedText = text.Single(n => n.InnerText.Length > 0).InnerText;

      HttpUtility.HtmlDecode(parsedText, stringWriter);

      string decodedString = stringWriter.ToString();
      this.pluginLog.Debug($"In parser: " + parsedText);

      return decodedString;
    }
  }
}
