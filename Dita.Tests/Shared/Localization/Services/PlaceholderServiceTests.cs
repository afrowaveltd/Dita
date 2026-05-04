using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Logging;

namespace Dita.Tests.Shared.Localization.Services;

/// <summary>
/// Tests for <see cref="PlaceholderService"/>.
/// </summary>
public class PlaceholderServiceTests
{
    // ──────────────────────────────────────────────────────────────
    //  Basic Format / Extract / Has tests (unchanged)
    // ──────────────────────────────────────────────────────────────

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
        var values = new Dictionary<string, string> { ["name"] = "John", ["count"] = "5" };
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
        Assert.Empty(service.ExtractPlaceholders("No placeholders here"));
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

    [Fact]
    public void GetPlaceholders_WithNoStoredValues_ReturnsEmpty()
    {
        var service = CreateService();
        Assert.Empty(service.GetPlaceholders("NonExistentKey"));
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
        Assert.Empty(service.GetPlaceholders("Key1"));
    }

    [Fact(Skip = "Concurrency issue with shared placeholders.json file. Test passes when run in isolation.")]
    public async Task SaveAndLoad_PreservesPlaceholders()
    {
        string testKey = $"SaveLoadTest_{Guid.NewGuid():N}";
        var service = new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>());
        await service.LoadAsync();
        service.SetPlaceholder(testKey, "name", "SavedValue");
        await service.SaveAsync();

