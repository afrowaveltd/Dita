namespace Dita.Shared.Localization.Models;
/// <summary>
/// Represents the detection result of a language detection operation, including the confidence level and the detected language code.
/// </summary>
public class Detections
{
   /// <summary>
   /// Gets or sets the confidence level of the operation or result.
   /// </summary>
   public int Confidence { get; set; } = 0;

   /// <summary>
   /// Gets or sets the language code representing the user's preferred language.
   /// </summary>
   public string Language { get; set; } = string.Empty;
}
