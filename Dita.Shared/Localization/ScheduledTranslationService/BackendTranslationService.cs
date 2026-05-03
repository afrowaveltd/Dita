using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Orchestrates the automatic translation pipeline by delegating to specialized sub-services.
/// Coordinates server validation, country name translations, JSON dictionary synchronization,
/// and Markdown document translations with real-time SignalR reporting.
/// </summary>
public class BackendTranslationService(
    IConfiguration configuration,
    ILibreTranslateService translateService,
    ISignalRPublisher signalRPublisher,
    ICountriesTranslationService countriesService,
    ILocalizationTranslationService localizationService,
    IStringLocalizer<BackendTranslationService> t,
    IDocumentsTranslationService documentsService,
    ILogger<BackendTranslationService> logger) : IBackendTranslationService
{
    private readonly AutomaticTranslationSettings _settings = configuration.GetSection("AutomaticTranslationSettings").Get<AutomaticTranslationSettings>() ?? new AutomaticTranslationSettings();
    private readonly ILibreTranslateService _translateService = translateService;
    private readonly ISignalRPublisher _signalRPublisher = signalRPublisher;
    private readonly IStringLocalizer<BackendTranslationService> _t = t;
    private readonly ICountriesTranslationService _countriesService = countriesService;
    private readonly ILocalizationTranslationService _localizationService = localizationService;
    private readonly IDocumentsTranslationService _documentsService = documentsService;
    private readonly ILogger<BackendTranslationService> _logger = logger;

    private readonly SemaphoreSlim _pipelineRunLock = new(1, 1);

    /// <summary>
    /// Executes a full automatic translation pipeline run.
    /// Uses a semaphore to prevent overlapping runs.
    /// </summary>
    public async Task RunAsync()
    {
        if (!await _pipelineRunLock.WaitAsync(0))
        {
            _logger.LogWarning("Automatic translation pipeline is already running. This cycle will be skipped.");
            return;
        }

        Guid runId = Guid.NewGuid();
        var storingReport = new StoringReport { RunStartedUtc = DateTime.UtcNow };

        try
        {
            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.StageStarted,
                ProcessStage.CheckServers,
                T("Automatic translation pipeline started."));

            _logger.LogInformation("Translation pipeline {RunId} started.", runId);

            // Stage 1: Validate servers and build context
            var checkContext = await RunCheckServersStageAsync(runId);

            // Stage 2: Translate country names
            await _countriesService.RunAsync(checkContext.TargetLanguages, storingReport, runId);

            // Stage 3: Synchronize JSON localization dictionaries
            await _localizationService.RunAsync(checkContext.TargetLanguages, storingReport, runId);

            // Stage 4: Synchronize Markdown documents
            await _documentsService.RunAsync(checkContext.TargetLanguages, storingReport, runId);

            // Pipeline completion
            storingReport.RunCompletedUtc = DateTime.UtcNow;

            await _signalRPublisher.PublishStageAsync(
                runId,
                ProcessStage.StoringResults,
                storingReport,
                LocalizationMessageType.StageCompleted,
                T("Localization artifacts stored."));

            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.PipelineCompleted,
                ProcessStage.StoringResults,
                T("Automatic translation pipeline completed successfully."),
                storingReport);

            _logger.LogInformation("Translation pipeline {RunId} completed successfully.", runId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Automatic translation pipeline {RunId} failed.", runId);
            string failureMessage = T("Automatic translation pipeline failed: {message}", new { message = ex.Message });
            storingReport.Errors.Add(CreateError("pipeline", ErrorCode.InternalError, failureMessage));
            storingReport.RunCompletedUtc = DateTime.UtcNow;

            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.PipelineFailed,
                ProcessStage.StoringResults,
                failureMessage,
                storingReport,
                isError: true);
        }
        finally
        {
            _pipelineRunLock.Release();
        }
    }

    private async Task<CheckContext> RunCheckServersStageAsync(Guid runId)
    {
        var report = new CheckingReport
        {
            AppsettingsLoaded = _settings.AppsettingsLoaded,
            DefaultLanguage = _settings.DefaultLanguage ?? "en"
        };

        try
        {
            // Server latency check
            var latencyResponse = _translateService.ServerLatency();
            report.ServerLatencyMs = latencyResponse.Data;
            report.TranslationServerReady = latencyResponse.Success;

            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.Progress,
                ProcessStage.CheckServers,
                T("Server latency: {latency}ms. Ready: {ready}.", new { latency = latencyResponse.Data, ready = latencyResponse.Success }));

            // Available languages
            var languagesResponse = await _translateService.GetAvailableLanguagesAsync();
            if (!languagesResponse.Success || languagesResponse.Data is null || languagesResponse.Data.Length == 0)
            {
                throw new InvalidOperationException(languagesResponse.Message);
            }

            report.AvailableLanguages = languagesResponse.Data;

            await _signalRPublisher.PublishMessageAsync(
                runId,
                LocalizationMessageType.Progress,
                ProcessStage.CheckServers,
                T("Discovered {count} available languages: {languages}.", new
                {
                    count = report.AvailableLanguages.Length,
                    languages = string.Join(", ", report.AvailableLanguages)
                }));

            if (!_settings.AppsettingsLoaded)
            {
                throw new InvalidOperationException(T("AutomaticTranslationSettings were not loaded."));
            }

            string defaultLanguage = _settings.DefaultLanguage ?? "en";

            if (!languagesResponse.Data.Contains(defaultLanguage, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(T("Default language '{defaultLanguage}' is not supported by the translation server.", new { defaultLanguage }));
            }

            // Build target language list
            var ignoredLanguages = _settings.IgnoredLanguages ?? [];
            List<string> targetLanguages = [.. languagesResponse.Data
                .Where(language => !language.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))
                .Where(language => !ignoredLanguages.Contains(language, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)];

            await _signalRPublisher.PublishStageAsync(
                runId,
                ProcessStage.CheckServers,
                report,
                LocalizationMessageType.StageCompleted,
                T("Translation server and configuration validated."));

            return new CheckContext(report, targetLanguages, defaultLanguage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckServers stage failed.");
            await _signalRPublisher.PublishStageAsync(
                runId,
                ProcessStage.CheckServers,
                report,
                LocalizationMessageType.StageFailed,
                ex.Message,
                isError: true);
            throw;
        }
    }

    private string T(string text) => _t[text].Value;

    private string T(string text, object values) => _t[text, values].Value;

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

    private sealed record CheckContext(CheckingReport Report, List<string> TargetLanguages, string DefaultLanguage);
}
