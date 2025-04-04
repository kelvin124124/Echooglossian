// <copyright file="ItemTooltip.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
    [Table("itemtooltips")]
    public class ItemTooltip
    {
        [Key]
        public int Id { get; set; }

        public string? OriginalItemTooltip { get; set; }

        public string? OriginalItemTooltipLang { get; set; }

        public string? TranslatedItemTooltip { get; set; }

        public string? TranslationLang { get; set; }

        public int? TranslationEngine { get; set; }

        public string? GameVersion { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemTooltip"/> class.
        /// </summary>
        /// <param name="originalItemTooltip">The original item tooltip text.</param>
        /// <param name="originalItemTooltipLang">The language of the original item tooltip.</param>
        /// <param name="translatedItemTooltip">The translated item tooltip text.</param>
        /// <param name="translationLang">The language of the translated item tooltip.</param>
        /// <param name="translationEngine">The translation engine used.</param>
        /// <param name="gameVersion">The game version.</param>
        /// <param name="createdDate">The date the item tooltip was created.</param>
        /// <param name="updatedDate">The date the item tooltip was last updated.</param>
        public ItemTooltip(string? originalItemTooltip, string? originalItemTooltipLang, string? translatedItemTooltip, string? translationLang, int? translationEngine, string? gameVersion, DateTime? createdDate, DateTime? updatedDate)
        {
            OriginalItemTooltip = originalItemTooltip;
            OriginalItemTooltipLang = originalItemTooltipLang;
            TranslatedItemTooltip = translatedItemTooltip;
            TranslationLang = translationLang;
            TranslationEngine = translationEngine;
            GameVersion = gameVersion;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string? ToString()
        {
            return $"Id: {Id}, OriginalItemTooltip: {OriginalItemTooltip}, OriginalItemTooltipLang: {OriginalItemTooltipLang}, TranslatedItemTooltip: {TranslatedItemTooltip}, TranslationLang: {TranslationLang}, TranslationEngine: {TranslationEngine}, GameVersion: {GameVersion}, CreatedDate: {CreatedDate}, UpdatedDate: {UpdatedDate}";
        }
    }
}
