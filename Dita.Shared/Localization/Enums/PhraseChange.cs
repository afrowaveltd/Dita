namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Representating possible change of each value in the localization dictionary.
/// </summary>
public enum PhraseChange
{
   /// <summary>
   /// Indicates that no change has occurred.
   /// </summary>
   NoChange = 0,
   /// <summary>
   /// Represents an item that has been added.
   /// </summary>
   Added = 1,
   /// <summary>
   /// Indicates that the item has been updated.
   /// </summary>
   Updated = 2,
   /// <summary>
   /// Indicates that the item has been removed.
   /// </summary>
   Removed = 3
}
