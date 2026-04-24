using Dita.Shared.Localization.Enums;

namespace Dita.Shared.Localization.Models;
/// <summary>
///   Represents a report for a processing stage, containing stage identification, associated data, and timing
/// information.
/// </summary>
/// <typeparam name="T">The type of stage-specific data contained in the report.</typeparam>
public class StageReport<T> where T : class
{
   /// <summary>
   /// The current stage of the process being reported. Defaults to <see cref="ProcessStage.TranslateLanguages"/>.
   /// </summary>
   public ProcessStage ReportedStage { get; set; } = ProcessStage.TranslateLanguages;
   /// <summary>
   /// The data associated with the current stage. This is of type <typeparamref name="T"/>.
   /// </summary>
   public T? StageData { get; set; }
   /// <summary>
   /// The start time of the current stage.
   /// </summary>
   public DateTime? StageStartTime { get; set; }
   /// <summary>
   /// The end time of the current stage.
   /// </summary>
   public DateTime? StageEndTime { get; set; }
   /// <summary>
   /// The duration of the current stage, calculated as the difference between <see cref="StageEndTime"/> and <see cref="StageStartTime"/>.
   /// </summary>          
   public TimeSpan? StageDuration => StageEndTime - StageStartTime;

}

