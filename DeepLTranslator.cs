﻿// <copyright file="DeepLTranslator.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Dalamud.Plugin.Services;
using DeepL;
using Newtonsoft.Json;

namespace Echoglossian
{
  public partial class DeepLTranslator : ITranslator
  {
    private readonly IPluginLog pluginLog;

    private readonly bool isUsingAPIKey;
    private readonly Translator? client;

    private readonly HttpClient? httpClient;

    private readonly Random? rndId;

    private const string FreeEndpoint = "https://www2.deepl.com/jsonrpc";

    public DeepLTranslator(IPluginLog pluginLog, bool isUsingAPIKey, string translatorKey)
    {
      this.pluginLog = pluginLog;
      this.isUsingAPIKey = isUsingAPIKey;
      if (isUsingAPIKey && !string.IsNullOrEmpty(translatorKey))
      {
        this.client = new(translatorKey);
        return;
      }

      this.rndId = new Random(Guid.NewGuid().GetHashCode());
      HttpClientHandler handler = new HttpClientHandler()
      {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
      };
      this.httpClient = new HttpClient(handler);

      // Setting HTTP headers to mimic a request from the DeepL iOS App
      this.httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
      this.httpClient.DefaultRequestHeaders.Add("x-app-os-name", "iOS");
      this.httpClient.DefaultRequestHeaders.Add("x-app-os-version", "16.3.0");
      this.httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
      this.httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
      this.httpClient.DefaultRequestHeaders.Add("x-app-device", "iPhone13,2");
      this.httpClient.DefaultRequestHeaders.Add("User-Agent", "DeepL-iOS/2.9.1 iOS 16.3.0 (iPhone13,2)");
      this.httpClient.DefaultRequestHeaders.Add("x-app-build", "510265");
      this.httpClient.DefaultRequestHeaders.Add("x-app-version", "2.9.1");
      this.httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
    }

    async Task<string?> ITranslator.TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
      if (this.isUsingAPIKey)
      {
        return await this.TranslateAsync(text, sourceLanguage, targetLanguage);
      }
      else
      {
        return await this.FreeTranslateAsync(text, sourceLanguage, targetLanguage);
      }
    }

    string? ITranslator.Translate(string text, string sourceLanguage, string targetLanguage)
    {
      if (this.isUsingAPIKey)
      {
        return this.Translate(text, sourceLanguage, targetLanguage);
      }
      else
      {
        return this.FreeTranslateAsync(text, sourceLanguage, targetLanguage).Result;
      }
    }

    private string? Translate(string text, string sourceLanguage, string targetLanguage)
    {
      this.pluginLog.Debug("inside DeepLTranslator Translate method");

      try
      {
        var translation = this.client?.TranslateTextAsync(
          text,
          this.FormatSourceLanguage(sourceLanguage),
          this.FormatTargetLanguage(targetLanguage)).Result;
        this.pluginLog.Debug($"FinalTranslatedText: {translation?.Text}");
        return translation?.Text;
      }
      catch (Exception exception)
      {
        this.pluginLog.Warning($"DeepLTranslator Translate: {exception.Message}");
        return text;
      }
    }

    private async Task<string?> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
      this.pluginLog.Debug("inside DeepLTranslator TranslateAsync method");

