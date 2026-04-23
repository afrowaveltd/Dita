namespace Dita.Shared.Localization.Enums;
/// <summary>
/// Defines the target for translation operations, specifying whether 
/// translations should be applied to languages, 
/// JSON files, or Markdown files.
/// </summary>
public enum TranslationTarget
{
   /// <summary>
   /// Translate the language names from the JSON files to the target language.
   /// </summary>
   Languages = 0,
   /// <summary>
   /// Translate JSON Localization files to the target language.
   /// </summary>
   JsonFiles = 1,
   /// <summary>
   /// Translate Markdown files to the target language.
   /// </summary>
   MDFiles = 2
}
