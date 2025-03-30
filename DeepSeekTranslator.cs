﻿// <copyright file="DeepSeekTranslator.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dalamud.Plugin.Services;
using Echoglossian.Properties;

namespace Echoglossian
{
  public class DeepSeekTranslator : ITranslator
  {
    private readonly HttpClient? httpClient;
    private readonly IPluginLog pluginLog;
    private readonly string model;
    private readonly float temperature = 0.1f;
    private readonly Dictionary<string, string> translationCache = new Dictionary<string, string>();
    private string baseUrl;
    private string apiKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeepSeekTranslator"/> class.
    /// </summary>
    /// <param name="pluginLog">The plugin log instance for logging purposes.</param>
    /// <param name="config">The configuration settings for the DeepSeekTranslator.</param>
    public DeepSeekTranslator(IPluginLog pluginLog, Config config)
    {
      this.baseUrl = config.DeepSeekBaseUrl ?? "https://api.deepseek.com/v1";
      this.apiKey = config.DeepSeekTranslatorApiKey ?? string.Empty;
      this.model = config.DeepSeekModel ?? "deepseek-chat";
      this.temperature = config.DeepSeekTemperature;
      this.pluginLog = pluginLog;

      if (string.IsNullOrWhiteSpace(this.apiKey))
      {
        this.pluginLog.Warning(Resources.APIKeyIsEmptyOrInvalidDeepSeekTranslationWillNotBeAvailable);
        this.httpClient = null;
      }
      else
      {
        try
        {
          pluginLog.Debug($"DeepSeekTranslator: {this.baseUrl}, {this.apiKey[..20]}***{this.apiKey[^5..]}, {this.temperature}");

          this.httpClient = new HttpClient
          {
            BaseAddress = new Uri(this.baseUrl),
          };
          this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.apiKey);
          this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        catch (Exception ex)
        {
          this.pluginLog.Error($"Failed to initialize DeepSeek HTTP client: {ex.Message}");
          this.httpClient = null;
        }
      }
    }

    public string Translate(string text, string sourceLanguage, string targetLanguage)
    {
      return this.TranslateAsync(text, sourceLanguage, targetLanguage).GetAwaiter().GetResult() ?? string.Empty;
    }

    public async Task<string?> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
      if (this.httpClient == null)
      {
        return Resources.DeepSeekTranslationUnavailablePleaseCheckYourAPIKey;
      }

      string cacheKey = $"{text}_{sourceLanguage}_{targetLanguage}";
      if (this.translationCache.TryGetValue(cacheKey, out string? cachedTranslation))
      {
        return cachedTranslation;
      }

      string prompt = @$"As an expert translator and cultural localization specialist with deep knowledge of video game localization, your task is to translate dialogues from the game Final Fantasy XIV from {sourceLanguage} to {targetLanguage}. This is not just a translation, but a full localization effort tailored for the Final Fantasy XIV universe. Please adhere to the following guidelines:

1. Preserve the original tone, humor, personality, and emotional nuances of the dialogue, considering the unique style and atmosphere of Final Fantasy XIV.
2. Adapt idioms, cultural references, and wordplay to resonate naturally with native {targetLanguage} speakers while maintaining the fantasy RPG context.
3. Maintain consistency in character voices, terminology, and naming conventions specific to Final Fantasy XIV throughout the translation.
4. Avoid literal translations that may lose the original intent or impact, especially for game-specific terms or lore elements.
5. Ensure the translation flows naturally and reads as if it were originally written in {targetLanguage}, while staying true to the game's narrative style.
6. Consider the context and subtext of the dialogue, including any references to the game's lore, world, or ongoing storylines.
7. If a word, phrase, or name has been translated in a specific way, maintain that translation consistently unless the context demands otherwise, respecting established localization choices for Final Fantasy XIV.
8. Pay attention to formal/informal speech patterns and adjust accordingly for the target language and cultural norms, considering the speaker's role and status within the game world.
9. Be mindful of character limits or text box constraints that may be present in the game, adapting the translation to fit if necessary.
10. Preserve any game-specific jargon, spell names, or technical terms according to the official localization guidelines for Final Fantasy XIV in the target language.

Text to translate: ""{text}""

Please provide only the translated text in your response, without any explanations, additional comments, or quotation marks. Your goal is to create a localized version that captures the essence of the original Final Fantasy XIV dialogue while feeling authentic to {targetLanguage} speakers and seamlessly fitting into the game world.";

      try
      {
        var requestData = new
        {
          model = this.model,
          messages = new[]
            {
                        new
                        {
                            role = "user",
                            content = prompt,
                        },
            },
          temperature = this.temperature,
        };

        var jsonContent = JsonConvert.SerializeObject(requestData);
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await this.httpClient.PostAsync("chat/completions", httpContent);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JObject.Parse(responseString);

        var translatedText = responseObject["choices"]?[0]?["message"]?["content"]?.ToString().Trim();

        if (!string.IsNullOrEmpty(translatedText))
        {
          translatedText = translatedText.Trim('"');
          this.translationCache[cacheKey] = translatedText;
          return translatedText;
        }
      }
      catch (HttpRequestException httpEx)
      {
        this.pluginLog.Error($"{Resources.TranslationError} HTTP Error: {httpEx.Message}");
        return $"[{Resources.TranslationError} HTTP Error: {httpEx.Message}]";
      }
      catch (JsonException jsonEx)
      {
        this.pluginLog.Error($"{Resources.TranslationError} JSON Error: {jsonEx.Message}");
        return $"[{Resources.TranslationError} JSON Error: {jsonEx.Message}]";
      }
      catch (Exception ex)
      {
        this.pluginLog.Error($"{Resources.TranslationError} {ex.Message}");
        return $"[{Resources.TranslationError} {ex.Message}]";
      }

      return string.Empty;
    }
  }
}