namespace Dita.Shared.Localization.Services;

/// <summary>
/// Provides a factory interface for obtaining an HTTP client configured to communicate with a LibreTranslate service.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for supplying an appropriately configured instance of
/// <see cref="HttpClient"/> for use with LibreTranslate APIs. The returned client may include custom headers,
/// authentication, or base address settings required for successful communication with the translation service.
/// </remarks>
public interface ILibreTranslateHttpClientFactory
{
   /// <summary>
   /// Gets the HTTP client used to send requests to the Libre service.
   /// </summary>
   /// <remarks>
   /// The returned client is configured for communication with the Libre API. Callers should not dispose of this
   /// instance.
   /// </remarks>
   HttpClient LibreClient { get; }
}