namespace Dita.Shared.Localization.Models;

/// <summary>
/// Summarises persisted outputs produced by an automatic translation run.
/// </summary>
public class StoringReport
{
    /// <summary>
    /// UTC timestamp when the pipeline run started.
    /// </summary>
    public DateTime RunStartedUtc { get; set; }

    /// <summary>
    /// UTC timestamp when the pipeline run completed.
    /// </summary>
    public DateTime? RunCompletedUtc { get; set; }

    /// <summary>
    /// Count of locale JSON files written during the run.
    /// </summary>
    public int SavedDictionaryFiles { get; set; }

    /// <summary>
    /// Count of markdown files written during the run.
    /// </summary>
    public int SavedMarkdownFiles { get; set; }

    /// <summary>
    /// Count of source hash files written during the run.
    /// </summary>
    public int SavedHashFiles { get; set; }

    /// <summary>
    /// Count of times hash persistence had to fall back to the temporary folder.
    /// </summary>
    public int TempFallbackWrites { get; set; }

    /// <summary>
    /// Errors collected while storing or finalising outputs.
    /// </summary>
    public List<TranslationError> Errors { get; set; } = [];
}
