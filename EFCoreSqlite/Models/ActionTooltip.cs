// <copyright file="ActionTooltip.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
    [Table("actiontooltips")]
    public class ActionTooltip
    {
        [Key]
        public int Id { get; set; }

        public string? OriginalActionTooltip { get; set; }

        public string? OriginalActionTooltipLang { get; set; }

        public string? TranslatedActionTooltip { get; set; }

        public string? TranslationLang { get; set; }

        public int? TranslationEngine { get; set; }

        public string? GameVersion { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionTooltip"/> class.
        /// </summary>
        /// <param name="originalActionTooltip">The original action tooltip text.</param>
        /// <param name="originalActionTooltipLang">The language of the original action tooltip.</param>
        /// <param name="translatedActionTooltip">The translated action tooltip text.</param>
        /// <param name="translationLang">The language of the translated action tooltip.</param>
        /// <param name="translationEngine">The translation engine used.</param>
        /// <param name="gameVersion">The game version.</param>
        /// <param name="createdDate">The date the tooltip was created.</param>
        /// <param name="updatedDate">The date the tooltip was last updated.</param>
        public ActionTooltip(string? originalActionTooltip, string? originalActionTooltipLang, string? translatedActionTooltip, string? translationLang, int? translationEngine, string? gameVersion, DateTime? createdDate, DateTime? updatedDate)
        {
            OriginalActionTooltip = originalActionTooltip;
            OriginalActionTooltipLang = originalActionTooltipLang;
            TranslatedActionTooltip = translatedActionTooltip;
            TranslationLang = translationLang;
            TranslationEngine = translationEngine;
            GameVersion = gameVersion;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string? ToString()
        {
            return $"Id: {Id}, OriginalActionTooltip: {OriginalActionTooltip}, OriginalActionTooltipLang: {OriginalActionTooltipLang}, TranslatedActionTooltip: {TranslatedActionTooltip}, TranslationLang: {TranslationLang}, TranslationEngine: {TranslationEngine}, GameVersion: {GameVersion}, CreatedDate: {CreatedDate}, UpdatedDate: {UpdatedDate}";
        }
    }
}
