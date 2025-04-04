// <copyright file="ToastMessage.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
    [Table("toastmessages")]
    public class ToastMessage
    {
        [Key]
        public int Id { get; set; }

        public string? ToastType { get; set; }

        public string? OriginalToastMessage { get; set; }

        public string? OriginalLang { get; set; }

        public string? TranslatedToastMessage { get; set; }

        public string? TranslationLang { get; set; }

        public int? TranslationEngine { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToastMessage"/> class.
        /// </summary>
        /// <param name="toastType">Type of the toast message.</param>
        /// <param name="originalToastMessage">The original toast message.</param>
        /// <param name="originalLang">The original language of the toast message.</param>
        /// <param name="translatedToastMessage">The translated toast message.</param>
        /// <param name="translationLang">The language of the translated toast message.</param>
        /// <param name="translationEngine">The translation engine used.</param>
        /// <param name="createdDate">The date the toast message was created.</param>
        /// <param name="updatedDate">The date the toast message was last updated.</param>
        public ToastMessage(string? toastType, string? originalToastMessage, string? originalLang,
          string? translatedToastMessage, string? translationLang, int? translationEngine, DateTime? createdDate, DateTime? updatedDate)
        {
            ToastType = toastType;
            OriginalToastMessage = originalToastMessage;
            OriginalLang = originalLang;
            TranslatedToastMessage = translatedToastMessage;
            TranslationLang = translationLang;
            TranslationEngine = translationEngine;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string? ToString()
        {
            return
              $"Id: {Id}, ToastType: {ToastType}, OriginalMsg: {OriginalToastMessage}, OriginalLang: {OriginalLang}, TranslMsg: {TranslatedToastMessage}, TransLang: {TranslationLang}, TranEngine: {TranslationEngine}, CreatedAt: {CreatedDate}, UpdatedAt: {UpdatedDate}";
        }
    }
}
