using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Hubs;
using Dita.Shared.Localization.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Provides translation services using the LibreTranslate API.
/// </summary>
/// <remarks>
/// This service handles communication with LibreTranslate API for text and file translation operations, language
/// detection, and retrieval of available languages. It includes retry logic with exponential backoff and intelligent
/// translation validation for text translations.
/// </remarks>
public class LibreTranslateService(AutomaticTranslationSettings settings, ILibreTranslateHttpClientFactory httpClientFactory, IHubContext<LocalizationHub> hub, ILogger<LibreTranslateService> logger) : ILibreTranslateService
{
   private readonly AutomaticTranslationSettings _settings = settings;
   private readonly HttpClient libreClient = httpClientFactory.LibreClient;
   private readonly IHubContext<LocalizationHub> _hub = hub;
   private readonly ILogger<LibreTranslateService> _logger = logger;

   private readonly JsonSerializerOptions _options = new()
   {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DefaultIgnoreCondition = JsonIgnoreCondition.Never,
      ReferenceHandler = ReferenceHandler.IgnoreCycles
   };

   /// <summary>
   /// Measures the latency of the LibreTranslate server.
   /// </summary>
   /// <returns>
   /// A <see cref="Response{T}"/> containing the server latency in milliseconds, or a failure response if the
   /// measurement failed.
   /// </returns>
   private Response<int> ServerLatency()
   {
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();
      var response = libreClient.GetAsync("/").Result;
      stopwatch.Stop();
      if(response.IsSuccessStatusCode)
      {
         return new Response<int>
         {
            Success = true,
            Data = (int)stopwatch.ElapsedMilliseconds,
            Message = "Server latency measured successfully."
         };
      }
      else
      {
         _logger.LogError("Failed to measure server latency. Status code: {StatusCode}", response.StatusCode);
         return new Response<int>
         {
            Success = false,
            Data = 0,
            Message = "Failed to measure server latency."
         };
      }
   }

   /// <summary>
   /// Detects the language of the provided text.
   /// </summary>
   /// <param name="text">The text to detect the language for.</param>
   /// <returns>
   /// A <see cref="Response{T}"/> containing <see cref="Detections"/> with detected language information, or a failure
   /// response if detection failed.
   /// </returns>
   public async Task<Response<Detections>> DetectLanguageAsync(string text)
   {
      Dictionary<string, string> formFields = new()
      {
         { "q", text }
      };

      if(_settings.NeedsKey && _settings.Key is not null)
      {
         formFields.Add("api_key", _settings.Key);
      }
      var response = await libreClient.PostAsync(_settings.Address + _settings.DetectLanguageEndpoint, new FormUrlEncodedContent(formFields));
      if(!response.IsSuccessStatusCode)
      {
         _logger.LogError("Failed to detect language. Status code: {StatusCode}", response.StatusCode);
         return new Response<Detections>
         {
            Success = false,
            Message = "Failed to detect language."
         };
      }

      var content = await response.Content.ReadAsStringAsync();
      var detections = JsonSerializer.Deserialize<Detections>(content, _options);
      return new Response<Detections>
      {
         Success = true,
         Data = detections ?? new(),
         Message = "Language detected successfully."
      };
   }