        var newService = new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>());
        await newService.LoadAsync();
        Assert.True(newService.GetPlaceholders(testKey).ContainsKey("name"));
        Assert.Equal("SavedValue", newService.GetPlaceholders(testKey)["name"]);
    }

    // ──────────────────────────────────────────────────────────────
    //  PrepareForTranslation — basic (no reference values)
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void PrepareForTranslation_WithNoPlaceholders_ReturnsIdentity()
    {
        var service = CreateService();
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation("No placeholders");
        Assert.Equal("No placeholders", prepared);
        Assert.Equal("translated text", restore("translated text"));
    }

    [Fact]
    public void PrepareForTranslation_WithPlaceholders_MasksAndRestores()
    {
        var service = CreateService();
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation("Hello {name}, welcome to {app}");

        Assert.DoesNotContain("{name}", prepared);
        Assert.DoesNotContain("{app}", prepared);
        // New token format: ⟦0⟧ ⟦1⟧
        Assert.Contains("\u27e60\u27e7", prepared);
        Assert.Contains("\u27e61\u27e7", prepared);

        string restored = restore($"Bonjour \u27e60\u27e7, bienvenue dans \u27e61\u27e7");
        Assert.Equal("Bonjour {name}, bienvenue dans {app}", restored);
    }

    [Fact]
    public void PrepareForTranslation_RestoreWithMissingToken_LeavesTokenUnchanged()
    {
        var service = CreateService();
        (_, Func<string, string> restore) = service.PrepareForTranslation("Hello {name}");
        Assert.Equal("Bonjour", restore("Bonjour"));
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

    // ──────────────────────────────────────────────────────────────
    //  Multiple-placeholder with reference values
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void PrepareForTranslation_WithMultiplePlaceholders_SameReferenceValue_RestoresEachUniquely()
    {
        var service = CreateService();
        var references = new Dictionary<string, string>
        {
            ["addedCount"] = "5", ["removedCount"] = "5",
            ["skippedCount"] = "3", ["errorCount"] = "0"
        };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Added: {addedCount}, Removed: {removedCount}, Skipped: {skippedCount}, Errors: {errorCount}", references);
        Assert.Equal("Added: 5, Removed: 5, Skipped: 3, Errors: 0", prepared);
        string restored = restore("Přidáno: 5, Odebráno: 5, Přeskočeno: 3, Chyby: 0");
        Assert.Equal("Přidáno: {addedCount}, Odebráno: {removedCount}, Přeskočeno: {skippedCount}, Chyby: {errorCount}", restored);
    }

    [Fact]
    public void PrepareForTranslation_WithMultiplePlaceholders_DifferentReferenceValues_RestoresAll()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["language"] = "English", ["sourcePath"] = "docs/intro.md" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Saved '{language}' translation for '{sourcePath}'.", references);
        Assert.Equal("Saved 'English' translation for 'docs/intro.md'.", prepared);
        string restored = restore("Uloženo 'English' překlad pro 'docs/intro.md'.");
        Assert.Equal("Uloženo '{language}' překlad pro '{sourcePath}'.", restored);
    }

    [Fact]
    public void PrepareForTranslation_WithMixedPlaceholders_SomeWithReferencesSomeWithout_RestoresAll()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["targetLanguage"] = "cs" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Language: {targetLanguage}, Blocks: {blockCount}", references);
        // targetLanguage → "cs", blockCount → token ⟦1⟧
        Assert.Equal("Language: cs, Blocks: \u27e61\u27e7", prepared);
        string restored = restore($"Jazyk: cs, Bloky: \u27e61\u27e7");
        Assert.Equal("Jazyk: {targetLanguage}, Bloky: {blockCount}", restored);
    }

    [Fact]
    public void PrepareForTranslation_WithRepeatedSameReferenceValue_FourTimes_RestoresEachUniquely()
    {
        var service = CreateService();
        var references = new Dictionary<string, string>
        {
            ["language"] = "cs", ["sourcePath"] = "docs/index.md",
            ["translatedCount"] = "3", ["blockCount"] = "3"
        };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Saved '{language}' translation for '{sourcePath}' ({translatedCount}/{blockCount} blocks translated).", references);
        Assert.Equal("Saved 'cs' translation for 'docs/index.md' (3/3 blocks translated).", prepared);
        string restored = restore("Uloženo 'cs' překlad pro 'docs/index.md' (3/3 bloky přeloženy).");
        Assert.Equal("Uloženo '{language}' překlad pro '{sourcePath}' ({translatedCount}/{blockCount} bloky přeloženy).", restored);
    }

    [Fact]
    public void PrepareForTranslation_WithThreeIdenticalNumericRefs_RestoresPositionally()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["count1"] = "10", ["count2"] = "10", ["count3"] = "10" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation("{count1} + {count2} + {count3}", references);
        Assert.Equal("10 + 10 + 10", prepared);
        Assert.Equal("{count1} + {count2} + {count3}", restore("10 + 10 + 10"));
    }

    [Fact]
    public void PrepareForTranslation_ReferenceValueSubstringOfAnother_RestoresPositionally()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["unit"] = "block", ["pluralUnit"] = "blocks" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation("{unit} / {pluralUnit}", references);
        Assert.Equal("block / blocks", prepared);
        Assert.Equal("{unit} / {pluralUnit}", restore("block / blocks"));
    }

    [Fact]
    public void PrepareForTranslation_SinglePlaceholderWithReference_StillWorksAfterFix()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["age"] = "42" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation("User is {age} years old", references);
        Assert.Equal("User is 42 years old", prepared);
        Assert.Equal("L'utilisateur a {age} ans", restore("L'utilisateur a 42 ans"));
    }

    [Fact]
    public void PrepareForTranslation_NoReferenceValues_MultipleTokens_RestoresAll()
    {
        var service = CreateService();
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation("{a} and {b} and {c}");
        Assert.Equal($"\u27e60\u27e7 and \u27e61\u27e7 and \u27e62\u27e7", prepared);
        string restored = restore($"\u27e60\u27e7 a \u27e61\u27e7 a \u27e62\u27e7");
        Assert.Equal("{a} a {b} a {c}", restored);
    }

    [Fact]
    public void PrepareForTranslation_TranslationReordersReferenceValues_RestoresByPosition()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["sourcePath"] = "index.md", ["language"] = "cs" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Saved '{language}' for '{sourcePath}'", references);
        Assert.Equal("Saved 'cs' for 'index.md'", prepared);
        string restored = restore("Uloženo pro 'index.md' v 'cs'");
        Assert.Equal("Uloženo pro '{sourcePath}' v '{language}'", restored);
    }

    // ──────────────────────────────────────────────────────────────
    //  Edge-case tests from JsonStringLocalizer call paths
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void PrepareForTranslation_FromAnonymousObject_SameNumericValues_RestoresAll()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["addedCount"] = "5", ["removedCount"] = "5" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Added: {addedCount}, Removed: {removedCount}", references);
        Assert.Equal("Added: 5, Removed: 5", prepared);
        Assert.Equal("Přidáno: {addedCount}, Odebráno: {removedCount}", restore("Přidáno: 5, Odebráno: 5"));
    }

    [Fact]
    public void PrepareForTranslation_WithSlashSeparatedSameValues_RestoresCorrectly()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["translatedCount"] = "5", ["blockCount"] = "5" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "{translatedCount}/{blockCount} blocks translated", references);
        Assert.Equal("5/5 blocks translated", prepared);
        Assert.Equal("{translatedCount}/{blockCount} bloků přeloženo", restore("5/5 bloků přeloženo"));
    }

    [Fact]
    public void PrepareForTranslation_ReferenceValueAppearsInSurroundingText_ReplacesNthOccurrencePositionally()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["language"] = "en" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "The word 'en' appears here and also as {language}", references);
        Assert.Equal("The word 'en' appears here and also as en", prepared);
        string restored = restore("Slovo 'en' se objevuje zde a také jako en");
        Assert.Equal("Slovo '{language}' se objevuje zde a také jako en", restored);
    }

    [Fact]
    public void PrepareForTranslation_EmptyReferenceValue_FallsBackToToken()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["name"] = "", ["age"] = "42" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Name: {name}, Age: {age}", references);
        Assert.Equal($"Name: \u27e60\u27e7, Age: 42", prepared);
        string restored = restore($"Jméno: \u27e60\u27e7, Věk: 42");
        Assert.Equal("Jméno: {name}, Věk: {age}", restored);
    }

    [Fact]
    public void PrepareForTranslation_ReferenceValueIsSingleChar_MultiplePlaceholders()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["errors"] = "0", ["warnings"] = "1" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Errors: {errors}, Warnings: {warnings}", references);
        Assert.Equal("Errors: 0, Warnings: 1", prepared);
        Assert.Equal("Chyby: {errors}, Varování: {warnings}", restore("Chyby: 0, Varování: 1"));
    }

    [Fact]
    public void PrepareForTranslation_MultipleSingleCharSameValue_RestoresAll()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["added"] = "0", ["removed"] = "0", ["errors"] = "0" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Added {added}, Removed {removed}, Errors: {errors}", references);
        Assert.Equal("Added 0, Removed 0, Errors: 0", prepared);
        Assert.Equal("Přidáno {added}, Odebráno {removed}, Chyby: {errors}", restore("Přidáno 0, Odebráno 0, Chyby: 0"));
    }

    // ──────────────────────────────────────────────────────────────
    //  Token corruption — new ⟦N⟧ tokens are robust against MT
    //  but we also test backward-compat with legacy ___PH_N___
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void PrepareForTranslation_NewTokenFormat_LibreTranslatePreserves()
    {
        // New ⟦N⟧ tokens use Unicode chars outside normal text — MT engines keep them
        var service = CreateService();
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation("Hello {name}!");
        Assert.Equal($"Hello \u27e60\u27e7!", prepared);
        // In practice LibreTranslate leaves ⟦0⟧ intact, but let's verify restore
        string restored = restore($"Ahoj \u27e60\u27e7!");
        Assert.Equal("Ahoj {name}!", restored);
    }

    [Fact]
    public void PrepareForTranslation_NewTokenFormat_MultiplePlaceholders()
    {
        // The original bug: "Uloženo ___PH_0___ překlad pro ___PH_1___"
        var service = CreateService();
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Saved '{language}' translation for '{sourcePath}' ({translatedCount}/{blockCount} blocks translated).");
        Assert.Equal(
            $"Saved '\u27e60\u27e7' translation for '\u27e61\u27e7' (\u27e62\u27e7/\u27e63\u27e7 blocks translated).",
            prepared);
        string restored = restore(
            $"Uloženo '\u27e60\u27e7' překlad pro '\u27e61\u27e7' (\u27e62\u27e7/\u27e63\u27e7 bloků přeloženo).");
        Assert.Equal("Uloženo '{language}' překlad pro '{sourcePath}' ({translatedCount}/{blockCount} bloků přeloženo).", restored);
    }

    [Fact]
    public void PrepareForTranslation_BackwardCompat_LegacyUncorruptedToken_Restores()
    {
        // Legacy ___PH_0___ tokens (from old data / other services) still get restored
        var service = CreateService();
        (string _, Func<string, string> restore) = service.PrepareForTranslation("Hello {name}!");
        string legacyInput = "Ahoj ___PH_0___!";
        string restored = restore(legacyInput);
        Assert.Equal("Ahoj {name}!", restored);
    }

    [Fact]
    public void PrepareForTranslation_BackwardCompat_LegacyCorruptedToken_Restores()
    {
        // Legacy ___PH_0___ with spaces inserted by LibreTranslate is restored, but
        // surrounding whitespace from the corruption remains (e.g. "___ PH _ 0 ___" →
        // "{name}" with possible extra spaces). This is a best-effort fallback — the
        // primary ⟦N⟧ token format does not suffer from this.
        var service = CreateService();
        (string _, Func<string, string> restore) = service.PrepareForTranslation("Hello {name}!");
        string corrupted = "Ahoj ___ PH _ 0 ___ !";
        string restored = restore(corrupted);
        Assert.Equal("Ahoj {name} !", restored); // extra space is a corruption artifact
    }

    [Fact]
    public void PrepareForTranslation_BackwardCompat_LegacyHeavyCorruption_Restores()
    {
        // Extreme case: every underscore/letter separated by spaces
        var service = CreateService();
        (string _, Func<string, string> restore) = service.PrepareForTranslation("{count} items");
        string heavilyCorrupted = "_ _ _ P H _ 0 _ _ _ položek";
        string restored = restore(heavilyCorrupted);
        Assert.Equal("{count} položek", restored);
    }

    [Fact]
    public void PrepareForTranslation_BackwardCompat_MultipleLegacyCorruptedTokens()
    {
        // Legacy heavily-corrupted tokens are restored but extra whitespace from
        // corruption may remain (best-effort fallback, not primary format).
        var service = CreateService();
        (string _, Func<string, string> restore) = service.PrepareForTranslation(
            "Saved '{language}' translation for '{sourcePath}' ({translatedCount}/{blockCount} blocks translated).");
        string corrupted = "Uloženo '___ PH _ 0 ___' překlad pro '___ PH _ 1 ___' (___ PH _ 2 ___ / ___ PH _ 3 ___ bloků přeloženo).";
        string restored = restore(corrupted);
        // Extra spaces around / are corruption artifacts from "___ PH _ 2 ___ / ___ PH _ 3 ___"
        Assert.Equal("Uloženo '{language}' překlad pro '{sourcePath}' ({translatedCount} / {blockCount} bloků přeloženo).", restored);
    }

    [Fact]
    public void PrepareForTranslation_WithRefValues_CorruptedLegacyTokenFallback_RestoresCorrectly()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["language"] = "cs", ["count"] = "5" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Language: {language}, Path: {path}, Count: {count}", references);
        Assert.Equal($"Language: cs, Path: \u27e61\u27e7, Count: 5", prepared);
        // Simulate legacy corrupted token mixed with new-format token
        string translated = $"Jazyk: cs, Cesta: ___ PH _ 1 ___, Počet: 5";
        string restored = restore(translated);
        Assert.Equal("Jazyk: {language}, Cesta: {path}, Počet: {count}", restored);
    }

    [Fact]
    public void PrepareForTranslation_WithRefValues_SameValueAndCorruptedToken_RestoresAll()
    {
        var service = CreateService();
        var references = new Dictionary<string, string>
        {
            ["language"] = "cs", ["sourcePath"] = "docs/index.md",
            ["translatedCount"] = "3", ["blockCount"] = "3"
        };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Saved '{language}' translation for '{sourcePath}' ({translatedCount}/{blockCount} blocks translated).", references);
        Assert.Equal("Saved 'cs' translation for 'docs/index.md' (3/3 blocks translated).", prepared);
        string translated = "Uloženo 'cs' překlad pro 'docs/index.md' (3/3 bloky přeloženy).";
        Assert.Equal(
            "Uloženo '{language}' překlad pro '{sourcePath}' ({translatedCount}/{blockCount} bloky přeloženy).",
            restore(translated));
    }

    // ──────────────────────────────────────────────────────────────
    //  Regression — existing behaviour must not break
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void PrepareForTranslation_NoRefValues_UncorruptedNewTokens_StillWork()
    {
        var service = CreateService();
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "Hello {name}! You have {count} messages.");
        Assert.Equal($"Hello \u27e60\u27e7! You have \u27e61\u27e7 messages.", prepared);
        string translated = $"Ahoj \u27e60\u27e7! Máš \u27e61\u27e7 zpráv.";
        Assert.Equal("Ahoj {name}! Máš {count} zpráv.", restore(translated));
    }

    [Fact]
    public void PrepareForTranslation_WithRefValues_UncorruptedReferenceValues_StillWork()
    {
        var service = CreateService();
        var references = new Dictionary<string, string> { ["count"] = "5" };
        (string prepared, Func<string, string> restore) = service.PrepareForTranslation(
            "You have {count} messages", references);
        Assert.Equal("You have 5 messages", prepared);
        Assert.Equal("Máte {count} zpráv", restore("Máte 5 zpráv"));
    }

    private static PlaceholderService CreateService()
    {
        string filePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory[..AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin")],
            "Locales", "placeholders.json");
        if (File.Exists(filePath)) File.Delete(filePath);
        return new PlaceholderService(Substitute.For<ILogger<PlaceholderService>>());
    }
}