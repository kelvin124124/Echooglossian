using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Echoglossian.EFCoreSqlite.Models
{
  [Table("selectstrings")]
  public class SelectString
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public string OriginalSelectString { get; set; }

    [Required]
    public string OriginalSelectStringLang { get; set; }

    public string TranslatedSelectString { get; set; }

    public string TranslationLang { get; set; }

    [Required]
    public int TranslationEngine { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectString"/> class.
    /// </summary>
    /// <param name="originalSelectString"></param>
    /// <param name="originalSelectStringLang"></param>
    /// <param name="translatedSelectString"></param>
    /// <param name="translationLang"></param>
    /// <param name="translationEngine"></param>
    /// <param name="createdDate"></param>
    /// <param name="updatedDate"></param>
    public SelectString(string originalSelectString, string originalSelectStringLang, string translatedSelectString, string translationLang, int translationEngine, DateTime createdDate, DateTime? updatedDate)
    {
      this.OriginalSelectString = originalSelectString;
      this.OriginalSelectStringLang = originalSelectStringLang;
      this.TranslatedSelectString = translatedSelectString;
      this.TranslationLang = translationLang;
      this.TranslationEngine = translationEngine;
      this.CreatedDate = createdDate;
      this.UpdatedDate = updatedDate;
    }

    public override string ToString()
    {
      return $"Id: {this.Id}, OriginalSelectString: {this.OriginalSelectString}, OriginalSelectStringLang: {this.OriginalSelectStringLang}, TranslatedSelectString: {this.TranslatedSelectString}, TranslationLang: {this.TranslationLang}, TranslationEngine: {this.TranslationEngine}, CreatedDate: {this.CreatedDate}, UpdatedDate: {this.UpdatedDate}";
    }
  }
}
