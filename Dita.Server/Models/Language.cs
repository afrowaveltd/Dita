namespace Dita.Server.Models;

/// <summary>
/// Represents a language with its associated metadata, including code, name, native name, and text direction.
/// </summary>
/// <remarks>
/// This class provides information about a language, such as its ISO code, display name, native
/// name, and whether the language is written in a right-to-left (RTL) script.
/// </remarks>
public class Language
{
   /// <summary>
   /// Gets or sets the language ISO code associated with this instance.
   /// </summary>
   public string Code { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the English name of the language.
   /// </summary>
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the native representation of the language name (e.g., "Čeština" for Czech).
   /// </summary>
   public string Native { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets a value indicating whether the layout should be rendered in a right-to-left
   /// (RTL) direction.
   /// </summary>
   public bool Rtl { get; set; } = false;
}
