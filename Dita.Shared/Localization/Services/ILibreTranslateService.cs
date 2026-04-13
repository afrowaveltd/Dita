using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

public interface ILibreTranslateService
{
   Task<Response<Detections>> DetectLanguageAsync(string text);
   Task<Response<string[]>> GetAvailableLanguagesAsync();
   Task<Response<TranslateFileResult>> TranslateFileAsync(Stream fileStream, string targetLanguage, string fileName);
   Task<Response<TranslateFileResult>> TranslateFileAsync(Stream fileStream, string sourceLanguage, string targetLanguage, string fileName);
   Task<Response<TranslateResult>> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage);
}