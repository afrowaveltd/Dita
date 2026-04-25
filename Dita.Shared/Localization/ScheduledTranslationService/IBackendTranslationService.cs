namespace Dita.Shared.Localization.ScheduledTranslationService;

/// <summary>
/// Defines a background translation service that executes scheduled backend translation work.
/// </summary>
public interface IBackendTranslationService
{
   /// <summary>
   /// Runs the scheduled backend translation process.
   /// </summary>
   /// <returns>A task that represents the asynchronous execution.</returns>
   Task RunAsync();
}
