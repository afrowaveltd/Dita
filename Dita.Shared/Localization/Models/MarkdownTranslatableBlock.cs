namespace Dita.Shared.Localization.Models;

/// <summary>
/// Represents a translatable block extracted from a Markdown document.
/// Each block is assigned a unique GUID key and contains the original text,
/// its position in the document, and metadata for reconstruction.
/// </summary>
public class MarkdownTranslatableBlock
{
   /// <summary>
   /// Gets or sets the unique identifier for this translatable block.
   /// Used to match original and translated text during reconstruction.
   /// </summary>
   public Guid Key { get; set; } = Guid.NewGuid();

   /// <summary>
   /// Gets or sets the original text content of this block before translation.
   /// </summary>
   public string OriginalText { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the translated text content of this block.
   /// Null or empty when translation has not yet been performed.
   /// </summary>
   public string? TranslatedText { get; set; }

   /// <summary>
   /// Gets or sets the zero-based line number where this block starts in the original Markdown document.
   /// </summary>
   public int StartLine { get; set; }

   /// <summary>
   /// Gets or sets the zero-based line number where this block ends in the original Markdown document.
   /// </summary>
   public int EndLine { get; set; }

   /// <summary>
   /// Gets or sets the type of Markdown block (e.g., "Paragraph", "Heading", "ListItem").
   /// Used for context and debugging purposes.
   /// </summary>
   public string BlockType { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets additional context or metadata required for accurate reconstruction
   /// (e.g., heading level, list indentation, emphasis markers).
   /// </summary>
   public Dictionary<string, object> Metadata { get; set; } = [];

   /// <summary>
   /// Gets or sets a value indicating whether this block has been successfully translated.
   /// </summary>
   public bool IsTranslated { get; set; }
}
