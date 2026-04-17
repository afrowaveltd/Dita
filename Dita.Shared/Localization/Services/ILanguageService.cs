using Afrowave.SharedTools.Models.Localization;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Defines operations for managing application languages, locale files, and translation dictionaries.
/// </summary>
/// <remarks>
/// Implementations load language metadata from a bundled JSON file and read/write per-language locale files
/// stored on the file system. All file I/O methods return a <see cref="Response{T}"/> that carries both the result
/// and a success/failure indicator so callers never need to catch exceptions for expected error paths.
/// </remarks>
public interface ILanguageService
{
   /// <summary>
   /// Gets the full list of languages known to the application, loaded from the bundled <c>languages.json</c> file.
   /// </summary>
   List<Language> Languages { get; }

   /// <summary>
   /// Creates an empty locale file for every language in <paramref name="languages"/> that does not already have one.
   /// </summary>
   /// <param name="languages">The list of language codes to check and, if needed, create files for.</param>
   /// <returns>
   /// A dictionary keyed by language code whose value is <see langword="true"/> if the file was created
   /// or <see langword="false"/> if it already existed.
   /// </returns>
   Task<Dictionary<string, bool>> CreateMissingLanguageFilesAsync(List<string> languages);

   /// <summary>
   /// Loads and returns the translation dictionaries for all languages that have a locale file on disk.
   /// </summary>
   /// <returns>
   /// A <see cref="Response{T}"/> wrapping the list of <see cref="SingleTranslation"/> objects, one per language.
   /// </returns>
   Task<Response<List<SingleTranslation>>> GetAllDictionariesAsync();

   /// <summary>
   /// Loads the translation dictionary for a single language identified by its ISO code.
   /// </summary>
   /// <param name="code">The ISO language code (e.g. <c>"en"</c>, <c>"cs"</c>).</param>
   /// <returns>
   /// A <see cref="Response{T}"/> wrapping the key/value translation pairs for the requested language,
   /// or a failure response if the locale file does not exist or cannot be read.
   /// </returns>
   Task<Response<Dictionary<string, string>>> GetDictionaryAsync(string code);

   /// <summary>
   /// Returns metadata for the language identified by <paramref name="code"/>.
   /// </summary>
   /// <param name="code">The ISO language code to look up.</param>
   /// <returns>
   /// A <see cref="Response{T}"/> wrapping the matching <see cref="Language"/>,
   /// or <see langword="null"/> / a failure response if the code is not recognised.
   /// </returns>
   Response<Language>? GetLanguageByCode(string code);

   /// <summary>
   /// Returns the English display names of all languages known to the application.
   /// </summary>
   /// <returns>A list of language name strings.</returns>
   List<string> GetLanguageNames();

   /// <summary>
   /// Reads the most recently saved translation dictionary from the temporary storage path.
   /// </summary>
   /// <returns>
   /// A <see cref="Response{T}"/> wrapping the key/value pairs from the last stored translation file,
   /// or a failure response if no file has been saved yet.
   /// </returns>
   Task<Response<Dictionary<string, string>>> GetLastStored();

   /// <summary>
   /// Returns the subset of <see cref="Languages"/> that are marked as required for the application.
   /// </summary>
   /// <returns>A <see cref="Response{T}"/> wrapping the list of required <see cref="Language"/> objects.</returns>
   Response<List<Language>> GetRequiredLanguagesAsync();

   /// <summary>
   /// Returns full language metadata for each code supplied in <paramref name="languages"/>.
   /// </summary>
   /// <param name="languages">The list of ISO language codes whose metadata is requested.</param>
   /// <returns>
   /// A <see cref="Response{T}"/> wrapping the matching <see cref="Language"/> objects.
   /// Codes that are not recognised are silently skipped.
   /// </returns>
   Response<List<Language>> GetSelectedLanguagesInfo(List<string> languages);

   /// <summary>
   /// Determines whether the language identified by <paramref name="code"/> uses a right-to-left script.
   /// </summary>
   /// <param name="code">The ISO language code to check.</param>
   /// <returns><see langword="true"/> if the language is RTL; otherwise <see langword="false"/>.</returns>
   bool IsRtl(string code);

   /// <summary>
   /// Persists a single <see cref="SingleTranslation"/> to its corresponding locale file, replacing any existing content.
   /// </summary>
   /// <param name="data">The translation object containing the language code and key/value pairs to save.</param>
   /// <returns>A <see cref="Response{T}"/> indicating whether the save succeeded.</returns>
   Task<Response<bool>> SaveDictionaryAsync(SingleTranslation data);

   /// <summary>
   /// Saves a flat key/value translation dictionary to the temporary "last stored" file for later retrieval.
   /// </summary>
   /// <param name="data">The translation key/value pairs to persist.</param>
   /// <returns>A <see cref="Response{T}"/> indicating whether the save succeeded.</returns>
   Task<Response<bool>> SaveOldTranslationAsync(Dictionary<string, string> data);

   /// <summary>
   /// Saves a collection of <see cref="SingleTranslation"/> objects, one per language, to their respective locale files.
   /// </summary>
   /// <param name="tree">The list of translations to persist.</param>
   /// <returns>
   /// A <see cref="Response{T}"/> wrapping a dictionary keyed by language code whose value indicates
   /// whether that language's file was saved successfully.
   /// </returns>
   Task<Response<Dictionary<string, bool>>> SaveTranslationsAsync(List<SingleTranslation> tree);

   /// <summary>
   /// Adds a new key/value entry to the specified language's locale file.
   /// Returns a failure response (without modifying the file) if the key already exists.
   /// </summary>
   /// <param name="code">The ISO language code identifying which locale file to update.</param>
   /// <param name="key">The translation key to add. Must not already exist in the file.</param>
   /// <param name="value">The translated string to associate with <paramref name="key"/>.</param>
   /// <returns>A <see cref="Response{T}"/> indicating whether the entry was added successfully.</returns>
   Task<Response<bool>> AddTranslationEntryAsync(string code, string key, string value);

   /// <summary>
   /// Removes the entry with the specified key from the language's locale file.
   /// Returns a failure response if the key does not exist.
   /// </summary>
   /// <param name="code">The ISO language code identifying which locale file to update.</param>
   /// <param name="key">The translation key to remove.</param>
   /// <returns>A <see cref="Response{T}"/> indicating whether the entry was removed successfully.</returns>
   Task<Response<bool>> RemoveTranslationEntryAsync(string code, string key);

   /// <summary>
   /// Creates or overwrites the entry with the specified key in the language's locale file (upsert).
   /// Unlike <see cref="AddTranslationEntryAsync"/>, this method always writes the value regardless of whether
   /// the key already exists.
   /// </summary>
   /// <param name="code">The ISO language code identifying which locale file to update.</param>
   /// <param name="key">The translation key to create or overwrite.</param>
   /// <param name="value">The translated string to associate with <paramref name="key"/>.</param>
   /// <returns>A <see cref="Response{T}"/> indicating whether the entry was written successfully.</returns>
   Task<Response<bool>> UpdateTranslationEntryAsync(string code, string key, string value);
}
