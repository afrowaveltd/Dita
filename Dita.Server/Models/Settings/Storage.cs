using Dita.Server.Models.Enums;

namespace Dita.Server.Models.Settings;

/// <summary>
/// Represents the storage configuration for the application, including the type of storage provider, the location where
/// data should be stored, and any necessary connection details. This class allows for flexible configuration of storage
/// options to accommodate different deployment scenarios and requirements.
/// </summary>
public class StorageSettings
{
   /// <summary>
   /// Gets or sets the type of storage to use for data persistence.
   /// </summary>
   /// <remarks>
   /// Use this property to specify the storage backend for the application. The default value is StorageType.AjisFiles.
   /// Changing this property may affect how and where data is stored and retrieved.
   /// </remarks>
   public StorageType StorageType { get; set; } = StorageType.AjisFiles;

   /// <summary>
   /// Gets or sets the storage location used for persisting data.
   /// </summary>
   /// <remarks>
   /// Use this property to specify where data should be stored. The default value is
   /// <see cref="StorageLocation.InFolderStorage"/> . Changing this property may affect data accessibility and
   /// persistence behavior depending on the selected storage location.
   /// </remarks>
   public StorageLocation StorageLocation { get; set; } = StorageLocation.InFolderStorage;

   /// <summary>
   /// Gets or sets the custom file system path to use for operations.
   /// </summary>
   public string? CustomPath { get; set; }

   /// <summary>
   /// Gets or sets the connection string used for connecting to the storage backend.
   /// </summary>
   public string? ConnectionString { get; set; }
}