using Dita.Server.Models.Enums;

namespace Dita.Tests.Server.Models.Enums;

/// <summary>
/// Unit tests for the <see cref="ServerCapabilities"/> enum.
/// </summary>
public class ServerCapabilitiesTests
{
   [Fact]
   public void WhenNoneThenValueIsZero()
   {
      // Arrange & Act
      var capability = ServerCapabilities.None;

      // Assert
      Assert.Equal(0, (int)capability);
   }

   [Theory]
   [InlineData(ServerCapabilities.Discover, 1)]
   [InlineData(ServerCapabilities.DataStorage, 2)]
   [InlineData(ServerCapabilities.TranslationService, 4)]
   [InlineData(ServerCapabilities.IdentityService, 8)]
   [InlineData(ServerCapabilities.EmailService, 16)]
   [InlineData(ServerCapabilities.SharedMailer, 32)]
   [InlineData(ServerCapabilities.ClusterMember, 64)]
   public void WhenCapabilityReadThenExpectedFlagValueIsReturned(ServerCapabilities capability, int expectedValue)
   {
      // Act & Assert
      Assert.Equal(expectedValue, (int)capability);
   }

   [Fact]
   public void WhenCombiningCapabilitiesThenFlagsWork()
   {
      // Arrange
      var combined = ServerCapabilities.Discover | ServerCapabilities.DataStorage;

      // Act & Assert
      Assert.True(combined.HasFlag(ServerCapabilities.Discover));
      Assert.True(combined.HasFlag(ServerCapabilities.DataStorage));
      Assert.False(combined.HasFlag(ServerCapabilities.TranslationService));
   }

   [Fact]
   public void WhenAllCapabilitiesCombinedThenHasAllFlags()
   {
      // Arrange
      var all = ServerCapabilities.Discover |
                ServerCapabilities.DataStorage |
                ServerCapabilities.TranslationService |
                ServerCapabilities.IdentityService |
                ServerCapabilities.EmailService |
                ServerCapabilities.SharedMailer |
                ServerCapabilities.ClusterMember;

      // Act & Assert
      Assert.True(all.HasFlag(ServerCapabilities.Discover));
      Assert.True(all.HasFlag(ServerCapabilities.DataStorage));
      Assert.True(all.HasFlag(ServerCapabilities.TranslationService));
      Assert.True(all.HasFlag(ServerCapabilities.IdentityService));
      Assert.True(all.HasFlag(ServerCapabilities.EmailService));
      Assert.True(all.HasFlag(ServerCapabilities.SharedMailer));
      Assert.True(all.HasFlag(ServerCapabilities.ClusterMember));
   }

   [Fact]
   public void WhenRemovingCapabilityThenFlagIsRemoved()
   {
      // Arrange
      var combined = ServerCapabilities.Discover | ServerCapabilities.DataStorage;

      // Act
      var result = combined & ~ServerCapabilities.Discover;

      // Assert
      Assert.False(result.HasFlag(ServerCapabilities.Discover));
      Assert.True(result.HasFlag(ServerCapabilities.DataStorage));
   }
}