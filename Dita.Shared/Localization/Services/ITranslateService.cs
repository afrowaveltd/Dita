using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Resolves dynamic client text through dictionaries first and the translation server second.
/// </summary>
/// <remarks>
/// Unlike <see cref="ILocalizeService"/>, this service never writes to locale dictionaries. It may use dictionaries
/// as a fast cache, but missing text is translated on demand and returned without persistence.
/// </remarks>
public interface ITranslateService
{
   /// <summary>
   /// Translates dynamic text without creating or modifying locale dictionary entries.
   /// </summary>
   /// <param name="request">The translation request supplied by the API client.</param>
   /// <param name="cancellationToken">Token used to cancel request processing.</param>
   /// <returns>A response describing the translated text and whether the translation server was used.</returns>
   Task<Response<TextTranslationResponse>> TranslateAsync(
      TextTranslationRequest request,
      CancellationToken cancellationToken = default);
}
