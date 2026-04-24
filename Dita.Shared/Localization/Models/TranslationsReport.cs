namespace Dita.Shared.Localization.Models;
/// <summary>
/// Represents a report of the translation process, including counts of default dictionary entries, 
/// entries to translate, translated entries, and any errors encountered.
/// </summary>
public class TranslationsReport
{
   /// <summary>
   /// Indicates whether the default dictionary exists. This is important for determining if there are any entries to translate.
   /// </summary>
   public bool DefaultDictionaryExists { get; set; } = false;
   /// <summary>
   /// The count of entries in the default dictionary. This helps to understand the scope of the translation work needed.
   /// </summary>
   public int DefaultDictionaryCount { get; set; } = 0;
   /// <summary>
   ///   The count of entries that need to be translated. This is calculated based on the default dictionary 
   /// and the existing translations, and it indicates the amount of work remaining in the translation process.
   /// </summary>
   public int ToTranslateCount { get; set; } = 0;

   /// <summary>
   /// The count of entries added during this run.
   /// </summary>
   public int AddedCount { get; set; } = 0;

   /// <summary>
   /// The count of entries removed during this run.
   /// </summary>
   public int RemovedCount { get; set; } = 0;

   /// <summary>
   /// The count of items skipped because an existing manual translation took precedence.
   /// </summary>
   public int SkippedCount { get; set; } = 0;

   /// <summary>
   /// The count of entries that have been successfully translated. This helps to track progress in the translation 
   /// process and can be used to calculate completion percentages. 
   /// </summary>
   public int TranslatedCount { get; set; } = 0;
   /// <summary>
   /// The count of errors encountered during the translation process. 
   /// This is important for identifying issues that may need to be addressed, such as missing translations, formatting problems, or other errors that could affect the quality of the translated content.
   /// </summary>
   public int ErrorsCount { get; set; } = 0;
   /// <summary>
   /// A list of translation errors encountered during the translation process. 
   /// This provides detailed information about each error, which can be used for troubleshooting 
   /// and improving the translation workflow. Each error in the list may include details 
   /// such as the type of error, the affected entry, and any relevant messages or codes 
   /// to help identify and resolve the issue.
   /// </summary>
   public List<TranslationError>? Errors { get; set; } = [];
}

