using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Factory class for creating HttpClient instances configured to communicate with the LibreTranslate service based on
/// the provided settings.
/// </summary>
/// <param name="settings"></param>
public class LibreTranslateHttpClientFactory(AutomaticTranslationSettings settings) : ILibreTranslateHttpClientFactory
{
   /// <summary>
   /// Creates and configures a new HttpClient instance with the base address set to the LibreTranslate service URL
   /// specified in the settings. This client can be used to send requests to the translation service for performing
   /// automatic translations.
   /// </summary>
   /// <returns></returns>
   public HttpClient LibreClient
   {
      get
      {
         return new HttpClient() { BaseAddress = new Uri(settings.Address) };
      }
   }
}