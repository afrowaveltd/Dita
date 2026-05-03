using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Logging;

namespace Dita.Tests.Shared.Localization.Services;

/// <summary>
/// Tests for <see cref="PlaceholderService"/>.
/// </summary>
public class PlaceholderServiceTests
{
    [Fact]
    public void Format_WithNoPlaceholders_ReturnsOriginalText()
    {
        var service = CreateService();
        string result = service.Format("Hello world");
        Assert.Equal("Hello world", result);
    }

    [Fact]
    public void Format_WithRuntimeValues_ReplacesPlaceholders()
    {
        var service = CreateService();
        var values = new Dictionary<string, string>
        {
            ["name"] = "John",
            ["count"] = "5"
        };

        string result = service.Format("Hello {name}, you have {count} messages", values);
        Assert.Equal("Hello John, you have 5 messages", result);
    }

    [Fact]
    public void Format_WithStoredValues_FallsBackToStorage()
    {
        var service = CreateService();
        service.SetPlaceholder("StorageTestKey", "name", "StoredUser");

        string result = service.Format("StorageTestKey", "Welcome {name}", null);
        Assert.Equal("Welcome StoredUser", result);
    }

    [Fact]
    public void Format_RuntimeValuesOverrideStoredValues()
    {
        var service = CreateService();
        service.SetPlaceholder("OverrideTestKey", "name", "StoredUser");

        var runtime = new Dictionary<string, string> { ["name"] = "RuntimeUser" };
        string result = service.Format("OverrideTestKey", "Welcome {name}", runtime);
        Assert.Equal("Welcome RuntimeUser", result);
    }

    [Fact]
    public void Format_MissingPlaceholder_LeavesPlaceholderUnchanged()
    {
        var service = CreateService();
        string result = service.Format("Hello {missing}");
        Assert.Equal("Hello {missing}", result);
    }

    [Fact]
    public void ExtractPlaceholders_ReturnsUniqueNames()
    {
        var service = CreateService();
        string[] placeholders = service.ExtractPlaceholders("{name} has {count} and {name} again");
        Assert.Equal(2, placeholders.Length);
        Assert.Contains("name", placeholders);
        Assert.Contains("count", placeholders);
    }

    [Fact]
    public void ExtractPlaceholders_WithNoPlaceholders_ReturnsEmpty()
    {
        var service = CreateService();
        string[] placeholders = service.ExtractPlaceholders("No placeholders here");
        Assert.Empty(placeholders);
    }

    [Fact]
    public void HasPlaceholders_WithPlaceholders_ReturnsTrue()
    {
        var service = CreateService();
        Assert.True(service.HasPlaceholders("Hello {name}"));
    }

    [Fact]
    public void HasPlaceholders_WithoutPlaceholders_ReturnsFalse()
    {
        var service = CreateService();
        Assert.False(service.HasPlaceholders("Hello world"));
    }

    [Fact]
    public void HasPlaceholders_WithEmptyString_ReturnsFalse()
    {
        var service = CreateService();
        Assert.False(service.HasPlaceholders(""));
    }

    [Fact]
    public void PrepareForTranslation_WithNoPlaceholders_ReturnsIdentity()
    {
        var service = CreateService();
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation("No placeholders");

        Assert.Equal("No placeholders", prepared);
        // When there are no placeholders, restore should return the translated text as-is
        Assert.Equal("translated text", restore("translated text"));
    }

    [Fact]
    public void PrepareForTranslation_WithPlaceholders_MasksAndRestores()
    {
        var service = CreateService();
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation("Hello {name}, welcome to {app}");

        // Should not contain original placeholders
        Assert.DoesNotContain("{name}", prepared);
        Assert.DoesNotContain("{app}", prepared);
        Assert.Contains("___PH_0___", prepared);
        Assert.Contains("___PH_1___", prepared);

        // Restore should put back original placeholders
        string restored = restore("Bonjour ___PH_0___, bienvenue dans ___PH_1___");
        Assert.Equal("Bonjour {name}, bienvenue dans {app}", restored);
    }

