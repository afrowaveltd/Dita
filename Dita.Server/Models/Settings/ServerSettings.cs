using Dita.Server.Models.Enums;

namespace Dita.Server.Models.Settings;

/// <summary>
/// Represents the configuration settings for a server instance, including its unique identifier, display name,
/// description, network configuration, declared capabilities, and cluster membership flag.
/// </summary>
public class ServerSettings
{
   /// <summary>
   /// Gets or sets the unique identifier for the server instance.
   /// </summary>
   /// <remarks>
   /// This value is generated as a new GUID string by default when the property is initialized. It can be set to a
   /// custom value if needed to identify the server across different environments or deployments.
   /// </remarks>
   public string ServerId { get; set; } = Guid.NewGuid().ToString();

   /// <summary>
   /// Gets or sets the name of the server to connect to.
   /// </summary>
   public string ServerName { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the description of the server.
   /// </summary>
   public string ServerDescription { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the network settings for the server.
   /// </summary>
   public NetworkSettings NetworkSettings { get; set; } = new();

   /// <summary>
   /// Gets or sets the capabilities of the server.
   /// </summary>
   public ServerCapabilities Capabilities { get; set; } = ServerCapabilities.None;

   /// <summary>
   /// Gets or sets a value indicating whether the server is part of a cluster.
   /// </summary>
   public bool IsClustered { get; set; } = false;
}