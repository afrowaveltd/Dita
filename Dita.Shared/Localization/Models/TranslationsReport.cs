namespace Dita.Shared.Localization.Models;

public class TranslationsReport
{
   public bool DefaultDictionaryExists { get; set; } = false;
   public int DefaultDictionaryCount { get; set; } = 0;
   public int ToTranslateCount { get; set; } = 0;
   public int TranslatedCount { get; set; } = 0;
   public int ErrorsCount { get; set; } = 0;
   public List<TranslationError>? Errors { get; set; }
}

