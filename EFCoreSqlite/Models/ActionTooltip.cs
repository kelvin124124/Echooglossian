// <copyright file="ActionTooltip.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
  [Table("actiontooltips")]
  public class ActionTooltip
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public string OriginalActionTooltip { get; set; }

    [Required]
    public string OriginalActionTooltipLang { get; set; }

    public string TranslatedActionTooltip { get; set; }

    public string TranslationLang { get; set; }

    [Required]
    public int TranslationEngine { get; set; }

    [Required]
    public string GameVersion { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionTooltip"/> class.
    /// </summary>
    /// <param name="originalActionTooltip"></param>
    /// <param name="originalActionTooltipLang"></param>
    /// <param name="translatedActionTooltip"></param>
    /// <param name="translationLang"></param>
    /// <param name="translationEngine"></param>
    /// <param name="gameVersion"></param>
    /// <param name="createdDate"></param>
    /// <param name="updatedDate"></param>
    public ActionTooltip(string originalActionTooltip, string originalActionTooltipLang, string translatedActionTooltip, string translationLang, int translationEngine, string gameVersion, DateTime createdDate, DateTime? updatedDate)
    {
      this.OriginalActionTooltip = originalActionTooltip;
      this.OriginalActionTooltipLang = originalActionTooltip;
      this.TranslatedActionTooltip = translatedActionTooltip;
      this.TranslationLang = translationLang;
      this.TranslationEngine = translationEngine;
      this.GameVersion = gameVersion;
      this.CreatedDate = createdDate;
      this.UpdatedDate = updatedDate;
    }

    public override string ToString()
    {
      return $"Id: {this.Id}, OriginalActionTooltip: {this.OriginalActionTooltip}, OriginalActionTooltipLang: {this.OriginalActionTooltipLang}, TranslatedActionTooltip: {this.TranslatedActionTooltip}, TranslationLang: {this.TranslationLang}, TranslationEngine: {this.TranslationEngine}, GameVersion: {this.GameVersion}, CreatedDate: {this.CreatedDate}, UpdatedDate: {this.UpdatedDate}";
    }
  }
}
