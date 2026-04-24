namespace Dita.Shared.Localization.Models;

/// <summary>
/// Report produced by the environment and translation server validation stage.
/// </summary>
public class CheckingReport
{
   /// <summary>
   /// Indicates whether automatic translation configuration was loaded successfully.
   /// </summary>
   public bool AppsettingsLoaded { get; set; } = false;

   /// <summary>
   /// Indicates whether the translation server is reachable and responsive.
   /// </summary>
   public bool TranslationServerReady { get; set; } = false;

   /// <summary>
   /// The configured default language used as the source language for automatic translations.
   /// </summary>
   public string DefaultLanguage { get; set; } = string.Empty;

   /// <summary>
   /// The language codes reported by the translation server.
   /// </summary>
   public string[] AvailableLanguages { get; set; } = [];

   /// <summary>
   /// Measured translation server latency in milliseconds.
   /// </summary>
   public int ServerLatencyMs { get; set; }
}
