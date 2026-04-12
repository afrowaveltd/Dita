using Microsoft.AspNetCore.Http;

namespace Dita.Shared.Localization.Models;

/// <summary>
/// Represents a request to translate a file using the LibreTranslate API.
/// </summary>
/// <remarks>
/// This class encapsulates the necessary parameters for submitting a file translation request, including the file to be
/// translated, the source and target languages, and an optional API key.
/// </remarks>
public class TranslateFileRequest
{
   /// <summary>
   /// Gets or sets the API key used for authenticating requests to external services.
   /// </summary>
   public string Api_key { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the uploaded file associated with the current request.
   /// </summary>
   public IFormFile File { get; set; } = null!;

   /// <summary>
   /// Gets or sets the source language code for the translation.
   /// </summary>
   public string Source { get; set; } = "auto";

   /// <summary>
   /// Gets or sets the target language code for the translation.
   /// </summary>
   public string Target { get; set; } = "en";
}