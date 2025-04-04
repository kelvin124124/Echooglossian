// <copyright file="TalkSubtitleMessage.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
    [Table("talksubtitlemessages")]
    public class TalkSubtitleMessage
    {
        [Key]
        public int Id { get; set; }

        public string? OriginalTalkSubtitleMessage { get; set; }

        public string? OriginalTalkSubtitleMessageLang { get; set; }

        public string? TranslatedTalkSubtitleMessage { get; set; }

        public string? TranslationLang { get; set; }

        public int? TranslationEngine { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TalkSubtitleMessage"/> class.
        /// </summary>
        /// <param name="originalTalkSubtitleMessage">The original talk subtitle message.</param>
        /// <param name="originalTalkSubtitleMessageLang">The language of the original talk subtitle message.</param>
        /// <param name="translatedTalkSubtitleMessage">The translated talk subtitle message.</param>
        /// <param name="translationLang">The language of the translated talk subtitle message.</param>
        /// <param name="translationEngine">The translation engine used.</param>
        /// <param name="createdDate">The date the message was created.</param>
        /// <param name="updatedDate">The date the message was last updated.</param>
        public TalkSubtitleMessage(string? originalTalkSubtitleMessage, string? originalTalkSubtitleMessageLang, string? translatedTalkSubtitleMessage, string? translationLang, int? translationEngine, DateTime? createdDate, DateTime? updatedDate)
        {
            OriginalTalkSubtitleMessage = originalTalkSubtitleMessage;
            OriginalTalkSubtitleMessageLang = originalTalkSubtitleMessageLang;
            TranslatedTalkSubtitleMessage = translatedTalkSubtitleMessage;
            TranslationLang = translationLang;
            TranslationEngine = translationEngine;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string? ToString()
        {
            return
              $"Id: {Id}, OriginalMsg: {OriginalTalkSubtitleMessage}, OriginalLang: {OriginalTalkSubtitleMessageLang}, TranslMsg: {TranslatedTalkSubtitleMessage}, TransLang: {TranslationLang}, TranEngine: {TranslationEngine}, CreatedAt: {CreatedDate}, UpdatedAt: {UpdatedDate}";
        }
    }
}