      try
      {
        if (this.client == null)
        {
          throw new InvalidOperationException("DeepL client is not initialized.");
        }

        var translation = await this.client.TranslateTextAsync(
          text,
          this.FormatSourceLanguage(sourceLanguage),
          this.FormatTargetLanguage(targetLanguage));
        this.pluginLog.Debug($"FinalTranslatedText: {translation.Text}");
        return translation.Text;
      }
      catch (Exception exception)
      {
        this.pluginLog.Warning($"DeepLTranslator TranslateAsync: {exception.Message}");
        return text;
      }
    }

    private int GetICount(string translateText)
    {
      return translateText.Count(c => c == 'i');
    }

    private long GetTimestamp(int iCount)
    {
      long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      if (iCount == 0)
      {
        return timestamp;
      }

      iCount++;
      return timestamp - (timestamp % iCount) + iCount;
    }

    /// <summary>
    /// Translates a string using the Free DeepL API.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <param name="sourceLanguage">The source language of the text.</param>
    /// <param name="targetLanguage">The target language for the translation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    private async Task<string> FreeTranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
      this.pluginLog.Debug("inside DeepLTranslator FreeTranslateAsync method");

      try
      {
        long timestamp = this.GetTimestamp(this.GetICount(text));
        var id = this.rndId?.Next(11111111, 99999999) ?? throw new InvalidOperationException("Random number generator not initialized.");

        var requestBody = new
        {
          jsonrpc = "2.0",
          method = "LMT_handle_texts",
          @params = new
          {
            splitting = "newlines",
            lang = new
            {
              target_lang = this.FormatFreeTargetLanguage(targetLanguage),
              source_lang_user_selected = this.FormatSourceLanguage(sourceLanguage),
            },
            commonJobParams = new
            {
              wasSpoken = false,
              transcribe_as = string.Empty,
            },
            texts = new[]
            {
              new
              {
                text,
                request_alternatives = 3,
              },
            },
            timestamp,
          },
          id,
        };

        var requestBodyText = JsonConvert.SerializeObject(requestBody);

        // Adding spaces to the JSON string based on the ID to adhere to DeepL's request formatting rules
        if ((id + 5) % 29 == 0 || (id + 3) % 13 == 0)
        {
          requestBodyText = requestBodyText.Replace("\"method\":\"", "\"method\" : \"");
        }
        else
        {
          requestBodyText = requestBodyText.Replace("\"method\":\"", "\"method\": \"");
        }

        if (this.httpClient == null)
        {
          throw new InvalidOperationException("DeepL HttpClient is not initialized.");
        }

        var response = await this.httpClient.PostAsync(FreeEndpoint, new StringContent(
             requestBodyText,
             Encoding.UTF8,
             "application/json"));

        if (response.IsSuccessStatusCode)
        {
          var jsonString = await response.Content.ReadAsStringAsync();
          var deepLResponse = JsonConvert.DeserializeObject<DeepLResponse>(jsonString);
          if (deepLResponse?.Result?.Texts != null && deepLResponse.Result.Texts.Length > 0)
          {
            var finalTranslatedText = deepLResponse.Result.Texts[0].Text;
            this.pluginLog.Debug($"FinalTranslatedText: {finalTranslatedText}");
            return finalTranslatedText;
          }
          else
          {
            this.pluginLog.Warning("DeepLTranslator FreeTranslateAsync: No translation result found.");
            return text;
          }
        }
        else
        {
          this.pluginLog.Warning($"DeepLTranslator FreeTranslateAsync error: {response.StatusCode}");
          return text;
        }
      }
      catch (Exception exception)
      {
        this.pluginLog.Warning($"DeepLTranslator FreeTranslateAsync: {exception.Message}");
        return text;
      }
    }

    private string FormatSourceLanguage(string source)
    {
      switch (source)
      {
        case "Japanese":
          return "JA";
        case "English":
          return "EN";
        case "German":
          return "DE";
        case "French":
          return "FR";
        default:
          return "EN";
      }
    }

    private string FormatTargetLanguage(string source)
    {
      switch (source)
      {
        case "en":
          return "EN-US";
        case "no":
          return "NB";
        case "pt":
          return "PT-BR";
        case "zh-CN":
          return "ZH";
        case "pt-PT":
          return "PT-PT";
        case "it":
          return "IT";
        default:
          return source.ToUpper();
      }
    }

    private string FormatFreeTargetLanguage(string source)
    {
      switch (source)
      {
        case "en":
          return "EN-US";
        case "no":
          return "NB";
        case "pt":
          return "PT-BR";
        case "zh-CN":
          return "ZH";
        case "pt-PT":
          return "PT-PT";
        case "it":
          return "IT";
        default:
          return source.ToUpper();
      }
    }
  }

  public class DeepLResponse
  {
    public string Id { get; set; }

    public string Jsonrpc { get; set; }

    public DeepLResult Result { get; set; }
  }

  public class DeepLResult
  {
    public DeepLTextResult[] Texts { get; set; }

    public string Lang { get; set; }
  }

  public class DeepLTextResult
  {
    public string Text { get; set; }
  }
}
