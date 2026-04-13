namespace Dita.Shared.Localization.Models;

/// <summary>
/// Represents the result of a file translation operation performed by LibreTranslate.
/// </summary>
/// <remarks>
/// This class contains the URL of the translated file, which can be used to access the file after
/// the translation process.
/// </remarks>
public class TranslateFileResult
{
   /// <summary>
   /// Gets or sets the URL of the translated file.
   /// </summary>
   public string TranslatedFileUrl { get; set; } = string.Empty;
}