using Afrowave.SharedTools.Models.Results;
using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Resolves client phrases through the application localization dictionaries.
/// </summary>
/// <remarks>
/// The localize workflow is writable only for the configured default dictionary. If a phrase is not found in
/// the requested target dictionary or the default dictionary, it is added to the default dictionary as
/// <c>key = value = phrase</c> so the scheduled translation pipeline can translate it later.
/// </remarks>
public interface ILocalizeService
{
   /// <summary>
   /// Resolves a phrase through locale dictionaries and adds missing phrases to the default dictionary.
   /// </summary>
   /// <param name="request">The localization request supplied by the API client.</param>
   /// <param name="cancellationToken">Token used to cancel request processing.</param>
   /// <returns>A response describing the localized text and where it was resolved from.</returns>
   Task<Response<TextLocalizationResponse>> LocalizeAsync(
      TextLocalizationRequest request,
      CancellationToken cancellationToken = default);
}
