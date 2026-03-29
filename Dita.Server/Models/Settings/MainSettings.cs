using Dita.Server.Models.Enums;

namespace Dita.Server.Models.Settings;
/// <summary>
/// Represents the main settings for the Dita server, including server identification, capabilities, and cluster membership.
/// </summary>

public class MainSettings
{
   /// <summary>
   /// Gets or sets the unique identifier for the server. This is generated as a new GUID by default to ensure uniqueness across different instances of the server.
   /// </summary>
   public string ServerId { get; set; } = Guid.NewGuid().ToString();
   /// <summary>
   /// Gets or sets the name of the server. This is a user-friendly name that can be used for display purposes. The default value is "Dita 01", but it can be customized to reflect the specific instance or role of the server within a cluster or network.   
   /// </summary>
   public string ServerName { get; set; } = "Dita 01";
   /// <summary>
   /// Gets or sets the IP address and port on which the server is listening for incoming connections. The default value is "127.0.0.1:5678". This should be configured to match the actual network settings of the server, especially if it is intended to be accessed from other machines or as part of a cluster.
   /// </summary>
   public string ServerIP { get; set; } = "127.0.0.1:5678";
   /// <summary>
   /// Gets or sets the capabilities of the server. This is represented as a bitwise combination of the ServerCapabilities enum values.
   /// </summary>
   public ServerCapabilities Capabilities { get; set; } = ServerCapabilities.None;
   /// <summary>
   /// Gets or sets a value indicating whether the server is a member of a cluster.
   /// </summary>
   public bool MemberOfCluster { get; set; } = false;
   /// <summary>
   /// Gets or sets a value indicating whether the server should automatically synchronize with other cluster members.
   /// </summary>
   public bool AutoSync { get; set; } = false;
}