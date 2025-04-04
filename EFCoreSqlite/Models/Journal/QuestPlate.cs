// <copyright file="QuestPlate.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

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
            QuestId = questId;
            QuestName = questName;
            OriginalQuestMessage = originalQuestMessage;
            OriginalLang = originalLang;
            TranslatedQuestName = translatedQuestName;
            TranslatedQuestMessage = translatedQuestMessage;
            TranslationLang = translationLang;
            TranslationEngine = translationEngine;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
            Objectives = new();
            Summaries = new();
        }

        public void UpdateFieldsAsText()
        {
            ObjectivesAsText = string.Empty;
            SummariesAsText = string.Empty;
            if (Objectives != null && Objectives.Count != 0)
            {
                ObjectivesAsText = JsonSerializer.Serialize(Objectives);
            }

            if (Summaries != null && Summaries.Count != 0)
            {
                SummariesAsText = JsonSerializer.Serialize(Summaries);
            }
        }

        public void UpdateFieldsFromText()
        {
            if (!string.IsNullOrEmpty(ObjectivesAsText))
            {
                Objectives = JsonSerializer.Deserialize<Dictionary<string, string>>(ObjectivesAsText) ?? new Dictionary<string, string>();
            }

            if (!string.IsNullOrEmpty(SummariesAsText))
            {
                Summaries = JsonSerializer.Deserialize<Dictionary<string, string>>(SummariesAsText) ?? new Dictionary<string, string>();
            }
        }

        public override string? ToString()
        {
            return
              $"Id: {Id}, QuestName: {QuestName}, QuestID: {QuestId}, OriginalMsg: {OriginalQuestMessage}, OriginalLang: {OriginalLang}, TranslQuestName: {TranslatedQuestName}, TranslMsg: {TranslatedQuestMessage}, TransLang: {TranslationLang}, TranEngine: {TranslationEngine}, CreatedAt: {CreatedDate}, UpdatedAt: {UpdatedDate}, Objectives: {Objectives}, Summaries: {Summaries}";
        }
    }
}
