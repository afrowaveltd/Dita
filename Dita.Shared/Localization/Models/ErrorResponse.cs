namespace Dita.Shared.Localization.Models;

/// <summary>
/// Represents error message with its text
/// </summary>
/// <param name="message">Text of the error message.</param>
public class ErrorResponse(string? message)
{
   /// <summary>
   /// Error message
   /// </summary>
   public string? Error { get; set; } = message;
}
