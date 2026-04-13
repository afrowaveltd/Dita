namespace Dita.Server.Models.Settings;

/// <summary>
/// Settings related to localization and language preferences for the application. This class includes properties for
/// specifying the default language and whether to use automatic translation features, which can enhance the user
/// experience by providing content in the user's preferred language. The automatic translation feature may require
/// integration with a translation service such as LibreTranslate, which should be configured in the server settings for
/// it to function properly.
/// </summary>
public class LocalizationSettings
{
   /// <summary>
   /// Gets or sets the default language code used for localization or language-specific operations.
   /// </summary>
   /// <remarks>
   /// The value should be a valid language code, such as "en" for English or "cs" for Czech. This property determines
   /// the language context for features that support localization.
   /// </remarks>
   public string DefaultLanguage { get; set; } = "en";

   /// <summary>
   /// Gets or sets a value indicating whether automatic translation is enabled for messages and related features.
   /// </summary>
   /// <remarks>
   /// Automatic translation requires LibreTranslate to be set up and configured in the server settings. The property
   /// value should be set to "true" to enable automatic translation, or "false" to disable it.
   /// </remarks>
   public string UseAutomaticTranslation { get; set; } = "false";  // for messages, smart, etc.. Requires LibreTranslate to be set up and configured in the server settings
}