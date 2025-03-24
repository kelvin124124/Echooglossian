// <copyright file="YandexTranslator.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System;
using System.Threading.Tasks;

using Dalamud.Plugin.Services;

namespace Echoglossian
{
  public partial class YandexTranslator : ITranslator
  {
    private readonly IPluginLog pluginLog;
    private readonly Config config;

    public YandexTranslator(IPluginLog pluginLog, Config config)
    {
      this.pluginLog = pluginLog;
      this.config = config;
    }

    public string Translate(string text, string sourceLanguage, string targetLanguage)
    {
      throw new NotImplementedException();
    }

    public Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
    {
      throw new NotImplementedException();
    }
  }
}
