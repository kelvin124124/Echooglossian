// <copyright file="ItemTooltip.cs" company="lokinmodar">
// Copyright (c) lokinmodar. All rights reserved.
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International Public License license.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
  [Table("itemtooltips")]
  public class ItemTooltip
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public string OriginalItemTooltip { get; set; }

    [Required]
    public string OriginalItemTooltipLang { get; set; }

    public string TranslatedItemTooltip { get; set; }

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
    /// Initializes a new instance of the <see cref="ItemTooltip"/> class.
    /// </summary>
    /// <param name="originalItemTooltip"></param>
    /// <param name="originalItemTooltipLang"></param>
    /// <param name="translatedItemTooltip"></param>
    /// <param name="translationLang"></param>
    /// <param name="translationEngine"></param>
    /// <param name="gameVersion"></param>
    /// <param name="createdDate"></param>
    /// <param name="updatedDate"></param>
    public ItemTooltip(string originalItemTooltip, string originalItemTooltipLang, string translatedItemTooltip, string translationLang, int translationEngine, string gameVersion, DateTime createdDate, DateTime? updatedDate)
    {
      this.OriginalItemTooltip = originalItemTooltip;
      this.OriginalItemTooltipLang = originalItemTooltip;
      this.TranslatedItemTooltip = translatedItemTooltip;
      this.TranslationLang = translationLang;
      this.TranslationEngine = translationEngine;
      this.GameVersion = gameVersion;
      this.CreatedDate = createdDate;
      this.UpdatedDate = updatedDate;
    }

    public override string ToString()
    {
      return $"Id: {this.Id}, OriginalItemTooltip: {this.OriginalItemTooltip}, OriginalItemTooltipLang: {this.OriginalItemTooltipLang}, TranslatedItemTooltip: {this.TranslatedItemTooltip}, TranslationLang: {this.TranslationLang}, TranslationEngine: {this.TranslationEngine}, GameVersion: {this.GameVersion}, CreatedDate: {this.CreatedDate}, UpdatedDate: {this.UpdatedDate}";
    }
  }
}
