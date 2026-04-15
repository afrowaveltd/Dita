using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.Services;

public class LanguageService(ILogger<LanguageService> logger, IStringLocalizer<LanguageService> t)
{
   private readonly ILogger<LanguageService> _logger = logger;
   private readonly IStringLocalizer<LanguageService> _t = t;


}
