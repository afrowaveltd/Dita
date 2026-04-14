using Dita.Server.Models.Settings;
using System.Text.Json;

namespace Dita.Server.Services;

/// <summary>
/// Provides thread-safe access to application settings, supporting reading, updating, and persisting settings to a JSON
/// file. Changes to settings are automatically saved in the background, and settings are loaded from disk on
/// initialization.
/// </summary>
/// <remarks>
/// This service ensures that all access to the settings is synchronized and that updates are persisted asynchronously
/// to avoid blocking the main thread. The settings are stored in a JSON file located in the application's 'Settings'
/// directory. If the settings file does not exist or is invalid, default settings are created and used. Logging is
/// performed for key operations and error conditions. This class is intended to be used as a singleton within the
/// application.
/// </remarks>
public class SettingsService : ISettingsService
{
   private readonly ILogger<SettingsService> _logger;
   private readonly SemaphoreSlim _saveSemaphore = new(1, 1);
   private readonly Lock _settingsLock = new();
   private readonly string _settingsFilePath;
   private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

   private ServerSettings _settings;

   /// <summary>
   /// Initializes a new instance of the SettingsService class with the specified logger.
   /// </summary>
   /// <param name="logger">
   /// The logger used to record diagnostic and operational messages for the settings service. Cannot be null.
   /// </param>
   public SettingsService(ILogger<SettingsService> logger)
   {
      _logger = logger;
      _settingsFilePath = GetSettingsFilePath();
      _settings = LoadSettings();
   }

   /// <summary>
   /// Gets or sets the current server settings used by the application.
   /// </summary>
   /// <remarks>
   /// Setting this property updates the server configuration and persists the new settings asynchronously. The value
   /// cannot be null.
   /// </remarks>
   public ServerSettings Settings
   {
      get
      {
         lock(_settingsLock)
         {
            return _settings;
         }
      }
      set
      {
         ArgumentNullException.ThrowIfNull(value);

         ServerSettings snapshot;
         lock(_settingsLock)
         {
            _settings = value;
            snapshot = Clone(_settings);
         }

         SaveInBackground(snapshot);
      }
   }

   /// <summary>
   /// Retrieves a copy of the current server settings in a thread-safe manner.
   /// </summary>
   /// <remarks>
   /// The returned settings object is a clone of the internal state, ensuring that modifications to the returned
   /// instance do not affect the original settings. This method is thread-safe.
   /// </remarks>
   /// <returns>A new instance of the <see cref="ServerSettings"/> class containing the current settings.</returns>
   public ServerSettings ReadSettings()
   {
      lock(_settingsLock)
      {
         return Clone(_settings);
      }
   }

   /// <summary>
   /// Applies the specified update action to the current server settings and persists the changes asynchronously.
   /// </summary>
   /// <remarks>
   /// The update action is executed within a thread-safe lock to ensure consistency. Changes are saved in the
   /// background after the update is applied.
   /// </remarks>
   /// <param name="updateAction">
   /// An action that modifies the current <see cref="ServerSettings"/> instance. Cannot be <see langword="null"/> .
   /// </param>
   public void UpdateSettings(Action<ServerSettings> updateAction)
   {
      ArgumentNullException.ThrowIfNull(updateAction);

      ServerSettings snapshot;
      lock(_settingsLock)
      {
         updateAction(_settings);
         snapshot = Clone(_settings);
      }

      SaveInBackground(snapshot);
   }

   /// <summary>
   /// Asynchronously saves the current server settings to persistent storage.
   /// </summary>
   /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
   /// <returns>A task that represents the asynchronous save operation.</returns>
   public async Task SaveNowAsync(CancellationToken cancellationToken = default)
   {
      ServerSettings snapshot;
      lock(_settingsLock)
      {
         snapshot = Clone(_settings);
      }

      await SaveSettingsAsync(snapshot, cancellationToken);
   }

   private ServerSettings LoadSettings()
   {
      if(!File.Exists(_settingsFilePath))
      {
         _logger.LogInformation("Settings file does not exist. Creating default settings...");
         var defaultSettings = new ServerSettings();
         SaveInBackground(defaultSettings);
         return defaultSettings;
      }

      try
      {
         var json = File.ReadAllText(_settingsFilePath);
         var settings = JsonSerializer.Deserialize<ServerSettings>(json);
         if(settings is null)
         {
            _logger.LogWarning("Settings file exists but could not be deserialized. Using default settings.");
            return new ServerSettings();
         }

         _logger.LogInformation("Settings loaded successfully.");
         return settings;
      }
      catch(IOException ex)
      {
         _logger.LogError(ex, "Failed to read settings file.");
         throw new InvalidOperationException("Failed to read settings file.", ex);
      }
      catch(JsonException ex)
      {
         _logger.LogError(ex, "Settings file contains invalid JSON.");
         throw new InvalidOperationException("Settings file contains invalid JSON.", ex);
      }
   }

   private void SaveInBackground(ServerSettings settings)
   {
      _ = SaveSettingsAsync(settings, CancellationToken.None).ContinueWith(task =>
      {
         _logger.LogError(task.Exception, "Failed to save settings in background.");
      }, TaskContinuationOptions.OnlyOnFaulted);
   }

   private async Task SaveSettingsAsync(ServerSettings settings, CancellationToken cancellationToken)
   {
      await _saveSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
      try
      {
         var json = JsonSerializer.Serialize(settings, SerializerOptions);
         await File.WriteAllTextAsync(_settingsFilePath, json, cancellationToken).ConfigureAwait(false);
         _logger.LogInformation("Settings saved successfully.");
      }
      catch(IOException ex)
      {
         _logger.LogError(ex, "Failed to write settings file.");
         throw new InvalidOperationException("Failed to write settings file.", ex);
      }
      finally
      {
         _saveSemaphore.Release();
      }
   }

   private string GetSettingsFilePath()
   {
      var settingsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
      if(!Directory.Exists(settingsFolder))
      {
         _logger.LogWarning("Settings folder does not exist. Creating...");

         try
         {
            Directory.CreateDirectory(settingsFolder);
#pragma warning disable CA1873 // Vyhněte se potenciálně nákladnému protokolování
            _logger.LogInformation("Settings folder was created successfully. Path is {path}", settingsFolder);
#pragma warning restore CA1873 // Vyhněte se potenciálně nákladnému protokolování
         }
         catch(IOException ex)
         {
            _logger.LogError(ex, "Failed to create settings directory.");
            throw new InvalidOperationException("Failed to create settings directory.", ex);
         }
      }

      return Path.Combine(settingsFolder, "settings.json");
   }

   private static ServerSettings Clone(ServerSettings settings)
   {
      var json = JsonSerializer.Serialize(settings);
      return JsonSerializer.Deserialize<ServerSettings>(json) ?? new ServerSettings();
   }
}