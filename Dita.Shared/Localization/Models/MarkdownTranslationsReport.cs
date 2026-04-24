namespace Dita.Shared.Localization.Models;

/// <summary>
/// Report for a Markdown translation synchronization stage.
/// </summary>
public class MarkdownTranslationsReport
{
    /// <summary>
    /// Count of source Markdown files discovered.
    /// </summary>
    public int SourceFilesDetected { get; set; }

    /// <summary>
    /// Count of source Markdown files that required translation updates.
    /// </summary>
    public int SourceFilesChanged { get; set; }

    /// <summary>
    /// Count of target Markdown files written.
    /// </summary>
    public int SavedFiles { get; set; }

    /// <summary>
    /// Count of source Markdown files skipped because hashes and targets were already up to date.
    /// </summary>
    public int SkippedFiles { get; set; }

    /// <summary>
    /// Count of hash writes that fell back to temporary storage.
    /// </summary>
    public int TempFallbackWrites { get; set; }

    /// <summary>
    /// Errors collected during the stage.
    /// </summary>
    public List<TranslationError> Errors { get; set; } = [];
}