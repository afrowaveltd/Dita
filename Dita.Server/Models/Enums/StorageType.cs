namespace Dita.Server.Models.Enums;

/// <summary>
/// Defines supported backend storage providers.
/// </summary>
public enum StorageType
{
   /// <summary>
   /// Uses Ajis file-based storage.
   /// </summary>
   AjisFiles,

   /// <summary>
   /// Uses generic JSON file-based storage.
   /// </summary>
   JsonFiles,

   /// <summary>
   /// Uses Entity Framework Core with SQLite provider.
   /// </summary>
   EFCoreSqlite,

   /// <summary>
   /// Uses Entity Framework Core with SQL Server provider.
   /// </summary>
   EFCoreSqlServer,

   /// <summary>
   /// Uses Entity Framework Core with PostgreSQL provider.
   /// </summary>
   EFCorePostgres,

   /// <summary>
   /// Uses Entity Framework Core with MariaDB provider.
   /// </summary>
   EFCoreMariaDb,

   /// <summary>
   /// Uses a Mongo DB provider
   /// </summary>
   MongoDb

}
