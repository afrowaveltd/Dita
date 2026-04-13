using Dita.Server.Models.Enums;
using Dita.Server.Models.Settings;

namespace Dita.Tests.Server.Models.Settings;

public class AdditionalSettingsTests
{
   [Fact]
   public void WhenBrandCreatedThenDefaultValuesAreApplied()
   {
      var brand = new Brand();

      Assert.Equal("Afrowave", brand.Name);
      Assert.Equal(string.Empty, brand.LogoUrl);
      Assert.Equal(string.Empty, brand.SupportEmail);
   }

   [Fact]
   public void WhenLocalizationSettingsCreatedThenDefaultValuesAreApplied()
   {
      var settings = new LocalizationSettings();

      Assert.Equal("en", settings.DefaultLanguage);
      Assert.Equal("false", settings.UseAutomaticTranslation);
   }

   [Fact]
   public void WhenNetworkSettingsCreatedThenUrlsUseConfiguredDefaults()
   {
      var settings = new NetworkSettings();

      Assert.Equal("127.0.0.1", settings.IpAddress);
      Assert.Equal(5678, settings.Port);
      Assert.Equal(5679, settings.SecurePort);
      Assert.Equal("http://127.0.0.1:5678", settings.Url);
      Assert.Equal("https://127.0.0.1:5679", settings.SecureUrl);
   }

   [Fact]
   public void WhenNetworkSettingsChangedThenComputedUrlsReflectAssignedValues()
   {
      var settings = new NetworkSettings
      {
         IpAddress = "192.168.1.10",
         Port = 8080,
         SecurePort = 8443
      };

      Assert.Equal("http://192.168.1.10:8080", settings.Url);
      Assert.Equal("https://192.168.1.10:8443", settings.SecureUrl);
   }

   [Fact]
   public void WhenStorageCreatedThenDefaultValuesAreApplied()
   {
      var storage = new Storage();

      Assert.Equal(StorageType.AjisFiles, storage.StorageType);
      Assert.Equal(StorageLocation.InFolderStorage, storage.StorageLocation);
      Assert.Null(storage.CustomPath);
      Assert.Null(storage.ConnectionString);
   }

   [Fact]
   public void WhenStorageConfiguredThenAssignedValuesPersist()
   {
      var storage = new Storage
      {
         StorageType = StorageType.MongoDb,
         StorageLocation = StorageLocation.InSpecifiedPath,
         CustomPath = "C:/data/dita",
         ConnectionString = "mongodb://localhost:27017"
      };

      Assert.Equal(StorageType.MongoDb, storage.StorageType);
      Assert.Equal(StorageLocation.InSpecifiedPath, storage.StorageLocation);
      Assert.Equal("C:/data/dita", storage.CustomPath);
      Assert.Equal("mongodb://localhost:27017", storage.ConnectionString);
   }
}