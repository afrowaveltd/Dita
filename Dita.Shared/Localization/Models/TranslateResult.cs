namespace Dita.Shared.Localization.Models;

/// <summary>
/// Represents the result of a translation operation performed by the LibreTranslation service.
/// </summary>
/// <remarks>
/// This class encapsulates the translated text returned by the service. It is typically used to
/// store and access the output of a translation request.
/// </remarks>
public class TranslateResult
{
   /// <summary>
   /// Gets or sets the list of alternative translation.
   /// </summary>
   public List<string> Alternatives { get; set; } = [];

   /// <summary>
   /// Gets or sets the detected language of the analyzed text.
   /// </summary>
   public Detections DetectedLanguage { get; set; } = new();

   /// <summary>
   /// Gets or sets the translated text resulting from a translation operation.
   /// </summary>
   public string TranslatedText { get; set; } = string.Empty;
}
