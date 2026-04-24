namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Network-related error codes (range 1000-1999).
/// </summary>
public enum NetworkError
{
   /// <summary>
   /// Bad gateway response received from upstream server.
   /// </summary>
   BadGateway = 1001,

   /// <summary>
   /// Certificate validation failed during SSL/TLS handshake.
   /// </summary>
   CertificateValidationFailed = 1002,

   /// <summary>
   /// Connection was actively refused by the remote host.
   /// </summary>
   ConnectionRefused = 1003,

   /// <summary>
   /// Connection was reset by the remote host.
   /// </summary>
   ConnectionReset = 1004,

   /// <summary>
   /// Connection attempt timed out.
   /// </summary>
   ConnectionTimeout = 1005,

   /// <summary>
   /// DNS resolution failed for the specified hostname.
   /// </summary>
   DnsResolutionFailed = 1006,

   /// <summary>
   /// Gateway timeout occurred while waiting for upstream server.
   /// </summary>
   GatewayTimeout = 1007,

   /// <summary>
   /// Network host is unreachable.
   /// </summary>
   HostUnreachable = 1008,

   /// <summary>
   /// HTTP protocol error occurred.
   /// </summary>
   HttpProtocolError = 1009,

   /// <summary>
   /// Invalid or malformed URL.
   /// </summary>
   InvalidUrl = 1010,

   /// <summary>
   /// Network interface is not available.
   /// </summary>
   NetworkInterfaceUnavailable = 1011,

   /// <summary>
   /// Network is unreachable.
   /// </summary>
   NetworkUnreachable = 1012,

   /// <summary>
   /// No error occurred (success).
   /// </summary>
   None = 1000,

   /// <summary>
   /// Proxy authentication failed.
   /// </summary>
   ProxyAuthenticationFailed = 1013,

   /// <summary>
   /// Proxy connection error.
   /// </summary>
   ProxyConnectionError = 1014,

   /// <summary>
   /// Request was cancelled by the caller.
   /// </summary>
   RequestCancelled = 1015,

   /// <summary>
   /// Request entity too large (payload exceeds limit).
   /// </summary>
   RequestEntityTooLarge = 1016,

   /// <summary>
   /// Request timeout occurred.
   /// </summary>
   RequestTimeout = 1017,

   /// <summary>
   /// Service is temporarily unavailable.
   /// </summary>
   ServiceUnavailable = 1018,

   /// <summary>
   /// SSL/TLS handshake failed.
   /// </summary>
   SslHandshakeFailed = 1019,

   /// <summary>
   /// Too many redirects encountered.
   /// </summary>
   TooManyRedirects = 1020,

   /// <summary>
   /// Unknown network error occurred.
   /// </summary>
   UnknownNetworkError = 1021
}
