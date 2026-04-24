using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Thread-safe translation queue interface that serves as the source of truth for translation work.
/// Manages phrases from multiple sources and tracks their translation lifecycle.
/// </summary>
public interface ITranslationQueue
{
   /// <summary>
   /// Gets a snapshot of all phrases currently in the queue.
   /// </summary>
   List<PhraseInQueue> GetAll();

   /// <summary>
   /// Gets all phrases that need to be added to translation dictionaries.
   /// </summary>
   List<PhraseInQueue> GetPendingAdditions();

   /// <summary>
   /// Gets all phrases that need to be removed from translation dictionaries.
   /// </summary>
   List<PhraseInQueue> GetPendingRemovals();

   /// <summary>
   /// Gets all phrases that need to be updated in translation dictionaries.
   /// </summary>
   List<PhraseInQueue> GetPendingUpdates();

   /// <summary>
   /// Gets all phrases that have not yet been translated.
   /// </summary>
   List<PhraseInQueue> GetUntranslated();

   /// <summary>
   /// Gets all phrases that have been successfully translated.
   /// </summary>
   List<PhraseInQueue> GetTranslated();

   /// <summary>
   /// Adds a new phrase to the queue for translation.
   /// </summary>
   /// <param name="phrase">The phrase to add.</param>
   void Enqueue(PhraseInQueue phrase);

   /// <summary>
   /// Adds multiple phrases to the queue in a single operation.
   /// </summary>
   /// <param name="phrases">The collection of phrases to add.</param>
   void EnqueueRange(IEnumerable<PhraseInQueue> phrases);

   /// <summary>
   /// Removes a phrase from the queue by its key.
   /// </summary>
   /// <param name="key">The key identifier of the phrase to remove.</param>
   /// <returns><see langword="true"/> if the phrase was found and removed; otherwise <see langword="false"/>.</returns>
   bool Remove(string key);

   /// <summary>
   /// Removes all phrases from the queue.
   /// </summary>
   void Clear();

   /// <summary>
   /// Finds a phrase in the queue by its key.
   /// </summary>
   /// <param name="key">The key identifier to search for.</param>
   /// <returns>The matching phrase, or <see langword="null"/> if not found.</returns>
   PhraseInQueue? FindByKey(string key);

   /// <summary>
   /// Updates an existing phrase in the queue with new values.
   /// </summary>
   /// <param name="phrase">The phrase containing updated values.</param>
   /// <returns><see langword="true"/> if the phrase was found and updated; otherwise <see langword="false"/>.</returns>
   bool Update(PhraseInQueue phrase);

   /// <summary>
   /// Marks the translation as started for a specific phrase.
   /// </summary>
   /// <param name="key">The key identifier of the phrase.</param>
   /// <returns><see langword="true"/> if the phrase was found and updated; otherwise <see langword="false"/>.</returns>
   bool MarkTranslationStarted(string key);

   /// <summary>
   /// Marks the translation as successfully completed for a specific phrase.
   /// </summary>
   /// <param name="key">The key identifier of the phrase.</param>
   /// <param name="translatedText">The translated text result.</param>
   /// <returns><see langword="true"/> if the phrase was found and updated; otherwise <see langword="false"/>.</returns>
   bool MarkTranslationSucceeded(string key, string translatedText);

   /// <summary>
   /// Marks the translation as failed for a specific phrase.
   /// </summary>
   /// <param name="key">The key identifier of the phrase.</param>
   /// <returns><see langword="true"/> if the phrase was found and updated; otherwise <see langword="false"/>.</returns>
   bool MarkTranslationFailed(string key);

   /// <summary>
   /// Gets the total number of phrases currently in the queue.
   /// </summary>
   int Count { get; }
}
