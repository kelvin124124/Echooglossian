// <copyright file="BattleTalkMessage.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
    [Table("battletalkmessages")]
    public class BattleTalkMessage
    {
        [Key]
        public int Id { get; set; }

        public string? SenderName { get; set; }

        public string? OriginalBattleTalkMessage { get; set; }

        public string? OriginalSenderNameLang { get; set; }

        public string? OriginalBattleTalkMessageLang { get; set; }

        public string? TranslatedSenderName { get; set; }

        public string? TranslatedBattleTalkMessage { get; set; }

        public string? TranslationLang { get; set; }

        public int? TranslationEngine { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BattleTalkMessage"/> class.
        /// </summary>
        /// <param name="senderName">The name of the sender.</param>
        /// <param name="originalBattleTalkMessage">The original battle talk message.</param>
        /// <param name="originalBattleTalkMessageLang">The language of the original battle talk message.</param>
        /// <param name="originalSenderNameLang">The language of the original sender name.</param>
        /// <param name="translatedSenderName">The translated sender name.</param>
        /// <param name="translatedBattleTalkMessage">The translated battle talk message.</param>
        /// <param name="translationLang">The language of the translation.</param>
        /// <param name="translationEngine">The translation engine used.</param>
        /// <param name="createdDate">The date the message was created.</param>
        /// <param name="updatedDate">The date the message was last updated.</param>
        public BattleTalkMessage(string? senderName, string? originalBattleTalkMessage, string? originalBattleTalkMessageLang,
          string? originalSenderNameLang, string? translatedSenderName, string? translatedBattleTalkMessage,
          string? translationLang, int? translationEngine, DateTime? createdDate, DateTime? updatedDate)
        {
            SenderName = senderName;
            OriginalBattleTalkMessage = originalBattleTalkMessage;
            OriginalSenderNameLang = originalSenderNameLang;
            OriginalBattleTalkMessageLang = originalBattleTalkMessageLang;
            TranslatedSenderName = translatedSenderName;
            TranslatedBattleTalkMessage = translatedBattleTalkMessage;
            TranslationLang = translationLang;
            TranslationEngine = translationEngine;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string? ToString()
        {
            return
              $"Id: {Id}, Sender: {SenderName}, OriginalMsg: {OriginalBattleTalkMessage}, OriginalLang: {OriginalBattleTalkMessageLang}, OriginalSenderNameLang: {OriginalSenderNameLang}, TranslatedName: {TranslatedSenderName}, TranslMsg: {TranslatedBattleTalkMessage}, TransLang: {TranslationLang}, TranEngine: {TranslationEngine}, CreatedAt: {CreatedDate}, UpdatedAt: {UpdatedDate}";
        }
    }
}
