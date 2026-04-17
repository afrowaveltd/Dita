using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Factory that creates <see cref="HttpClient"/> instances pre-configured to communicate with the LibreTranslate service.
/// </summary>
/// <param name="settings">
/// The automatic translation settings that supply the LibreTranslate service address and related configuration.
/// </param>
public class LibreTranslateHttpClientFactory(AutomaticTranslationSettings settings) : ILibreTranslateHttpClientFactory
{
   /// <summary>
   /// Gets a new <see cref="HttpClient"/> instance with <see cref="HttpClient.BaseAddress"/> set to the LibreTranslate
   /// service URL from <see cref="AutomaticTranslationSettings.Address"/>.
   /// </summary>
   /// <remarks>
   /// A new client is created on every access. Callers should not dispose the instance while it is still in use by the
   /// translation service.
   /// </remarks>
   public HttpClient LibreClient
   {
      get
      {
         return new HttpClient() { BaseAddress = new Uri(settings.Address) };
      }
   }
}