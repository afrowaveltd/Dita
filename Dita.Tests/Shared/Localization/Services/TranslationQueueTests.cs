using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Xunit;

namespace Dita.Tests.Shared.Localization.Services;

/// <summary>
/// Tests for the <see cref="TranslationQueue"/> class.
/// </summary>
public class TranslationQueueTests
{
   private readonly ITranslationQueue _queue;

   public TranslationQueueTests()
   {
      _queue = new TranslationQueue();
   }

   [Fact]
   public void Count_WhenEmpty_ReturnsZero()
   {
      // Assert
      Assert.Equal(0, _queue.Count);
   }

   [Fact]
   public void Enqueue_AddsSinglePhrase_IncreasesCount()
   {
      // Arrange
      PhraseInQueue phrase = new()
      {
         Key = "test.key",
         Phrase = "Test phrase",
         SourceLanguage = "en",
         TargetLanguage = "cs"
      };

      // Act
      _queue.Enqueue(phrase);

      // Assert
      Assert.Equal(1, _queue.Count);
   }

   [Fact]
   public void EnqueueRange_AddsMultiplePhrases_IncreasesCount()
   {
      // Arrange
      List<PhraseInQueue> phrases =
      [
         new() { Key = "key1", Phrase = "Phrase 1" },
         new() { Key = "key2", Phrase = "Phrase 2" },
         new() { Key = "key3", Phrase = "Phrase 3" }
      ];

      // Act
      _queue.EnqueueRange(phrases);

      // Assert
      Assert.Equal(3, _queue.Count);
   }

   [Fact]
   public void FindByKey_WhenKeyExists_ReturnsPhrase()
   {
      // Arrange
      PhraseInQueue phrase = new()
      {
         Key = "find.me",
         Phrase = "Findable phrase"
      };
      _queue.Enqueue(phrase);

      // Act
      PhraseInQueue? found = _queue.FindByKey("find.me");

      // Assert
      Assert.NotNull(found);
      Assert.Equal("Findable phrase", found.Phrase);
   }

   [Fact]
   public void FindByKey_WhenKeyDoesNotExist_ReturnsNull()
   {
      // Act
      PhraseInQueue? found = _queue.FindByKey("nonexistent.key");

      // Assert
      Assert.Null(found);
   }

   [Fact]
   public void Remove_WhenKeyExists_RemovesPhrase()
   {
      // Arrange
      PhraseInQueue phrase = new() { Key = "remove.me", Phrase = "To be removed" };
      _queue.Enqueue(phrase);

      // Act
      bool removed = _queue.Remove("remove.me");

      // Assert
      Assert.True(removed);
      Assert.Equal(0, _queue.Count);
   }

   [Fact]
   public void Remove_WhenKeyDoesNotExist_ReturnsFalse()
   {
      // Act
      bool removed = _queue.Remove("nonexistent.key");

      // Assert
      Assert.False(removed);
   }

   [Fact]
   public void Update_WhenKeyExists_UpdatesPhrase()
   {
      // Arrange
      PhraseInQueue original = new()
      {
         Key = "update.me",
         Phrase = "Original text",
         SourceLanguage = "en"
      };
      _queue.Enqueue(original);

      PhraseInQueue updated = new()
      {
         Key = "update.me",
         Phrase = "Updated text",
         SourceLanguage = "cs"
      };

      // Act
      bool result = _queue.Update(updated);

      // Assert
      Assert.True(result);
      PhraseInQueue? found = _queue.FindByKey("update.me");
      Assert.NotNull(found);
      Assert.Equal("Updated text", found.Phrase);
      Assert.Equal("cs", found.SourceLanguage);
   }

   [Fact]
   public void Update_WhenKeyDoesNotExist_ReturnsFalse()
   {
      // Arrange
      PhraseInQueue phrase = new() { Key = "nonexistent", Phrase = "Does not exist" };

      // Act
      bool result = _queue.Update(phrase);

      // Assert
      Assert.False(result);
   }

   [Fact]
   public void Clear_RemovesAllPhrases()
   {
      // Arrange
      _queue.EnqueueRange(
      [
         new() { Key = "key1", Phrase = "Phrase 1" },
         new() { Key = "key2", Phrase = "Phrase 2" }
      ]);

      // Act
      _queue.Clear();

      // Assert
      Assert.Equal(0, _queue.Count);
   }

   [Fact]
   public void GetPendingAdditions_ReturnsOnlyAddedPhrases()
   {
      // Arrange
      _queue.EnqueueRange(
      [
         new() { Key = "add1", Phrase = "Added", ChangeRequired = PhraseChange.Added },
         new() { Key = "upd1", Phrase = "Updated", ChangeRequired = PhraseChange.Updated },
         new() { Key = "add2", Phrase = "Also added", ChangeRequired = PhraseChange.Added }
      ]);

      // Act
      List<PhraseInQueue> additions = _queue.GetPendingAdditions();

      // Assert
      Assert.Equal(2, additions.Count);
      Assert.All(additions, p => Assert.Equal(PhraseChange.Added, p.ChangeRequired));
   }

