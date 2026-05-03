using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Synchronizes JSON localization dictionaries by detecting added/removed keys in the default dictionary
/// and translating them into all target languages. Saves each target language dictionary immediately.
/// </summary>
public class LocalizationTranslationService(
    ILanguageService languageService,
    TranslationRetryService retryService,
    ISignalRPublisher signalRPublisher,
    IStringLocalizer<LocalizationTranslationService> localizer,
    ILogger<LocalizationTranslationService> logger) : ILocalizationTranslationService
{
    private readonly ILanguageService _languageService = languageService;
    private readonly TranslationRetryService _retryService = retryService;
    private readonly ISignalRPublisher _signalRPublisher = signalRPublisher;
    private readonly IStringLocalizer<LocalizationTranslationService> _localizer = localizer;
    private readonly ILogger<LocalizationTranslationService> _logger = logger;

    private string DefaultLanguage => "en";

    private string T(string text) => _localizer[text].Value;

    private string T(string text, object values) => _localizer[text, values].Value;

    /// <summary>
    /// Synchronizes JSON localization dictionaries for all target languages.
    /// Detects added/removed keys by comparing current default dictionary with previous snapshot.
    /// Translates keys per-language and saves each dictionary immediately.
    /// </summary>
    public async Task RunAsync(List<string> targetLanguages, StoringReport storingReport, Guid runId)
    {
        var report = new TranslationsReport();

        await _signalRPublisher.PublishStageAsync(
            runId,
            ProcessStage.TranslateJsonFiles,
            report,
            LocalizationMessageType.StageStarted,
            T("Synchronising JSON localization dictionaries."));

        try
        {
            // Load current and previous default dictionaries
            var currentDefault = await LoadDictionaryOrEmptyAsync(DefaultLanguage, report);
            report.DefaultDictionaryExists = currentDefault.Count > 0;
            report.DefaultDictionaryCount = currentDefault.Count;

            var oldResponse = await _languageService.GetLastStored();
            var previousDefault = oldResponse.Success && oldResponse.Data != null
                ? oldResponse.Data
                : new Dictionary<string, string>();

            // Detect changes
            string[] addedKeys = [.. currentDefault.Keys.Except(previousDefault.Keys, StringComparer.Ordinal).OrderBy(k => k, StringComparer.Ordinal)];
            string[] removedKeys = [.. previousDefault.Keys.Except(currentDefault.Keys, StringComparer.Ordinal).OrderBy(k => k, StringComparer.Ordinal)];

            report.AddedCount = addedKeys.Length;
            report.RemovedCount = removedKeys.Length;

            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.Progress,
                ProcessStage.TranslateJsonFiles,
                T("Detected {addedCount} added and {removedCount} removed keys in default dictionary.", new
                {
                    addedCount = addedKeys.Length,
                    removedCount = removedKeys.Length
                }),
                new TranslationProgressUpdate
                {
                    WorkItemId = "json:plan",
                    Stage = ProcessStage.TranslateJsonFiles,
                    Scope = T("JSON dictionaries"),
                    Unit = T("keys"),
                    TotalItems = (addedKeys.Length + removedKeys.Length) * targetLanguages.Count,
                    IsPlan = true
                });

            bool hasChanges = addedKeys.Length > 0 || removedKeys.Length > 0;

            if (!hasChanges)
            {
                _logger.LogInformation("JSON synchronisation skipped: no changes detected.");

                await _signalRPublisher.PublishStageAsync(
                    runId,
                    ProcessStage.TranslateJsonFiles,
                    report,
                    LocalizationMessageType.StageCompleted,
                    T("JSON localization dictionaries are up to date."));

                return;
            }

            // Process each target language independently
            foreach (string targetLanguage in targetLanguages)
            {
                var languageReport = new TranslationsReport
                {
                    DefaultDictionaryExists = report.DefaultDictionaryExists,
                    DefaultDictionaryCount = report.DefaultDictionaryCount,
                    AddedCount = addedKeys.Length,
                    RemovedCount = removedKeys.Length
                };

                await _signalRPublisher.PublishMessageAsync(
                    runId,
                    LocalizationMessageType.Progress,
                    ProcessStage.TranslateJsonFiles,
                    T("Starting JSON translations for '{targetLanguage}'.", new { targetLanguage }));

                var dictionaryResponse = await _languageService.GetDictionaryAsync(targetLanguage);
                var dictionary = dictionaryResponse.Success && dictionaryResponse.Data != null
                    ? dictionaryResponse.Data
                    : new Dictionary<string, string>();

                // Process removed keys
                foreach (string removedKey in removedKeys)
                {
                    if (dictionary.Remove(removedKey))
                    {
                        languageReport.RemovedCount++;
                    }
                }

                // Process added keys
                int translatedCount = 0;
                int skippedCount = 0;

                foreach (string addedKey in addedKeys)
                {
                    if (dictionary.ContainsKey(addedKey))
                    {
                        skippedCount++;
                        continue;
                    }

                    string phrase = currentDefault[addedKey];
                    var translationResponse = await _retryService.TranslateWithRetryAsync(phrase, DefaultLanguage, targetLanguage);

                    if (!translationResponse.Success || translationResponse.Data == null || string.IsNullOrWhiteSpace(translationResponse.Data.TranslatedText))
                    {
                        languageReport.Errors ??= [];
                        languageReport.Errors.Add(CreateError($"{targetLanguage}:{addedKey}", ErrorCode.TranslationFailed, translationResponse.Message));
                        _logger.LogWarning("JSON translation failed for key '{Key}' to '{TargetLanguage}'.", addedKey, targetLanguage);
                        continue;
                    }

                    dictionary[addedKey] = translationResponse.Data.TranslatedText.Trim();
                    translatedCount++;
                }

                languageReport.TranslatedCount = translatedCount;
                languageReport.SkippedCount = skippedCount;
                languageReport.ToTranslateCount = addedKeys.Length + removedKeys.Length;

                // Save dictionary immediately after this language is done
                await SaveDictionaryAsync(targetLanguage, dictionary, storingReport, languageReport, runId);

                await _signalRPublisher.PublishMessageAsync(
                    runId,
                    LocalizationMessageType.Progress,
                    ProcessStage.TranslateJsonFiles,
                    T("JSON translations for '{targetLanguage}' completed. Added: {addedCount}, Removed: {removedCount}, Skipped: {skippedCount}, Errors: {errorCount}.", new
                    {
                        targetLanguage,
                        addedCount = translatedCount,
                        removedCount = languageReport.RemovedCount,
                        skippedCount,
                        errorCount = languageReport.Errors?.Count ?? 0
                    }),
                    new TranslationProgressUpdate
                    {
                        WorkItemId = $"json:{targetLanguage}",
                        Stage = ProcessStage.TranslateJsonFiles,
                        Scope = T("JSON dictionaries"),
                        TargetLanguage = targetLanguage,
                        Unit = T("keys"),
                        TotalItems = addedKeys.Length + removedKeys.Length,
                        CompletedItems = translatedCount + languageReport.RemovedCount,
                        FailedItems = languageReport.Errors?.Count ?? 0,
                        SkippedItems = skippedCount,
                        SavedItems = 1
                    });
            }

            // Save snapshot for next comparison
            await _languageService.SaveOldTranslationAsync(currentDefault);

            await _signalRPublisher.PublishStageAsync(
                runId,
                ProcessStage.TranslateJsonFiles,
                report,
                LocalizationMessageType.StageCompleted,
                T("JSON localization dictionaries synchronised."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TranslateJsonFiles stage failed.");
            report.Errors ??= [];
            report.Errors.Add(CreateError("json", ErrorCode.TranslationFailed, ex.Message));

            await _signalRPublisher.PublishStageAsync(
                runId,
                ProcessStage.TranslateJsonFiles,
                report,
                LocalizationMessageType.StageFailed,
                ex.Message,
                isError: true);
            throw;
        }
    }

    private async Task<Dictionary<string, string>> LoadDictionaryOrEmptyAsync(string language, TranslationsReport report)
    {
        var response = await _languageService.GetDictionaryAsync(language);
        if (response.Success && response.Data != null)
        {
            return response.Data;
        }

        report.Errors ??= [];
        report.Errors.Add(CreateError(language, ErrorCode.DictionaryNotFound, response.Message));
        return new Dictionary<string, string>();
    }

    private async Task SaveDictionaryAsync(
        string language,
        Dictionary<string, string> dictionary,
        StoringReport storingReport,
        TranslationsReport report,
        Guid runId)
    {
        var result = await _languageService.SaveDictionaryAsync(new SingleTranslation
        {
            Language = language,
            Translations = dictionary
        });

        if (result.Success)
        {
            storingReport.SavedDictionaryFiles++;

            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.Progress,
                ProcessStage.TranslateJsonFiles,
                T("Saved dictionary for '{language}' ({entryCount} entries).", new { language, entryCount = dictionary.Count }));

            return;
        }

        report.Errors ??= [];
        var error = CreateError($"json/{language}", ErrorCode.StorageWriteFailed, result.Message);
        report.Errors.Add(error);
        storingReport.Errors.Add(error);

        await _signalRPublisher.PublishMessageAsync(
            runId,
            LocalizationMessageType.Progress,
            ProcessStage.TranslateJsonFiles,
            T("Failed to save dictionary for '{language}': {message}", new { language, message = result.Message }),
            isError: true);
    }

    private TranslationError CreateError(string source, ErrorCode code, string? details = null)
    {
        string errorText = T(ErrorCodeText.ErrorText(code));

        return new TranslationError
        {
            Source = source,
            Code = code,
            ErrorMessage = string.IsNullOrWhiteSpace(details)
                ? errorText
                : T("{error}: {details}", new { error = errorText, details })
        };
    }
}
