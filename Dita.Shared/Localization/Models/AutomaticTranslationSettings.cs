namespace Dita.Shared.Localization.Models;

/// <summary>
/// Settings for the automatic translation service.
/// </summary>
public class AutomaticTranslationSettings
{
   /// <summary>
   /// Gets or sets the service address.
   /// </summary>
   public string Address { get; set; } = "http://localhost:5000";

   /// <summary>
   /// Gets or sets a value indicating whether the service requires an API key.
   /// </summary>
   public bool NeedsKey { get; set; } = false;

   /// <summary>
   /// Gets or sets the API key used to access the service.
   /// </summary>
   public string Key { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the default source language.
   /// </summary>
   public string DefaultLanguage { get; set; } = "en";

   /// <summary>
   /// Gets or sets the languages that should be ignored during automatic translation.
   /// </summary>
   public List<string> IgnoredLanguages { get; set; } = [];

   /// <summary>
   /// Gets or sets a value indicating whether automatic translation runs automatically.
   /// </summary>
   public bool AutomaticRun { get; set; } = false;

   /// <summary>
   /// Gets or sets the delay before automatic translation starts.
   /// </summary>
   public TimeSpan WaitingTime { get; set; } = TimeSpan.Zero;

   /// <summary>
   /// Gets or sets the checking period in minutes.
   /// </summary>
   public int CheckingPeriod { get; set; } = 30;

   /// <summary>
   /// Gets or sets the endpoint URL for the translation service.
   /// </summary>
   public string TranslateEndpoint { get; set; } = "/translate";

   /// <summary>
   /// Gets or sets the endpoint URL used for file translation requests.
   /// </summary>
   public string TranslateFileEndpoint { get; set; } = "/translate_file";

   /// <summary>
   /// Gets or sets the endpoint URL used to retrieve supported languages.
   /// </summary>
	public string LanguagesEndpoint { get; set; } = "/languages";

   /// <summary>
   /// Gets or sets the endpoint URL used to detect the language from provided text.
   /// </summary>
	public string DetectLanguageEndpoint { get; set; } = "/detect";

   /// <summary>
   /// Gets or sets a value indicating whether application settings have been loaded.
   /// </summary>
   public bool AppsettingsLoaded { get; set; } = false;
}