   /// <summary>
   /// Retrieves the list of available languages supported by LibreTranslate.
   /// </summary>
   /// <returns>
   /// A <see cref="Response{T}"/> containing an array of language codes, or a failure response if retrieval failed
   /// after multiple attempts.
   /// </returns>
   /// <remarks>
   /// This method includes retry logic with exponential backoff (up to 5 attempts) to handle temporary network
   /// failures.
   /// </remarks>
   public async Task<Response<string[]>> GetAvailableLanguagesAsync()
   {
      int retries = 0;
      while(retries < 5)
      {
         try
         {
            var response = await libreClient.GetAsync(_settings.Address + _settings.LanguagesEndpoint);
            if(!response.IsSuccessStatusCode)
            {
               retries++;
               _logger.LogWarning("Failed to get available languages. Status code: {StatusCode}. Retrying {RetryCount}/5", response.StatusCode, retries);
               await Task.Delay(1000 * retries);
               continue;
            }
            else
            {
               var content = await response.Content.ReadAsStringAsync();
               var languages = JsonSerializer.Deserialize<List<LibreLanguage>>(content, _options);
               if(languages is null || languages.Count == 0)
               {
                  _logger.LogWarning("No languages found in the response.");
                  return new Response<string[]>
                  {
                     Success = false,
                     Message = "No languages found."
                  };
               }
               _logger.LogDebug("Found {LanguageCount} languages", languages.Count);
               return new Response<string[]>
               {
                  Success = true,
                  Data = [.. languages.Select(l => l.Code)],
                  Message = "Available languages retrieved successfully."
               };
            }
         }
         catch(Exception e)
         {
            _logger.LogError(e, "An error occurred while getting available languages.");
            return new Response<string[]>
            {
               Success = false,
               Message = "An error occurred while getting available languages."
            };
         }
      }
      _logger.LogError("Failed to get available languages after 5 attempts.");
      return new Response<string[]>
      {
         Success = false,
         Message = "Failed to get available languages after multiple attempts."
      };
   }

