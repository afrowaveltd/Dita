using Dita.Shared.Localization.Enums;

namespace Dita.Shared.Localization.Models;

/// <summary>
/// Represents a structured translation pipeline error.
/// </summary>
public class TranslationError
{
   /// <summary>
   /// Identifies the source of the error, for example a language code, file path, or pipeline step.
   /// </summary>
   public string Source { get; set; } = string.Empty;

   /// <summary>
   /// Unified machine-readable error code.
   /// </summary>
   public ErrorCode Code { get; set; } = ErrorCode.UnknownError;

   /// <summary>
   /// Human-readable error description.
   /// </summary>
   public string ErrorMessage { get; set; } = string.Empty;
}
