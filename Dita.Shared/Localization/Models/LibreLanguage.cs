namespace Dita.Shared.Localization.Models;

/// <summary>
/// Represents a language supported by the LibreTranslate service, including its code, name, and target languages for translation.
/// </summary>
public class LibreLanguage
{
   /// <summary>
   /// The language code (e.g., "en" for English, "es" for Spanish) used by the LibreTranslate API to identify the language.
   /// </summary>
   public string Code { get; set; } = string.Empty;
   /// <summary>
   /// The human-readable name of the language (e.g., "English", "Spanish") that corresponds to the language code. This is used for display purposes in user interfaces to help users identify the language more easily.
   /// </summary>
   public string Name { get; set; } = string.Empty;
   /// <summary>
   /// A list of language codes that represent the target languages into which this language can be translated using the LibreTranslate service. For example, if this language is "en" (English), the targets might include "es" (Spanish), "fr" (French), etc., indicating that English can be translated into those languages.
   /// </summary>
   public List<string> Targets { get; set; } = [];
}
