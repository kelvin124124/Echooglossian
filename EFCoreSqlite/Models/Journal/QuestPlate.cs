// <copyright file="QuestPlate.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Echoglossian.EFCoreSqlite.Models.Journal
{
  [Table("questplates")]
  public class QuestPlate
  {
    [Key]
    public int Id { get; set; }

    public string? QuestId { get; set; }

    public string? QuestName { get; set; }

    public string? OriginalQuestMessage { get; set; }

    public string? OriginalLang { get; set; }

    public string? TranslatedQuestName { get; set; }

    public string? TranslatedQuestMessage { get; set; }

    public string? TranslationLang { get; set; }

    public int? TranslationEngine { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [NotMapped]
    public Dictionary<string, string> Objectives { get; set; }

    public string? ObjectivesAsText { get; set; }

    [NotMapped]
    public Dictionary<string, string> Summaries { get; set; }

    public string? SummariesAsText { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuestPlate"/> class.
    /// </summary>
    /// <param name="questName">The name of the quest.</param>
    /// <param name="originalQuestMessage">The original quest message.</param>
    /// <param name="originalLang">The original language of the quest.</param>
    /// <param name="translatedQuestName">The translated name of the quest.</param>
    /// <param name="translatedQuestMessage">The translated quest message.</param>
    /// <param name="questId">The ID of the quest.</param>
    /// <param name="translationLang">The language of the translation.</param>
    /// <param name="translationEngine">The engine used for translation.</param>
    /// <param name="createdDate">The date the quest was created.</param>
    /// <param name="updatedDate">The date the quest was last updated.</param>
    public QuestPlate(
      string? questName, string? originalQuestMessage,
      string? originalLang,
      string? translatedQuestName, string? translatedQuestMessage,
      string? questId, string? translationLang, int? translationEngine,
      DateTime? createdDate, DateTime? updatedDate)
    {
      this.QuestId = questId;
      this.QuestName = questName;
      this.OriginalQuestMessage = originalQuestMessage;
      this.OriginalLang = originalLang;
      this.TranslatedQuestName = translatedQuestName;
      this.TranslatedQuestMessage = translatedQuestMessage;
      this.TranslationLang = translationLang;
      this.TranslationEngine = translationEngine;
      this.CreatedDate = createdDate;
      this.UpdatedDate = updatedDate;
      this.Objectives = new();
      this.Summaries = new();
    }

    public void UpdateFieldsAsText()
    {
      this.ObjectivesAsText = string.Empty;
      this.SummariesAsText = string.Empty;
      if (this.Objectives != null && this.Objectives.Count != 0)
      {
        this.ObjectivesAsText = JsonSerializer.Serialize(this.Objectives);
      }

      if (this.Summaries != null && this.Summaries.Count != 0)
      {
        this.SummariesAsText = JsonSerializer.Serialize(this.Summaries);
      }
    }

    public void UpdateFieldsFromText()
    {
      if (!string.IsNullOrEmpty(this.ObjectivesAsText))
      {
        this.Objectives = JsonSerializer.Deserialize<Dictionary<string, string>>(this.ObjectivesAsText) ?? new Dictionary<string, string>();
      }

      if (!string.IsNullOrEmpty(this.SummariesAsText))
      {
        this.Summaries = JsonSerializer.Deserialize<Dictionary<string, string>>(this.SummariesAsText) ?? new Dictionary<string, string>();
      }
    }

    public override string? ToString()
    {
      return
        $"Id: {this.Id}, QuestName: {this.QuestName}, QuestID: {this.QuestId}, OriginalMsg: {this.OriginalQuestMessage}, OriginalLang: {this.OriginalLang}, TranslQuestName: {this.TranslatedQuestName}, TranslMsg: {this.TranslatedQuestMessage}, TransLang: {this.TranslationLang}, TranEngine: {this.TranslationEngine}, CreatedAt: {this.CreatedDate}, UpdatedAt: {this.UpdatedDate}, Objectives: {this.Objectives}, Summaries: {this.Summaries}";
    }
  }
}
