using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Wraps translation calls with language validation and high-level retry orchestration.
/// LibreTranslate already implements low-level HTTP retries; this class adds stage-level
/// resilience by verifying language support and re-driving the whole request when the
/// server signals a retryable condition after its internal retries are exhausted.
/// </summary>
public class TranslationRetryService
{
    private readonly ILibreTranslateService _translateService;
    private readonly IPlaceholderService _placeholderService;
    private readonly ILogger<TranslationRetryService> _logger;
    private readonly int _stageMaxRetries;
    private readonly TimeSpan _stageRetryDelay;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationRetryService"/> class.
    /// </summary>
    /// <param name="translateService">The underlying translation service.</param>
    /// <param name="placeholderService">Service for handling named placeholders in translation text.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="stageMaxRetries">Maximum number of stage-level retries after LibreTranslate internal retries fail.</param>
    /// <param name="stageRetryDelaySeconds">Delay in seconds between stage-level retries.</param>
    public TranslationRetryService(
        ILibreTranslateService translateService,
        IPlaceholderService placeholderService,
        ILogger<TranslationRetryService> logger,
        int stageMaxRetries = 3,
        int stageRetryDelaySeconds = 30)
    {
        _translateService = translateService;
        _placeholderService = placeholderService;
        _logger = logger;
        _stageMaxRetries = stageMaxRetries;
        _stageRetryDelay = TimeSpan.FromSeconds(stageRetryDelaySeconds);
    }

    /// <summary>
    /// Translates text with stage-level retry and placeholder preservation.
    /// Named placeholders ({name}) are masked before translation and restored
    /// in the translated text to ensure correct grammar in target languages.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <param name="sourceLanguage">The source language code.</param>
    /// <param name="targetLanguage">The target language code.</param>
    /// <returns>A response containing the translation result with placeholders restored.</returns>
    public async Task<Response<TranslateResult>> TranslateWithRetryAsync(
        string text,
        string sourceLanguage,
        string targetLanguage)
    {
        // Fast path: identical languages
        if (string.Equals(sourceLanguage, targetLanguage, StringComparison.OrdinalIgnoreCase))
        {
            return Response<TranslateResult>.Ok(new TranslateResult { TranslatedText = text });
        }

        // Prepare text for translation: mask named placeholders to prevent translation engines
        // from modifying them. Placeholders will be restored after translation.
        (string preparedText, Func<string, string> restore) = _placeholderService.PrepareForTranslation(text);

        int attempt = 0;
        while (attempt <= _stageMaxRetries)
        {
            attempt++;
            _logger.LogDebug(
                "Translation stage attempt {Attempt}/{Max} for {TargetLanguage}.",
                attempt,
                _stageMaxRetries + 1,
                targetLanguage);

            var response = await _translateService.TranslateTextAsync(preparedText, sourceLanguage, targetLanguage);

            if (response.Success && response.Data != null)
            {
                // Restore original placeholder names in the translated text
                string restoredText = restore(response.Data.TranslatedText);
                response.Data.TranslatedText = _placeholderService.RestorePlaceholdersFromSource(text, restoredText);

                _logger.LogDebug(
                    "Translation succeeded for {TargetLanguage}. Placeholders preserved: {HasPlaceholders}.",
                    targetLanguage,
                    _placeholderService.HasPlaceholders(response.Data.TranslatedText));

                return response;
            }

            if (attempt > _stageMaxRetries)
            {
                break;
            }

            _logger.LogWarning(
                "Translation failed for {TargetLanguage} (attempt {Attempt}). Retrying in {Delay}s. Error: {Error}",
                targetLanguage,
                attempt,
                _stageRetryDelay.TotalSeconds,
                response.Message);

            await Task.Delay(_stageRetryDelay);
        }

        _logger.LogError(
            "Translation failed for {TargetLanguage} after {Attempts} stage attempts.",
            targetLanguage,
            attempt);

        return Response<TranslateResult>.Fail(
            $"Translation failed after {attempt} attempts for language '{targetLanguage}'.");
    }

    }
