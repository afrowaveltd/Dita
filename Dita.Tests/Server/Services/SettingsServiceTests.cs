using Dita.Server.Models.Settings;
using Dita.Server.Services;
using Microsoft.Extensions.Logging;

namespace Dita.Tests.Server.Services;

[Collection("SettingsServiceTests")]
public class SettingsServiceTests : IDisposable
{
   private readonly string _settingsFilePath;
   private readonly bool _originalExists;
   private readonly string _originalContent = string.Empty;

   public SettingsServiceTests()
   {
      _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings", "settings.json");

      if(File.Exists(_settingsFilePath))
      {
         _originalExists = true;
         _originalContent = File.ReadAllText(_settingsFilePath);
      }

      Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);
   }

   [Fact]
   public async Task WhenSettingsAssignedThenChangeIsPersistedWithoutAwaitingSetter()
   {
      // Arrange
      WriteSettings("Initial");
      var logger = Substitute.For<ILogger<SettingsService>>();
      var service = new SettingsService(logger);
      var updatedSettings = new ServerSettings { ServerName = "Updated" };

      // Act
      service.Settings = updatedSettings;

      // Assert
      var json = await WaitForFileContentAsync(content => content.Contains("\"ServerName\": \"Updated\""));
      Assert.Contains("\"ServerName\": \"Updated\"", json);
   }

   [Fact]
   public async Task WhenUpdateSettingsCalledThenChangeIsPersisted()
   {
      // Arrange
      WriteSettings("Initial");
      var logger = Substitute.For<ILogger<SettingsService>>();
      var service = new SettingsService(logger);

      // Act
      service.UpdateSettings(settings => settings.ServerDescription = "UpdatedDescription");

      // Assert
      var json = await WaitForFileContentAsync(content => content.Contains("\"ServerDescription\": \"UpdatedDescription\""));
      Assert.Contains("\"ServerDescription\": \"UpdatedDescription\"", json);
   }

   [Fact]
   public void WhenReadSettingsCalledThenReturnsSnapshot()
   {
      // Arrange
      WriteSettings("Initial");
      var logger = Substitute.For<ILogger<SettingsService>>();
      var service = new SettingsService(logger);

      // Act
      var snapshot = service.ReadSettings();
      snapshot.ServerName = "MutatedOutsideService";

      // Assert
      Assert.NotEqual("MutatedOutsideService", service.Settings.ServerName);
   }

   public void Dispose()
   {
      if(_originalExists)
      {
         File.WriteAllText(_settingsFilePath, _originalContent);
      }
      else if(File.Exists(_settingsFilePath))
      {
         File.Delete(_settingsFilePath);
      }

      GC.SuppressFinalize(this);
   }

   private void WriteSettings(string serverName)
   {
      var settings = new ServerSettings { ServerName = serverName, ServerDescription = "Description" };
      WriteSettingsFile(_settingsFilePath, settings, GetOptions());
   }

   private static System.Text.Json.JsonSerializerOptions GetOptions()
   {
      return new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
   }

   private static void WriteSettingsFile(string path, ServerSettings settings, System.Text.Json.JsonSerializerOptions jsonOptions)
   {
      var json = System.Text.Json.JsonSerializer.Serialize(settings, jsonOptions);
      File.WriteAllText(path, json);
   }

   private async Task<string> WaitForFileContentAsync(Func<string, bool> predicate)
   {
      const int retries = 20;

      for(var attempt = 0; attempt < retries; attempt++)
      {
         try
         {
            var json = await File.ReadAllTextAsync(_settingsFilePath);
            if(predicate(json))
            {
               return json;
            }
         }
         catch(IOException)
         {
            // Retry on transient lock while background save is writing.
         }

         await Task.Delay(50);
      }

      return await File.ReadAllTextAsync(_settingsFilePath);
   }
}

[CollectionDefinition("SettingsServiceTests", DisableParallelization = true)]
public class SettingsServiceTestsCollection;