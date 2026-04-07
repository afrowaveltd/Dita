namespace Dita.Shared.Localization.Models;
/// <summary>
/// Represents a single translation for a specific language, containing the language code and a dictionary of translation key-value pairs.
/// </summary>
public class SingleTranslation
{
   /// <summary>
   /// Gets or sets the language code for this translation (e.g., "en" for English, "fr" for French).
   /// </summary>
   public string Language { get; set; } = string.Empty;
   /// <summary>
   /// Gets or sets the dictionary of translation key-value pairs, where the key is the identifier for the text to be translated and the value is the translated text in the specified language.
   /// </summary>
   public Dictionary<string, string> Translations { get; set; } = [];
}
