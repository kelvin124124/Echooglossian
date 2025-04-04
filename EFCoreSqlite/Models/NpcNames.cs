// <copyright file="NpcNames.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
    [Table("npcnames")]
    public class NpcNames
    {
        [Key]
        public int Id { get; set; }

        public string? OriginalNpcName { get; set; }

        public string? OriginalNpcNameLang { get; set; }

        public string? TranslatedNpcName { get; set; }

        public string? TranslationLang { get; set; }

        public int? TranslationEngine { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NpcNames"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the NPC.</param>
        /// <param name="originalNpcName">The original name of the NPC.</param>
        /// <param name="originalNpcNameLang">The language of the original NPC name.</param>
        /// <param name="translatedNpcName">The translated name of the NPC.</param>
        /// <param name="translationLang">The language of the translated NPC name.</param>
        /// <param name="translationEngine">The translation engine used.</param>
        /// <param name="createdDate">The date the NPC name was created.</param>
        /// <param name="updatedDate">The date the NPC name was last updated.</param>
        public NpcNames(int id, string? originalNpcName, string? originalNpcNameLang, string? translatedNpcName, string? translationLang, int? translationEngine, DateTime? createdDate, DateTime? updatedDate)
        {
            Id = id;
            OriginalNpcName = originalNpcName;
            OriginalNpcNameLang = originalNpcNameLang;
            TranslatedNpcName = translatedNpcName;
            TranslationLang = translationLang;
            TranslationEngine = translationEngine;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string? ToString()
        {
            return
              $"Id: {Id}, " +
              $"OriginalNpcName: {OriginalNpcName}, " +
              $"OriginalNpcNameLang: {OriginalNpcNameLang}, " +
              $"TranslatedNpcName: {TranslatedNpcName}, " +
              $"TranslationLang: {TranslationLang}, " +
              $"TranslationEngine: {TranslationEngine}, " +
              $"CreatedDate: {CreatedDate}, " +
              $"UpdatedDate: {UpdatedDate}";
        }
    }
}
