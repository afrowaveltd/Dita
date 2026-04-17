using Afrowave.SharedTools.Models.Localization;
using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

public interface ILanguageService
{
   List<Language> Languages { get; }

   Task<Dictionary<string, bool>> CreateMissingLanguageFilesAsync(List<string> languages);
   Task<Response<List<SingleTranslation>>> GetAllDictionariesAsync();
   Task<Response<Dictionary<string, string>>> GetDictionaryAsync(string code);
   Response<Language>? GetLanguageByCode(string code);
   List<string> GetLanguageNames();
   Task<Response<Dictionary<string, string>>> GetLastStored();
   Response<List<Language>> GetRequiredLanguagesAsync();
   Response<List<Language>> GetSelectedLanguagesInfo(List<string> languages);
   bool IsRtl(string code);
   Task<Response<bool>> SaveDictionaryAsync(SingleTranslation data);
   Task<Response<bool>> SaveOldTranslationAsync(Dictionary<string, string> data);
   Task<Response<Dictionary<string, bool>>> SaveTranslationsAsync(List<SingleTranslation> tree);
}