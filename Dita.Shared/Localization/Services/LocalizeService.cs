using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Implements writable localization lookup for client supplied phrases.
/// </summary>
/// <remarks>
/// The service first checks the target dictionary, then the configured default dictionary. If the phrase is not
/// present, it creates a default-language entry so the normal automatic translation pipeline can process it.
/// </remarks>
public sealed class LocalizeService(
   ILanguageService languageService,
   IPlaceholderService placeholderService,
   AutomaticTranslationSettings settings,
   ILogger<LocalizeService> logger) : ILocalizeService
{
   private readonly ILanguageService _languageService = languageService;
   private readonly IPlaceholderService _placeholderService = placeholderService;
   private readonly AutomaticTranslationSettings _settings = settings;
   private readonly ILogger<LocalizeService> _logger = logger;

   /// <inheritdoc />
   public async Task<Response<TextLocalizationResponse>> LocalizeAsync(
      TextLocalizationRequest request,
      CancellationToken cancellationToken = default)
   {
      if (request is null)
      {
         return Response<TextLocalizationResponse>.Fail("Localization request cannot be null.");
      }

      string phrase = request.Text?.Trim() ?? string.Empty;
      if (string.IsNullOrWhiteSpace(phrase))
      {
         return Response<TextLocalizationResponse>.Fail("Text cannot be null or empty.");
      }

      string defaultLanguage = NormalizeLanguage(_settings.DefaultLanguage, "en");
      string targetLanguage = ResolveTargetLanguage(request.TargetLanguage, defaultLanguage);

      cancellationToken.ThrowIfCancellationRequested();

      if (await TryReadDictionaryValueAsync(targetLanguage, phrase, cancellationToken).ConfigureAwait(false) is { } targetValue)
      {
         return Response<TextLocalizationResponse>.Ok(new TextLocalizationResponse
         {
            Text = phrase,
            LocalizedText = Format(targetValue, request.Values),
            TargetLanguage = targetLanguage,
            DefaultLanguage = defaultLanguage,
            FoundInTargetDictionary = true,
            AddedToDefaultDictionary = false,
            ResolvedFrom = TextResolutionSource.TargetDictionary
         });
      }

      if (!targetLanguage.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase)
         && await TryReadDictionaryValueAsync(defaultLanguage, phrase, cancellationToken).ConfigureAwait(false) is { } defaultValue)
      {
         return Response<TextLocalizationResponse>.Ok(new TextLocalizationResponse
         {
            Text = phrase,
            LocalizedText = Format(defaultValue, request.Values),
            TargetLanguage = targetLanguage,
            DefaultLanguage = defaultLanguage,
            FoundInTargetDictionary = false,
            AddedToDefaultDictionary = false,
            ResolvedFrom = TextResolutionSource.DefaultDictionary
         });
      }

      bool created = await EnsureDefaultEntryAsync(defaultLanguage, phrase, cancellationToken).ConfigureAwait(false);

      return Response<TextLocalizationResponse>.Ok(new TextLocalizationResponse
      {
         Text = phrase,
         LocalizedText = Format(phrase, request.Values),
         TargetLanguage = targetLanguage,
         DefaultLanguage = defaultLanguage,
         FoundInTargetDictionary = false,
         AddedToDefaultDictionary = created,
         ResolvedFrom = created ? TextResolutionSource.DefaultDictionaryCreated : TextResolutionSource.DefaultDictionary
      });
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

   private async Task<bool> EnsureDefaultEntryAsync(
      string defaultLanguage,
      string phrase,
      CancellationToken cancellationToken)
   {
      await _languageService.CreateMissingLanguageFilesAsync([defaultLanguage]).ConfigureAwait(false);
      cancellationToken.ThrowIfCancellationRequested();

      Response<bool> addResponse = await _languageService.AddTranslationEntryAsync(defaultLanguage, phrase, phrase).ConfigureAwait(false);
      if (addResponse.Success)
      {
         _logger.LogInformation("Added localization key '{Phrase}' to default dictionary '{DefaultLanguage}'.", phrase, defaultLanguage);
         return true;
      }

      string? currentValue = await TryReadDictionaryValueAsync(defaultLanguage, phrase, cancellationToken).ConfigureAwait(false);
      bool alreadyExists = !string.IsNullOrWhiteSpace(currentValue);
      if (!alreadyExists)
      {
         _logger.LogWarning(
            "Could not add localization key '{Phrase}' to default dictionary '{DefaultLanguage}'. Message: {Message}",
            phrase,
            defaultLanguage,
            addResponse.Message);
      }

      return false;
   }

   private string Format(string template, Dictionary<string, string>? values)
      => _placeholderService.Format(template, values);

   private static string ResolveTargetLanguage(string? requestedLanguage, string defaultLanguage)
   {
      if (!string.IsNullOrWhiteSpace(requestedLanguage))
      {
         return NormalizeLanguage(requestedLanguage, defaultLanguage);
      }

      string currentCulture = CultureInfo.CurrentUICulture.Name;
      return string.IsNullOrWhiteSpace(currentCulture)
         ? defaultLanguage
         : NormalizeLanguage(currentCulture, defaultLanguage);
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