   /// <summary>
   /// Translates a file from one language to another.
   /// </summary>
   /// <param name="fileStream">The stream containing the file to translate.</param>
   /// <param name="sourceLanguage">The source language code (e.g., "en", "cs").</param>
   /// <param name="targetLanguage">The target language code (e.g., "en", "cs").</param>
   /// <param name="fileName">The name of the file being translated.</param>
   /// <returns>
   /// A <see cref="Response{T}"/> containing <see cref="TranslateFileResult"/> with the translated file URL, or a
   /// failure response if translation failed.
   /// </returns>
   /// <remarks>
   /// This method includes retry logic with exponential backoff (up to 5 attempts). File translations do not include
   /// intelligent translation validation.
   /// </remarks>
   public async Task<Response<TranslateFileResult>> TranslateFileAsync(Stream fileStream, string sourceLanguage, string targetLanguage, string fileName)
   {
      int retries = 0;
      while(retries < 5)
      {
         try
         {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "file", fileName);
            content.Add(new StringContent(sourceLanguage), "source");
            content.Add(new StringContent(targetLanguage), "target");
            if(_settings.NeedsKey && _settings.Key is not null)
            {
               content.Add(new StringContent(_settings.Key), "api_key");
            }
            var response = await libreClient.PostAsync(_settings.Address + _settings.TranslateFileEndpoint, content);
            if(!response.IsSuccessStatusCode)
            {
               retries++;
               _logger.LogWarning("Failed to translate file. Status code: {StatusCode}. Retrying {RetryCount}/5", response.StatusCode, retries);
               await Task.Delay(1000 * retries);
               continue;
            }
            else
            {
               var responseContent = await response.Content.ReadAsStringAsync();
               var translateResult = JsonSerializer.Deserialize<TranslateFileResult>(responseContent, _options);
               return new Response<TranslateFileResult>
               {
                  Success = true,
                  Data = translateResult ?? new(),
                  Message = "File translated successfully."
               };
            }
         }
         catch(Exception e)
         {
            _logger.LogError(e, "An error occurred while translating the file.");
            return new Response<TranslateFileResult>
            {
               Success = false,
               Message = "An error occurred while translating the file."
            };
         }
      }
      _logger.LogError("Failed to translate file after 5 attempts.");
      return new Response<TranslateFileResult>
      {
         Success = false,
         Message = "Failed to translate file after multiple attempts."
      };
   }

   /// <summary>
   /// Translates a file from any language to the specified target language.
   /// </summary>
   /// <param name="fileStream">The stream containing the file to translate.</param>
   /// <param name="targetLanguage">The target language code (e.g., "en", "cs").</param>
   /// <param name="fileName">The name of the file being translated.</param>
   /// <returns>
   /// A <see cref="Response{T}"/> containing <see cref="TranslateFileResult"/> with the translated file URL, or a
   /// failure response if translation failed.
   /// </returns>
   /// <remarks>
   /// This method automatically detects the source language using "auto".
   /// </remarks>
   public async Task<Response<TranslateFileResult>> TranslateFileAsync(Stream fileStream, string targetLanguage, string fileName)
   {
      return await TranslateFileAsync(fileStream, "auto", targetLanguage, fileName);
   }

   /// <summary>
   /// Translates text from one language to another with intelligent validation and retry logic.
   /// </summary>
   /// <param name="text">The text to translate.</param>
   /// <param name="sourceLanguage">The source language code (e.g., "en", "cs").</param>
   /// <param name="targetLanguage">The target language code (e.g., "en", "cs").</param>
   /// <returns>
   /// A <see cref="Response{T}"/> containing <see cref="TranslateResult"/> with the translated text, or a failure
   /// response if translation failed.
   /// </returns>
   /// <remarks>
   /// This method includes: - Retry logic with exponential backoff (up to 5 attempts) - Intelligent validation that
   /// checks for empty/null translations - Case-insensitive validation that retries with lowercase if translation
   /// equals source - Returns the original casing if only casing differs, otherwise returns the translation result
   /// </remarks>
   public async Task<Response<TranslateResult>> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
   {
      Dictionary<string, string> formFields = new()
      {
         { "q", text },
         { "source", sourceLanguage },
         { "target", targetLanguage }
      };
      if(_settings.NeedsKey && _settings.Key is not null)
      {
         formFields.Add("api_key", _settings.Key);
      }

      int retries = 0;
      while(retries < 5)
      {
         try
         {
            var response = await libreClient.PostAsync(_settings.Address + _settings.TranslateEndpoint, new FormUrlEncodedContent(formFields));
            if(!response.IsSuccessStatusCode)
            {
               retries++;
               _logger.LogWarning("Failed to translate text. Status code: {StatusCode}. Retrying {RetryCount}/5", response.StatusCode, retries);
               await Task.Delay(1000 * retries);
               continue;
            }
            else
            {
               var content = await response.Content.ReadAsStringAsync();
               var translateResult = JsonSerializer.Deserialize<TranslateResult>(content, _options);

               if(translateResult is null)
               {
                  return new Response<TranslateResult>
                  {
                     Success = false,
                     Data = new(),
                     Message = "Failed to deserialize translation result."
                  };
               }

               // Validate and potentially retry translation
               var validatedResult = await ValidateAndRetryTranslationAsync(text, translateResult, sourceLanguage, targetLanguage);

               return new Response<TranslateResult>
               {
                  Success = true,
                  Data = validatedResult,
                  Message = "Text translated successfully."
               };
            }
         }
         catch(Exception ex)
         {
            _logger.LogError(ex, "An error occurred while translating text.");
            return new Response<TranslateResult>
            {
               Success = false,
               Message = "An error occurred while translating text."
            };
         }
      }
      _logger.LogError("Failed to translate text after 5 attempts.");
      return new Response<TranslateResult>
      {
         Success = false,
         Message = "Failed to translate text after multiple attempts."
      };
   }

   /// <summary>
   /// Validates the translated text and performs intelligent retry logic if needed.
   /// </summary>
   /// <param name="originalText">The original text that was translated.</param>
   /// <param name="translationResult">The initial translation result.</param>
   /// <param name="sourceLanguage">The source language code.</param>
   /// <param name="targetLanguage">The target language code.</param>
   /// <returns>
   /// The validated or re-translated <see cref="TranslateResult"/> .
   /// </returns>
   /// <remarks>
   /// This method implements the following validation logic: 1. If translated text is null or empty, retry the
   /// translation. 2. If translated text equals original text (case-insensitive), attempt lowercase translation. 3. If
   /// lowercase translation differs from original, use it; otherwise use original translation.
   /// </remarks>
   private async Task<TranslateResult> ValidateAndRetryTranslationAsync(string originalText, TranslateResult translationResult, string sourceLanguage, string targetLanguage)
   {
      var translatedText = translationResult.TranslatedText;

      // Check 1: If translation is empty or null, retry
      if(string.IsNullOrWhiteSpace(translatedText))
      {
         _logger.LogWarning("Translation resulted in empty text. Retrying translation for text: {OriginalText}", originalText);
         return await RetryTranslationAsync(originalText, sourceLanguage, targetLanguage);
      }

      // Check 2: If translation is the same as original (case-insensitive), try lowercase
      if(translatedText.Equals(originalText, StringComparison.OrdinalIgnoreCase))
      {
         // Check if text has mixed casing
         if(HasMixedCasing(originalText))
         {
            _logger.LogDebug("Translation equals original text (case-insensitive). Retrying with lowercase for: {OriginalText}", originalText);

            var lowercaseResult = await RetryTranslationAsync(originalText.ToLowerInvariant(), sourceLanguage, targetLanguage);

            // If lowercase result is different from original (case-insensitive), use it
            if(!lowercaseResult.TranslatedText.Equals(originalText, StringComparison.OrdinalIgnoreCase))
            {
               _logger.LogDebug("Lowercase translation differs from original. Using lowercase result: {TranslatedText}", lowercaseResult.TranslatedText);
               return lowercaseResult;
            }

            // Otherwise, return original translation with proper casing
            _logger.LogDebug("Lowercase translation is same as original. Using original translation with proper casing.");
            return translationResult;
         }
      }

      return translationResult;
   }

   /// <summary>
   /// Checks if the given text contains both uppercase and lowercase characters.
   /// </summary>
   /// <param name="text">The text to check.</param>
   /// <returns>True if the text contains both cases, false otherwise.</returns>
   private static bool HasMixedCasing(string text)
   {
      bool hasUpper = text.Any(char.IsUpper);
      bool hasLower = text.Any(char.IsLower);
      return hasUpper && hasLower;
   }

   /// <summary>
   /// Retries the translation of text with exponential backoff.
   /// </summary>
   /// <param name="text">The text to translate.</param>
   /// <param name="sourceLanguage">The source language code.</param>
   /// <param name="targetLanguage">The target language code.</param>
   /// <returns>The <see cref="TranslateResult"/> from the retry attempt.</returns>
   /// <remarks>
   /// This method performs up to 3 retry attempts with exponential backoff (1s, 2s, 3s). If all attempts fail, returns
   /// an empty TranslateResult.
   /// </remarks>
   private async Task<TranslateResult> RetryTranslationAsync(string text, string sourceLanguage, string targetLanguage)
   {
      Dictionary<string, string> formFields = new()
      {
         { "q", text },
         { "source", sourceLanguage },
         { "target", targetLanguage }
      };

      if(_settings.NeedsKey && _settings.Key is not null)
      {
         formFields.Add("api_key", _settings.Key);
      }

      int retries = 0;
      while(retries < 3)
      {
         try
         {
            var response = await libreClient.PostAsync(_settings.Address + _settings.TranslateEndpoint, new FormUrlEncodedContent(formFields));
            if(!response.IsSuccessStatusCode)
            {
               retries++;
               _logger.LogWarning("Retry translation failed. Status code: {StatusCode}. Retry attempt {RetryCount}/3", response.StatusCode, retries);
               await Task.Delay(1000 * retries);
               continue;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TranslateResult>(content, _options);
            return result ?? new TranslateResult { TranslatedText = text };
         }
         catch(Exception ex)
         {
            _logger.LogWarning(ex, "Error during retry translation attempt {RetryCount}/3", retries);
            retries++;
            await Task.Delay(1000 * retries);
         }
      }

      _logger.LogError("Failed to retry translation after 3 attempts for text: {Text}", text);
      return new TranslateResult { TranslatedText = text };
   }
}