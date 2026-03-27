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

    [Fact]
    public void WhenDiscoverThenValueIsOne()
    {
        // Arrange & Act
        var capability = ServerCapabilities.Discover;

        // Assert
        Assert.Equal(1, (int)capability);
    }

    [Fact]
    public void WhenDataStorageThenValueIsTwo()
    {
        // Arrange & Act
        var capability = ServerCapabilities.DataStorage;

        // Assert
        Assert.Equal(2, (int)capability);
    }

    [Fact]
    public void WhenTranslationServiceThenValueIsFour()
    {
        // Arrange & Act
        var capability = ServerCapabilities.TranslationService;

        // Assert
        Assert.Equal(4, (int)capability);
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
                  ServerCapabilities.TranslationService;

        // Act & Assert
        Assert.True(all.HasFlag(ServerCapabilities.Discover));
        Assert.True(all.HasFlag(ServerCapabilities.DataStorage));
        Assert.True(all.HasFlag(ServerCapabilities.TranslationService));
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
