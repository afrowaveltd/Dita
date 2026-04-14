using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.Services;
/// <summary>
/// Represents a factory for creating instances of JsonStringLocalizer, which provides localization support using JSON files and integrates with LibreTranslate for translation services.
/// </summary>
/// <param name="cache"></param>
/// <param name="libreTranslate"></param>
/// <param name="logger"></param>
public class JsonStringLocalizerFactory(IDistributedCache cache, ILibreTranslateService libreTranslate, ILogger<JsonStringLocalizer> logger) : IStringLocalizerFactory
{
   private readonly IDistributedCache _cache = cache;
   private readonly ILibreTranslateService _libreTranslate = libreTranslate;
   private readonly ILogger<JsonStringLocalizer> _logger = logger;
   /// <summary>
   /// Creates an instance of JsonStringLocalizer for the specified resource source type.
   /// </summary>
   /// <param name="resourceSource"></param>
   /// <returns></returns>
   public IStringLocalizer Create(Type resourceSource)
   {
      return new JsonStringLocalizer(_cache, _libreTranslate, _logger);
   }
   /// <summary>
   /// Creates an instance of JsonStringLocalizer for the specified base name and location.
   /// </summary>
   /// <param name="baseName"></param>
   /// <param name="location"></param>
   /// <returns></returns>
   public IStringLocalizer Create(string baseName, string location)
   {
      return new JsonStringLocalizer(_cache, _libreTranslate, _logger);
   }
}