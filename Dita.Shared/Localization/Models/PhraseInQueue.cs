using Dita.Shared.Localization.Enums;

namespace Dita.Shared.Localization.Models;
/// <summary>
/// Represents a phrase queued for translation, including source and target language information, translation status,
/// and timing metadata.
/// </summary>
public class PhraseInQueue
{
   /// <summary>
   /// Specifies the target for the translation operation.
   /// </summary>
   public TranslationTarget Target { get; set; } = TranslationTarget.Languages;
   /// <summary>
   /// Gets or sets the key identifier.
   /// </summary>
   public string? Key { get; set; } = string.Empty;
   /// <summary>
   /// Gets or sets the phrase text.
   /// </summary>
   public string Phrase { get; set; } = string.Empty;
   /// <summary>
   /// Gets or sets the source language.
   /// </summary>
   public string? SourceLanguage { get; set; } = string.Empty;
   /// <summary>
   /// Target language for translation or processing.
   /// </summary>
   public string TargetLanguage { get; set; } = string.Empty;
   /// <summary>
   /// Type of change required for the phrase.
   /// </summary>
   public PhraseChange ChangeRequired { get; set; } = PhraseChange.NoChange;
   /// <summary>
   /// Gets or sets the UTC timestamp when the item was added to the list.
   /// </summary>
   public DateTime AddedToList { get; set; } = DateTime.UtcNow;
   /// <summary>
   /// Gets or sets the UTC timestamp when the translation started.
   /// </summary>
   public DateTime? TranslationStart { get; set; }
   /// <summary>
   /// Gets or sets the UTC timestamp when the translation ended.
   /// </summary>
   public DateTime? TranslationEnds { get; set; }
   /// <summary>
   /// Gets or sets a value indicating whether the item has been translated.
   /// </summary>
   public bool IsTranslated { get; set; } = false;
   /// <summary>
   /// Gets or sets the translated text.
   /// </summary>
   public string? TranslatedText { get; set; } = string.Empty;

}

