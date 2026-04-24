namespace Dita.Shared.Localization.Enums;
/// <summary>
/// Represents the current stage in the translation processing workflow.
/// </summary>
public enum ProcessStage
{
   /// <summary>
   /// Represents an idle or inactive state.
   /// </summary>
   Iddle = 0,
   /// <summary>
   /// Indicates that servers should be checked.
   /// </summary>
   CheckServers = 1,
   /// <summary>
   /// Translates between countries.
   /// </summary>
   TranslateCountries = 2,
   /// <summary>
   /// Translates JSON Localization files.
   /// </summary>
   TranslateJsonFiles = 3,
   /// <summary>
   /// Translates Markdown files.
   /// </summary>
   TranslateMarkdownFiles = 4,
   /// <summary>
   /// Stores the results of the translation process.
   /// </summary>
   StoringResults = 5
}
