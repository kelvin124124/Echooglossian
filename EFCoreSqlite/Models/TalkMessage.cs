// <copyright file="TalkMessage.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
    [Table("talkmessages")]
    public class TalkMessage
    {
        [Key]
        public int Id { get; set; }

        public string? SenderName { get; set; }

        public string? OriginalTalkMessage { get; set; }

        public string? OriginalSenderNameLang { get; set; }

        public string? OriginalTalkMessageLang { get; set; }

        public string? TranslatedSenderName { get; set; }

        public string? TranslatedTalkMessage { get; set; }

        public string? TranslationLang { get; set; }

        public int? TranslationEngine { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TalkMessage"/> class.
        /// </summary>
        /// <param name="senderName">The name of the sender.</param>
        /// <param name="originalTalkMessage">The original talk message.</param>
        /// <param name="originalTalkMessageLang">The language of the original talk message.</param>
        /// <param name="originalSenderNameLang">The language of the original sender's name.</param>
        /// <param name="translatedSenderName">The translated sender's name.</param>
        /// <param name="translatedTalkMessage">The translated talk message.</param>
        /// <param name="translationLang">The language of the translation.</param>
        /// <param name="translationEngine">The translation engine used.</param>
        /// <param name="createdDate">The date the message was created.</param>
        /// <param name="updatedDate">The date the message was last updated.</param>
        public TalkMessage(string? senderName, string? originalTalkMessage, string? originalTalkMessageLang, string? originalSenderNameLang, string? translatedSenderName, string? translatedTalkMessage, string? translationLang, int? translationEngine, DateTime? createdDate, DateTime? updatedDate)
        {
            SenderName = senderName;
            OriginalTalkMessage = originalTalkMessage;
            OriginalSenderNameLang = originalSenderNameLang;
            OriginalTalkMessageLang = originalTalkMessageLang;
            TranslatedSenderName = translatedSenderName;
            TranslatedTalkMessage = translatedTalkMessage;
            TranslationLang = translationLang;
            TranslationEngine = translationEngine;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string? ToString()
        {
            return
              $"Id: {Id}, Sender: {SenderName}, OriginalMsg: {OriginalTalkMessage}, OriginalLang: {OriginalTalkMessageLang}, OriginalSenderNameLang: {OriginalSenderNameLang}, TranslatedName: {TranslatedSenderName}, TranslMsg: {TranslatedTalkMessage}, TransLang: {TranslationLang}, TranEngine: {TranslationEngine}, CreatedAt: {CreatedDate}, UpdatedAt: {UpdatedDate}";
        }
    }
}