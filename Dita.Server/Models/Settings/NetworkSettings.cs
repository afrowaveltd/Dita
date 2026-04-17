namespace Dita.Server.Models.Settings;

/// <summary>
/// Represents the network configuration settings for a server instance, including the IP address and ports used for
/// HTTP and HTTPS communication.
/// </summary>
public class NetworkSettings
{
   /// <summary>
   /// Gets or sets the IP address used for network communication.
   /// </summary>
   public string IpAddress { get; set; } = "127.0.0.1";

   /// <summary>
   /// Gets or sets the network port number used for connections.
   /// </summary>
   /// <remarks>
   /// The default value is 5678. Ensure that the specified port is available and not blocked by a firewall. Valid port
   /// numbers range from 0 to 65535, but ports below 1024 may require elevated permissions.
   /// </remarks>
   public int Port { get; set; } = 5678;

   /// <summary>
   /// Gets or sets the network port number used for secure connections (HTTPS).
   /// </summary>
   /// <remarks>
   /// The default value is 5679. Ensure that the specified port is available and not blocked by a firewall. Valid port
   /// numbers range from 0 to 65535, but ports below 1024 may require elevated permissions. This port should be used
   /// for secure connections (HTTPS) only.
   /// </remarks>
   public int SecurePort { get; set; } = 5679;

   /// <summary>
   /// Gets the full HTTP URL constructed from the current IP address and port.
   /// </summary>
   /// <remarks>
   /// The returned URL uses the HTTP scheme and combines the values of the IpAddress and Port properties. Ensure that
   /// both properties are set to valid values before accessing this property.
   /// </remarks>
   public string Url => $"http://{IpAddress}:{Port}";

   /// <summary>
   /// Gets the full HTTPS URL constructed from the current IP address and secure port.
   /// </summary>
   /// <remarks>
   /// The returned URL uses the HTTPS scheme and combines the values of the IpAddress and SecurePort properties. Ensure
   /// that both properties are set to valid values before accessing this property.
   /// </remarks>
   public string SecureUrl => $"https://{IpAddress}:{SecurePort}";
}