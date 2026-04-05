namespace Dita.Server.Models.Settings;

public class LocalizationSettings
{
   public string DefaultLanguage { get; set; } = "en";
   public string UseAutomaticTranslation { get; set; } = "false";  // for messages, smart, etc.. Requires LibreTranslate to be set up and configured in the server settings
}