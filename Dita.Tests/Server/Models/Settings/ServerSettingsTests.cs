using Dita.Server.Models.Enums;
using Dita.Server.Models.Settings;

namespace Dita.Tests.Server.Models.Settings;

public class ServerSettingsTests
{
   [Fact]
   public void WhenCreatedThenHasDefaultValues()
   {
      // Arrange & Act
      var settings = new ServerSettings();
      // Assert
      Assert.NotNull(settings.ServerId);
      Assert.NotEmpty(settings.ServerId);
      Assert.Equal(string.Empty, settings.ServerName);
      Assert.Equal(string.Empty, settings.ServerDescription);
      Assert.NotNull(settings.NetworkSettings);
      Assert.Equal(ServerCapabilities.None, settings.Capabilities);
      Assert.False(settings.IsClustered);
   }

   [Fact]
   public void WhenServerNameSetThenValuePersists()
   {
      // Arrange
      var settings = new ServerSettings();
      var expectedName = "Production Server";

      // Act
      settings.ServerName = expectedName;

      // Assert
      Assert.Equal(expectedName, settings.ServerName);
   }

   [Theory]
   [InlineData(ServerCapabilities.None)]
   [InlineData(ServerCapabilities.Discover)]
   [InlineData(ServerCapabilities.DataStorage)]
   [InlineData(ServerCapabilities.TranslationService)]
   [InlineData(ServerCapabilities.Discover | ServerCapabilities.DataStorage)]
   public void WhenCapabilitiesSetThenValuePersists(ServerCapabilities capabilities)
   {
      // Arrange
      ServerSettings settings = new()
      {
         // Act
         Capabilities = capabilities
      };

      // Assert
      Assert.Equal(capabilities, settings.Capabilities);
   }

   [Fact]
   public void WhenMultipleInstancesCreatedThenHaveUniqueServerIds()
   {
      // Arrange & Act
      var settings1 = new ServerSettings();
      var settings2 = new ServerSettings();

      // Assert
      Assert.NotEqual(settings1.ServerId, settings2.ServerId);
   }
}