using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Defines the contract for LibreTranslate service operations including language detection, translation, and file translation capabilities.
/// </summary>
public interface ILibreTranslateService
{
   /// <summary>
   /// Detects the language of the provided text.
   /// </summary>
   /// <param name="text">The text to analyze for language detection.</param>
   /// <returns>A response containing detected language information.</returns>
   Task<Response<Detections>> DetectLanguageAsync(string text);

   /// <summary>
   /// Retrieves the list of available languages supported by the translation service.
   /// </summary>
   /// <returns>A response containing an array of available language codes.</returns>
   Task<Response<string[]>> GetAvailableLanguagesAsync();

   /// <summary>
   /// Measures the server latency in milliseconds.
   /// </summary>
   /// <returns>A response containing the server latency value in milliseconds.</returns>
   Response<int> ServerLatency();

   /// <summary>
   /// Translates a file to the specified target language with automatic source language detection.
   /// </summary>
   /// <param name="fileStream">The stream containing the file content to translate.</param>
   /// <param name="targetLanguage">The target language code for translation.</param>
   /// <param name="fileName">The name of the file being translated.</param>
   /// <returns>A response containing the translation result with the translated file content.</returns>
   Task<Response<TranslateFileResult>> TranslateFileAsync(Stream fileStream, string targetLanguage, string fileName);

   /// <summary>
   /// Translates a file from the specified source language to the target language.
   /// </summary>
   /// <param name="fileStream">The stream containing the file content to translate.</param>
   /// <param name="sourceLanguage">The source language code of the file.</param>
   /// <param name="targetLanguage">The target language code for translation.</param>
   /// <param name="fileName">The name of the file being translated.</param>
   /// <returns>A response containing the translation result with the translated file content.</returns>
   Task<Response<TranslateFileResult>> TranslateFileAsync(Stream fileStream, string sourceLanguage, string targetLanguage, string fileName);

   /// <summary>
   /// Translates text from the specified source language to the target language.
   /// </summary>
   /// <param name="text">The text to translate.</param>
   /// <param name="sourceLanguage">The source language code of the text.</param>
   /// <param name="targetLanguage">The target language code for translation.</param>
   /// <returns>A response containing the translation result with the translated text.</returns>
   Task<Response<TranslateResult>> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage);
}