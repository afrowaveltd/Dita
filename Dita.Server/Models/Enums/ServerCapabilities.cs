namespace Dita.Server.Models.Enums;

/// <summary>
/// Defines server roles used to share server capabilities with clients and other servers.
/// </summary>
[Flags]
public enum ServerCapabilities
{
   /// <summary>
   /// The server has no active roles or capabilities assigned.
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
   /// The server is a member of a cluster.
   /// </summary>
   ClusterMember = 64
}