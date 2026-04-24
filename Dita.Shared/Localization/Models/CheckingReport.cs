namespace Dita.Shared.Localization.Models;

public class CheckingReport
{
   public bool TranslationServerReady { get; set; } = false;
   public string DefaultLanguage { get; set; } = string.Empty;
   public string[] AvailableLanguages { get; set; } = [];
}