    [Fact]
    public void PrepareForTranslation_RestoreWithMissingToken_LeavesTokenUnchanged()
    {
        var service = CreateService();
        (_, Func<string, string> restore) = service.PrepareForTranslation("Hello {name}");

        // If translation doesn't include the token, it stays as-is
        string restored = restore("Bonjour");
        Assert.Equal("Bonjour", restored);
    }

    [Fact]
    public void PrepareForTranslation_WithReferenceValues_UsesValuesAndRestoresPlaceholders()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["age"] = "42" };

        (string prepared, Func<string, string> restore) = service.PrepareForTranslation("User is {age} years old", references);

        Assert.Equal("User is 42 years old", prepared);
        Assert.Equal("L'utilisateur a {age} ans", restore("L'utilisateur a 42 ans"));
    }

    [Fact]
    public void GetPlaceholders_WithNoStoredValues_ReturnsEmpty()
    {
        var service = CreateService();
        var placeholders = service.GetPlaceholders("NonExistentKey");
        Assert.Empty(placeholders);
    }

    [Fact]
    public void GetPlaceholders_WithStoredValues_ReturnsValues()
    {
        var service = CreateService();
        service.SetPlaceholder("MyKey", "name", "John");
        service.SetPlaceholder("MyKey", "count", "5");

        var placeholders = service.GetPlaceholders("MyKey");
        Assert.Equal(2, placeholders.Count);
        Assert.Equal("John", placeholders["name"]);
        Assert.Equal("5", placeholders["count"]);
    }

    [Fact]
    public void RemoveKey_RemovesAllPlaceholdersForKey()
    {
        var service = CreateService();
        service.SetPlaceholder("Key1", "name", "John");
        service.RemoveKey("Key1");

        var placeholders = service.GetPlaceholders("Key1");
        Assert.Empty(placeholders);
    }

    [Fact]
    public void Format_WithNullOrWhitespace_ReturnsOriginal()
    {
        var service = CreateService();
        Assert.Equal("", service.Format(""));
        Assert.Equal("   ", service.Format("   "));
    }

    [Fact]
    public void ExtractPlaceholders_WithComplexNames_HandlesAlphanumeric()
    {
        var service = CreateService();
        string[] placeholders = service.ExtractPlaceholders("{userName123} and {app_name}");
        Assert.Equal(2, placeholders.Length);
        Assert.Contains("userName123", placeholders);
        Assert.Contains("app_name", placeholders);
    }

    [Fact]
    public void Format_WithMultipleSamePlaceholder_ReplacesAll()
    {
        var service = CreateService();
        var values = new Dictionary<string, string> { ["name"] = "Alice" };

        string result = service.Format("{name} says hello to {name}", values);
        Assert.Equal("Alice says hello to Alice", result);
    }

    [Fact(Skip = "Concurrency issue with shared placeholders.json file. Test passes when run in isolation.")]
    public async Task SaveAndLoad_PreservesPlaceholders()
    {
        // Use a unique key to avoid conflicts with other tests
        string testKey = $"SaveLoadTest_{Guid.NewGuid():N}";
        
        var service = new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>());
        // Explicitly load to set _loaded = true so EnsureLoaded doesn't overwrite later
        await service.LoadAsync();
        service.SetPlaceholder(testKey, "name", "SavedValue");
        await service.SaveAsync();

        var newService = new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>());
        await newService.LoadAsync();

        Assert.True(newService.GetPlaceholders(testKey).ContainsKey("name"), 
            $"Expected '{testKey}' to contain 'name' placeholder. File may not have been saved correctly.");
        Assert.Equal("SavedValue", newService.GetPlaceholders(testKey)["name"]);
    }

    private static PlaceholderService CreateService()
    {
        // Clean up any existing placeholders file to ensure isolated test state
        string filePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory[..AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin")],
            "Locales", "placeholders.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>());
    }
}
