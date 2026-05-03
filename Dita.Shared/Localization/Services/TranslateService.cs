using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.ScheduledTranslationService;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Implements read-only dynamic translation for client supplied text.
/// </summary>
/// <remarks>
/// Dictionaries are used as a fast cache only. Missing phrases are sent to the translation server and are never
/// written back to any locale file by this service.
/// </remarks>
public sealed class TranslateService(
   ILanguageService languageService,
   ILibreTranslateService libreTranslateService,
   TranslationRetryService retryService,
   IPlaceholderService placeholderService,
   AutomaticTranslationSettings settings,
   ILogger<TranslateService> logger) : ITranslateService
{
   private readonly ILanguageService _languageService = languageService;
   private readonly ILibreTranslateService _libreTranslateService = libreTranslateService;
   private readonly TranslationRetryService _retryService = retryService;
   private readonly IPlaceholderService _placeholderService = placeholderService;
   private readonly AutomaticTranslationSettings _settings = settings;
   private readonly ILogger<TranslateService> _logger = logger;

   /// <inheritdoc />
   public async Task<Response<TextTranslationResponse>> TranslateAsync(
      TextTranslationRequest request,
      CancellationToken cancellationToken = default)
   {
      if (request is null)
      {
         return Response<TextTranslationResponse>.Fail("Translation request cannot be null.");
      }

      string phrase = request.Text?.Trim() ?? string.Empty;
      if (string.IsNullOrWhiteSpace(phrase))
      {
         return Response<TextTranslationResponse>.Fail("Text cannot be null or empty.");
      }

      string sourceLanguage = NormalizeLanguage(request.SourceLanguage, _settings.DefaultLanguage ?? "en");
      string targetLanguage = ResolveTargetLanguage(request.TargetLanguage, sourceLanguage);

      cancellationToken.ThrowIfCancellationRequested();

      if (await TryReadDictionaryValueAsync(targetLanguage, phrase, cancellationToken).ConfigureAwait(false) is { } dictionaryValue)
      {
         return Response<TextTranslationResponse>.Ok(new TextTranslationResponse
         {
            Text = phrase,
            TranslatedText = Format(dictionaryValue, request.Values),
            SourceLanguage = sourceLanguage,
            TargetLanguage = targetLanguage,
            FoundInTargetDictionary = true,
            TranslationServerUsed = false,
            ResolvedFrom = TextResolutionSource.TargetDictionary
         });
      }

      if (AreLanguagesEquivalent(sourceLanguage, targetLanguage))
      {
         return Response<TextTranslationResponse>.Ok(new TextTranslationResponse
         {
            Text = phrase,
            TranslatedText = Format(phrase, request.Values),
            SourceLanguage = sourceLanguage,
            TargetLanguage = targetLanguage,
            FoundInTargetDictionary = false,
            TranslationServerUsed = false,
            ResolvedFrom = TextResolutionSource.OriginalText
         });
      }

      Response<TranslateResult> translationResponse = await TranslateWithPlaceholderSupportAsync(
         phrase,
         sourceLanguage,
         targetLanguage,
         request.Values,
         cancellationToken).ConfigureAwait(false);

      if (!translationResponse.Success || translationResponse.Data is null || string.IsNullOrWhiteSpace(translationResponse.Data.TranslatedText))
      {
         _logger.LogWarning(
            "Dynamic translation failed. Source={SourceLanguage}, Target={TargetLanguage}, Text={Text}, Message={Message}",
            sourceLanguage,
            targetLanguage,
            phrase,
            translationResponse.Message);

         return Response<TextTranslationResponse>.Fail(translationResponse.Message ?? "Translation failed.");
      }

      return Response<TextTranslationResponse>.Ok(new TextTranslationResponse
      {
         Text = phrase,
         TranslatedText = Format(translationResponse.Data.TranslatedText.Trim(), request.Values),
         SourceLanguage = sourceLanguage,
         TargetLanguage = targetLanguage,
         FoundInTargetDictionary = false,
         TranslationServerUsed = true,
         ResolvedFrom = TextResolutionSource.TranslationServer
      });
   }

   private async Task<Response<TranslateResult>> TranslateWithPlaceholderSupportAsync(
      string phrase,
      string sourceLanguage,
      string targetLanguage,
      Dictionary<string, string>? values,
      CancellationToken cancellationToken)
   {
      if (values is null || values.Count == 0)
      {
         return await _retryService.TranslateWithRetryAsync(phrase, sourceLanguage, targetLanguage).ConfigureAwait(false);
      }

      (string preparedText, Func<string, string> restore) = _placeholderService.PrepareForTranslation(phrase, values);

      cancellationToken.ThrowIfCancellationRequested();
      Response<TranslateResult> response = await _libreTranslateService.TranslateTextAsync(preparedText, sourceLanguage, targetLanguage).ConfigureAwait(false);
      if (!response.Success || response.Data is null)
      {
         return response;
      }

      return Response<TranslateResult>.Ok(new TranslateResult
      {
         TranslatedText = restore(response.Data.TranslatedText)
      }, response.Message);
   }

   private async Task<string?> TryReadDictionaryValueAsync(
      string language,
      string phrase,
      CancellationToken cancellationToken)
   {
      foreach (string candidate in GetLanguageCandidates(language))
      {
         cancellationToken.ThrowIfCancellationRequested();
         Response<Dictionary<string, string>> dictionaryResponse = await _languageService.GetDictionaryAsync(candidate).ConfigureAwait(false);
         if (!dictionaryResponse.Success || dictionaryResponse.Data is null)
         {
            continue;
         }

         if (dictionaryResponse.Data.TryGetValue(phrase, out string? value) && !string.IsNullOrWhiteSpace(value))
         {
            return value;
         }
      }

      return null;
   }

   private string Format(string template, Dictionary<string, string>? values)
      => _placeholderService.Format(template, values);

   private static string ResolveTargetLanguage(string? requestedLanguage, string fallback)
   {
      if (!string.IsNullOrWhiteSpace(requestedLanguage))
      {
         return NormalizeLanguage(requestedLanguage, fallback);
      }

      string currentCulture = CultureInfo.CurrentUICulture.Name;
      return string.IsNullOrWhiteSpace(currentCulture)
         ? fallback
         : NormalizeLanguage(currentCulture, fallback);
   }

   private static string NormalizeLanguage(string? language, string fallback)
   {
      if (string.IsNullOrWhiteSpace(language))
      {
         return fallback;
      }

      try
      {
         return CultureInfo.GetCultureInfo(language).Name;
      }
      catch (CultureNotFoundException)
      {
         return language.Trim();
      }
   }

   private static bool AreLanguagesEquivalent(string sourceLanguage, string targetLanguage)
   {
      if (sourceLanguage.Equals(targetLanguage, StringComparison.OrdinalIgnoreCase))
      {
         return true;
      }

      try
      {
         CultureInfo sourceCulture = CultureInfo.GetCultureInfo(sourceLanguage);
         CultureInfo targetCulture = CultureInfo.GetCultureInfo(targetLanguage);
         return sourceCulture.TwoLetterISOLanguageName.Equals(targetCulture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase);
      }
      catch (CultureNotFoundException)
      {
         return false;
      }
   }

   private static IEnumerable<string> GetLanguageCandidates(string language)
   {
      List<string> candidates = [language];

      try
      {
         CultureInfo culture = CultureInfo.GetCultureInfo(language);
         if (!string.IsNullOrWhiteSpace(culture.TwoLetterISOLanguageName)
            && !culture.TwoLetterISOLanguageName.Equals(language, StringComparison.OrdinalIgnoreCase))
         {
            candidates.Add(culture.TwoLetterISOLanguageName);
         }
      }
      catch (CultureNotFoundException)
      {
         int separatorIndex = language.IndexOfAny(['-', '_']);
         if (separatorIndex > 1)
         {
            candidates.Add(language[..separatorIndex]);
         }
      }

      return candidates.Distinct(StringComparer.OrdinalIgnoreCase);
   }
}
