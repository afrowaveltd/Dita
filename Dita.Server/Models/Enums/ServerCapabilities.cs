namespace Dita.Server.Models.Enums;

/// <summary>
/// Defines server roles used to share server capabilities with clients and other servers.
/// </summary>
[Flags]
public enum ServerCapabilities
{
   /// <summary>
   /// Server is not having any active roles
   /// </summary>
   None = 0,
   /// <summary>
   /// Server provides discovery capabilities for locating other servers and services.
   /// </summary>
   Discover = 1,

   /// <summary>
   /// Server provides data storage capabilities.
   /// </summary>
   DataStorage = 2,

   /// <summary>
   /// Server provides translation services.
   /// </summary>
   TranslationService = 4,

   /// <summary>
   /// Server provides identity and authentication services.
   /// </summary>
   IdentityService = 8,

   /// <summary>
   /// Server provides email sending services.
   /// </summary>
   EmailService = 16,

   /// <summary>
   /// Server provides shared mailing functionality.
   /// </summary>
   SharedMailer = 32,

   /// <summary>
   /// Server is the member of cluster
   /// </summary>
   ClusterMember = 64
}

