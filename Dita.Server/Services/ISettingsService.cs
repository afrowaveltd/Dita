using Dita.Server.Models.Settings;

namespace Dita.Server.Services;
/// <summary>
/// Defines the contract for a service that manages application settings, including reading, updating, and saving configuration data.
/// </summary>
public interface ISettingsService
{
   /// <summary>
   /// Gets or sets the current server settings used by the application.
   /// </summary>
   ServerSettings Settings { get; set; }

   /// <summary>
   /// Reads the current server settings from the configuration source (e.g., file, database) and returns them as a ServerSettings object.
   /// </summary>
   /// <returns>A new instance of the <see cref="ServerSettings"/> class containing the current settings.</returns>
   ServerSettings ReadSettings();
   /// <summary>
   /// Saves the current server settings to the configuration source (e.g., file, database) immediately, ensuring that any changes made to the settings are persisted.
   /// </summary>
   /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
   /// <returns>A task that represents the asynchronous save operation.</returns>
   Task SaveNowAsync(CancellationToken cancellationToken = default);
   /// <summary>
   /// Updates the current server settings by applying the specified update action, allowing for modifications to the settings in a controlled manner.
   /// </summary>
   /// <param name="updateAction">An action that modifies the current <see cref="ServerSettings"/> instance. Cannot be <see langword="null"/>.</param>
   void UpdateSettings(Action<ServerSettings> updateAction);
}