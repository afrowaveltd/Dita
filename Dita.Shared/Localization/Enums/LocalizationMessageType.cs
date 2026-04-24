namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Describes the kind of real-time message emitted by the automatic localization pipeline.
/// </summary>
public enum LocalizationMessageType
{
    /// <summary>
    /// A pipeline stage has started.
    /// </summary>
    StageStarted = 0,

    /// <summary>
    /// A pipeline stage has completed successfully.
    /// </summary>
    StageCompleted = 1,

    /// <summary>
    /// A pipeline stage failed.
    /// </summary>
    StageFailed = 2,

    /// <summary>
    /// The whole pipeline completed successfully.
    /// </summary>
    PipelineCompleted = 3,

    /// <summary>
    /// The whole pipeline failed.
    /// </summary>
    PipelineFailed = 4,

    /// <summary>
    /// An informational progress message.
    /// </summary>
    Progress = 5,

    /// <summary>
    /// A non-fatal warning message.
    /// </summary>
    Warning = 6
}