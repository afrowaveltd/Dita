using Dita.Server.Models.Enums;
using Dita.Server.Models.Settings;

namespace Dita.Tests.Server.Models.Settings;

/// <summary>
/// Unit tests for the <see cref="MainSettings"/> class.
/// </summary>
public class MainSettingsTests
{
   [Fact]
   public void WhenCreatedThenHasDefaultValues()
   {
      // Arrange & Act
      var settings = new MainSettings();

      // Assert
      Assert.NotNull(settings.ServerId);
      Assert.NotEmpty(settings.ServerId);
      Assert.Equal("Dita 01", settings.ServerName);
      Assert.Equal("127.0.0.1:5678", settings.ServerIP);
      Assert.Equal(ServerCapabilities.None, settings.Capabilities);
      Assert.False(settings.MemberOfCluster);
      Assert.False(settings.AutoSync);
   }

   [Fact]
   public void WhenServerIdSetThenValuePersists()
   {
      // Arrange
      var settings = new MainSettings();
      var expectedId = "custom-server-id";

      // Act
      settings.ServerId = expectedId;

      // Assert
      Assert.Equal(expectedId, settings.ServerId);
   }

   [Fact]
   public void WhenServerNameSetThenValuePersists()
   {
      // Arrange
      var settings = new MainSettings();
      var expectedName = "Production Server";

      // Act
      settings.ServerName = expectedName;

      // Assert
      Assert.Equal(expectedName, settings.ServerName);
   }

   [Fact]
   public void WhenServerIPSetThenValuePersists()
   {
      // Arrange
      var settings = new MainSettings();
      var expectedIP = "192.168.1.100:8080";

      // Act
      settings.ServerIP = expectedIP;

      // Assert
      Assert.Equal(expectedIP, settings.ServerIP);
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
      MainSettings settings = new()
      {
         // Act
         Capabilities = capabilities
      };

      // Assert
      Assert.Equal(capabilities, settings.Capabilities);
   }

   [Theory]
   [InlineData(true)]
   [InlineData(false)]
   public void WhenMemberOfClusterSetThenValuePersists(bool value)
   {
      // Arrange
      var settings = new MainSettings
      {
         // Act
         MemberOfCluster = value
      };

      // Assert
      Assert.Equal(value, settings.MemberOfCluster);
   }

   [Theory]
   [InlineData(true)]
   [InlineData(false)]
   public void WhenAutoSyncSetThenValuePersists(bool value)
   {
      // Arrange
      var settings = new MainSettings
      {
         // Act
         AutoSync = value
      };

      // Assert
      Assert.Equal(value, settings.AutoSync);
   }

   [Fact]
   public void WhenMultipleInstancesCreatedThenHaveUniqueServerIds()
   {
      // Arrange & Act
      var settings1 = new MainSettings();
      var settings2 = new MainSettings();

      // Assert
      Assert.NotEqual(settings1.ServerId, settings2.ServerId);
   }
}