   [Fact]
   public void GetPendingRemovals_ReturnsOnlyRemovedPhrases()
   {
      // Arrange
      _queue.EnqueueRange(
      [
         new() { Key = "rem1", Phrase = "Removed", ChangeRequired = PhraseChange.Removed },
         new() { Key = "add1", Phrase = "Added", ChangeRequired = PhraseChange.Added },
         new() { Key = "rem2", Phrase = "Also removed", ChangeRequired = PhraseChange.Removed }
      ]);

      // Act
      List<PhraseInQueue> removals = _queue.GetPendingRemovals();

      // Assert
      Assert.Equal(2, removals.Count);
      Assert.All(removals, p => Assert.Equal(PhraseChange.Removed, p.ChangeRequired));
   }

   [Fact]
   public void GetPendingUpdates_ReturnsOnlyUpdatedPhrases()
   {
      // Arrange
      _queue.EnqueueRange(
      [
         new() { Key = "upd1", Phrase = "Updated", ChangeRequired = PhraseChange.Updated },
         new() { Key = "add1", Phrase = "Added", ChangeRequired = PhraseChange.Added },
         new() { Key = "upd2", Phrase = "Also updated", ChangeRequired = PhraseChange.Updated }
      ]);

      // Act
      List<PhraseInQueue> updates = _queue.GetPendingUpdates();

      // Assert
      Assert.Equal(2, updates.Count);
      Assert.All(updates, p => Assert.Equal(PhraseChange.Updated, p.ChangeRequired));
   }

   [Fact]
   public void GetUntranslated_ReturnsOnlyUntranslatedPhrases()
   {
      // Arrange
      _queue.EnqueueRange(
      [
         new() { Key = "un1", Phrase = "Untranslated", IsTranslated = false },
         new() { Key = "tr1", Phrase = "Translated", IsTranslated = true },
         new() { Key = "un2", Phrase = "Also untranslated", IsTranslated = false }
      ]);

      // Act
      List<PhraseInQueue> untranslated = _queue.GetUntranslated();

      // Assert
      Assert.Equal(2, untranslated.Count);
      Assert.All(untranslated, p => Assert.False(p.IsTranslated));
   }

   [Fact]
   public void GetTranslated_ReturnsOnlyTranslatedPhrases()
   {
      // Arrange
      _queue.EnqueueRange(
      [
         new() { Key = "tr1", Phrase = "Translated", IsTranslated = true },
         new() { Key = "un1", Phrase = "Untranslated", IsTranslated = false },
         new() { Key = "tr2", Phrase = "Also translated", IsTranslated = true }
      ]);

      // Act
      List<PhraseInQueue> translated = _queue.GetTranslated();

      // Assert
      Assert.Equal(2, translated.Count);
      Assert.All(translated, p => Assert.True(p.IsTranslated));
   }

   [Fact]
   public void MarkTranslationStarted_SetsStartTimeAndResetsIsTranslated()
   {
      // Arrange
      PhraseInQueue phrase = new() { Key = "start.me", Phrase = "Starting", IsTranslated = true };
      _queue.Enqueue(phrase);

      // Act
      bool result = _queue.MarkTranslationStarted("start.me");

      // Assert
      Assert.True(result);
      PhraseInQueue? found = _queue.FindByKey("start.me");
      Assert.NotNull(found);
      Assert.NotNull(found.TranslationStart);
      Assert.False(found.IsTranslated);
   }

   [Fact]
   public void MarkTranslationSucceeded_SetsEndTimeAndTranslatedText()
   {
      // Arrange
      PhraseInQueue phrase = new() { Key = "succeed.me", Phrase = "Success" };
      _queue.Enqueue(phrase);

      // Act
      bool result = _queue.MarkTranslationSucceeded("succeed.me", "Úspěch");

      // Assert
      Assert.True(result);
      PhraseInQueue? found = _queue.FindByKey("succeed.me");
      Assert.NotNull(found);
      Assert.NotNull(found.TranslationEnds);
      Assert.True(found.IsTranslated);
      Assert.Equal("Úspěch", found.TranslatedText);
   }

   [Fact]
   public void MarkTranslationFailed_SetsEndTimeAndClearsTranslatedText()
   {
      // Arrange
      PhraseInQueue phrase = new()
      {
         Key = "fail.me",
         Phrase = "Failure",
         TranslatedText = "Old translation",
         IsTranslated = true
      };
      _queue.Enqueue(phrase);

      // Act
      bool result = _queue.MarkTranslationFailed("fail.me");

      // Assert
      Assert.True(result);
      PhraseInQueue? found = _queue.FindByKey("fail.me");
      Assert.NotNull(found);
      Assert.NotNull(found.TranslationEnds);
      Assert.False(found.IsTranslated);
      Assert.Null(found.TranslatedText);
   }

   [Fact]
   public void GetAll_ReturnsSnapshotOfQueue()
   {
      // Arrange
      _queue.EnqueueRange(
      [
         new() { Key = "key1", Phrase = "Phrase 1" },
         new() { Key = "key2", Phrase = "Phrase 2" }
      ]);

      // Act
      List<PhraseInQueue> snapshot = _queue.GetAll();

      // Assert
      Assert.Equal(2, snapshot.Count);

      // Verify it's a snapshot (modifying it doesn't affect queue)
      snapshot.Clear();
      Assert.Equal(2, _queue.Count);
   }
}
