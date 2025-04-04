// <copyright file="GameWindow.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
    [Table("gamewindows")]
    public class GameWindow
    {
        [Key]
        public int Id { get; set; }

        public string? WindowAddonName { get; set; }

        public string? OriginalWindowStrings { get; set; }

        public string? OriginalWindowStringsLang { get; set; }

        public string? TranslatedWindowStrings { get; set; }

        public string? TranslationLang { get; set; }

        public int? TranslationEngine { get; set; }

        public string? GameVersion { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameWindow"/> class.
        /// </summary>
        /// <param name="windowAddonName">The name of the window addon.</param>
        /// <param name="originalWindowStrings">The original window strings.</param>
        /// <param name="originalWindowStringsLang">The language of the original window strings.</param>
        /// <param name="translatedWindowStrings">The translated window strings.</param>
        /// <param name="translationLang">The language of the translation.</param>
        /// <param name="translationEngine">The translation engine used.</param>
        /// <param name="gameVersion">The version of the game.</param>
        /// <param name="createdDate">The date the record was created.</param>
        /// <param name="updatedDate">The date the record was last updated.</param>
        public GameWindow(string? windowAddonName, string? originalWindowStrings, string? originalWindowStringsLang, string? translatedWindowStrings, string? translationLang, int? translationEngine, string? gameVersion, DateTime? createdDate, DateTime? updatedDate)
        {
            WindowAddonName = windowAddonName;
            OriginalWindowStrings = originalWindowStrings;
            OriginalWindowStringsLang = originalWindowStringsLang;
            TranslatedWindowStrings = translatedWindowStrings;
            TranslationLang = translationLang;
            TranslationEngine = translationEngine;
            GameVersion = gameVersion;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string? ToString()
        {
            return $"GameWindow: Id={Id}, WindowAddonName={WindowAddonName}, OriginalWindowStrings={OriginalWindowStrings}, OriginalWindowStringsLang={OriginalWindowStringsLang}, TranslatedWindowStrings={TranslatedWindowStrings}, TranslationLang={TranslationLang}, TranslationEngine={TranslationEngine}, GameVersion={GameVersion}, CreatedDate={CreatedDate}, UpdatedDate={UpdatedDate}";
        }
    }
}
