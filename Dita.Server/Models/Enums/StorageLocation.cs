namespace Dita.Server.Models.Enums;

/// <summary>
/// Specifies the available locations for storing application data.
/// </summary>
/// <remarks>
/// Use this enumeration to indicate where application data should be saved or retrieved. The selected storage location
/// may affect data accessibility, user profile scope, and application portability. Choose the appropriate value based
/// on the intended persistence and sharing requirements of the data.
/// </remarks>
public enum StorageLocation
{
   /// <summary>
   /// Store data in a folder within the application's installation directory. This is suitable for data that is
   /// specific to the application and does not need to be shared with other applications or users. Note that this
   /// location may require elevated permissions for writing data, especially if the application is installed in a
   /// protected system directory.
   /// </summary>
   InFolderStorage = 0,
   /// <summary>
   /// Store data in the user's profile directory. This is suitable for data that is specific to the user and should
   /// not be shared with other users on the same machine. This location typically does not require elevated
   /// permissions.
   /// </summary>
   InUserProfile = 1,
   /// <summary>
   /// Store data in the application's data directory. This is suitable for data that needs to be shared across
   /// different users or instances of the application. This location may require elevated permissions depending on
   /// the system configuration.
   /// </summary>
   InAppData = 2,
   /// <summary>
   /// Store data in a specified path. This is suitable for data that needs to be stored in a custom location defined
   /// by the user or application. Ensure that the application has the necessary permissions to read and write to
   /// the specified path.
   /// </summary>
   InSpecifiedPath = 3

}
