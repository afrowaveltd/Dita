namespace Dita.Server.Models.Enums;

/// <summary>
/// Defines supported backend storage providers.
/// </summary>
public enum StorageType
{
   /// <summary>
   /// Uses Ajis file-based storage.
   /// </summary>
   AjisFiles = 0,

   /// <summary>
   /// Uses generic JSON file-based storage.
   /// </summary>
   JsonFiles = 1,

   /// <summary>
   /// Uses Entity Framework Core with SQLite provider.
   /// </summary>
   EFCoreSqlite = 2,

   /// <summary>
   /// Uses Entity Framework Core with SQL Server provider.
   /// </summary>
   EFCoreSqlServer = 3,

   /// <summary>
   /// Uses Entity Framework Core with PostgreSQL provider.
   /// </summary>
   EFCorePostgres = 4,

   /// <summary>
   /// Uses Entity Framework Core with MariaDB provider.
   /// </summary>
   EFCoreMariaDb = 5,

   /// <summary>
   /// Uses a Mongo DB provider
   /// </summary>
   MongoDb = 6
}