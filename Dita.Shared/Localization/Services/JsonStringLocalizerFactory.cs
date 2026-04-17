using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Factory for creating <see cref="JsonStringLocalizer"/> instances that provide JSON-file-based localization
/// with an optional LibreTranslate fallback for missing translations.
/// </summary>
/// <param name="cache">The distributed cache used to store resolved translation strings.</param>
/// <param name="libreTranslate">The LibreTranslate service used as a fallback when a key is not found in a locale file.</param>
/// <param name="logger">The logger used for diagnostic output from created localizer instances.</param>
public class JsonStringLocalizerFactory(IDistributedCache cache, ILibreTranslateService libreTranslate, ILogger<JsonStringLocalizer> logger) : IStringLocalizerFactory
{
   private readonly IDistributedCache _cache = cache;
   private readonly ILibreTranslateService _libreTranslate = libreTranslate;
   private readonly ILogger<JsonStringLocalizer> _logger = logger;

   /// <summary>
   /// Creates a <see cref="JsonStringLocalizer"/> for the specified resource source type.
   /// </summary>
   /// <param name="resourceSource">The type whose assembly and namespace are used to locate locale files.</param>
   /// <returns>A new <see cref="IStringLocalizer"/> instance backed by JSON locale files.</returns>
   public IStringLocalizer Create(Type resourceSource)
   {
      return new JsonStringLocalizer(_cache, _libreTranslate, _logger);
   }

   /// <summary>
   /// Creates a <see cref="JsonStringLocalizer"/> for the specified base name and location.
   /// </summary>
   /// <param name="baseName">The base name of the resource (not used in the JSON-file strategy).</param>
   /// <param name="location">The location or assembly name of the resource (not used in the JSON-file strategy).</param>
   /// <returns>A new <see cref="IStringLocalizer"/> instance backed by JSON locale files.</returns>
   public IStringLocalizer Create(string baseName, string location)
   {
      return new JsonStringLocalizer(_cache, _libreTranslate, _logger);
   }
}