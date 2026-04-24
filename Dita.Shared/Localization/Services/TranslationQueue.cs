using Dita.Shared.Localization.Enums;
using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Thread-safe translation queue that manages phrases from multiple sources and tracks their translation lifecycle.
/// Serves as the single source of truth for the translation worker service.
/// </summary>
public class TranslationQueue : ITranslationQueue
{
   private readonly object _lock = new();
   private readonly List<PhraseInQueue> _queue = [];

   /// <summary>Gets the total number of phrases currently in the queue.</summary>
   public int Count
   {
      get
      {
         lock(_lock)
         {
            return _queue.Count;
         }
      }
   }

   /// <summary>Gets a snapshot of all phrases currently in the queue.</summary>
   public List<PhraseInQueue> GetAll()
   {
      lock(_lock)
      {
         return [.. _queue];
      }
   }

   /// <summary>Gets all phrases that need to be added to translation dictionaries.</summary>
   public List<PhraseInQueue> GetPendingAdditions()
   {
      lock(_lock)
      {
         return [.. _queue.Where(p => p.ChangeRequired == PhraseChange.Added)];
      }
   }

   /// <summary>Gets all phrases that need to be removed from translation dictionaries.</summary>
   public List<PhraseInQueue> GetPendingRemovals()
   {
      lock(_lock)
      {
         return [.. _queue.Where(p => p.ChangeRequired == PhraseChange.Removed)];
      }
   }

   /// <summary>Gets all phrases that need to be updated in translation dictionaries.</summary>
   public List<PhraseInQueue> GetPendingUpdates()
   {
      lock(_lock)
      {
         return [.. _queue.Where(p => p.ChangeRequired == PhraseChange.Updated)];
      }
   }

   /// <summary>Gets all phrases that have not yet been translated.</summary>
   public List<PhraseInQueue> GetUntranslated()
   {
      lock(_lock)
      {
         return [.. _queue.Where(p => !p.IsTranslated)];
      }
   }

   /// <summary>Gets all phrases that have been successfully translated.</summary>
   public List<PhraseInQueue> GetTranslated()
   {
      lock(_lock)
      {
         return [.. _queue.Where(p => p.IsTranslated)];
      }
   }

   /// <summary>Adds a new phrase to the queue for translation.</summary>
   /// <param name="phrase">The phrase to add.</param>
   public void Enqueue(PhraseInQueue phrase)
   {
      ArgumentNullException.ThrowIfNull(phrase);

      lock(_lock)
      {
         _queue.Add(phrase);
      }
   }

   /// <summary>Adds multiple phrases to the queue in a single operation.</summary>
   /// <param name="phrases">The collection of phrases to add.</param>
   public void EnqueueRange(IEnumerable<PhraseInQueue> phrases)
   {
      ArgumentNullException.ThrowIfNull(phrases);

      lock(_lock)
      {
         _queue.AddRange(phrases);
      }
   }

   /// <summary>Removes a phrase from the queue by its key.</summary>
   /// <param name="key">The key identifier of the phrase to remove.</param>
   /// <returns><see langword="true"/> if the phrase was found and removed; otherwise <see langword="false"/>.</returns>
   public bool Remove(string key)
   {
      if(string.IsNullOrWhiteSpace(key))
      {
         return false;
      }

      lock(_lock)
      {
         PhraseInQueue? existing = _queue.FirstOrDefault(p => p.Key == key);
         if(existing != null)
         {
            _queue.Remove(existing);
            return true;
         }
         return false;
      }
   }

   /// <summary>Removes all phrases from the queue.</summary>
   public void Clear()
   {
      lock(_lock)
      {
         _queue.Clear();
      }
   }

   /// <summary>Finds a phrase in the queue by its key.</summary>
   /// <param name="key">The key identifier to search for.</param>
   /// <returns>The matching phrase, or <see langword="null"/> if not found.</returns>
   public PhraseInQueue? FindByKey(string key)
   {
      if(string.IsNullOrWhiteSpace(key))
      {
         return null;
      }

      lock(_lock)
      {
         return _queue.FirstOrDefault(p => p.Key == key);
      }
   }

   /// <summary>Updates an existing phrase in the queue with new values.</summary>
   /// <param name="phrase">The phrase containing updated values.</param>
   /// <returns><see langword="true"/> if the phrase was found and updated; otherwise <see langword="false"/>.</returns>
   public bool Update(PhraseInQueue phrase)
   {
      ArgumentNullException.ThrowIfNull(phrase);

      lock(_lock)
      {
         PhraseInQueue? existing = _queue.FirstOrDefault(p => p.Key == phrase.Key);
         if(existing != null)
         {
            existing.Phrase = phrase.Phrase;
            existing.SourceLanguage = phrase.SourceLanguage;
            existing.TargetLanguage = phrase.TargetLanguage;
            existing.ChangeRequired = phrase.ChangeRequired;
            existing.TranslationStart = phrase.TranslationStart;
            existing.TranslationEnds = phrase.TranslationEnds;
            existing.IsTranslated = phrase.IsTranslated;
            existing.TranslatedText = phrase.TranslatedText;
            existing.Target = phrase.Target;
            return true;
         }
         return false;
      }
   }

   /// <summary>Marks the translation as started for a specific phrase.</summary>
   /// <param name="key">The key identifier of the phrase.</param>
   /// <returns><see langword="true"/> if the phrase was found and updated; otherwise <see langword="false"/>.</returns>
   public bool MarkTranslationStarted(string key)
   {
      if(string.IsNullOrWhiteSpace(key))
      {
         return false;
      }

      lock(_lock)
      {
         PhraseInQueue? existing = _queue.FirstOrDefault(p => p.Key == key);
         if(existing != null)
         {
            existing.TranslationStart = DateTime.UtcNow;
            existing.IsTranslated = false;
            return true;
         }
         return false;
      }
   }

   /// <summary>Marks the translation as successfully completed for a specific phrase.</summary>
   /// <param name="key">The key identifier of the phrase.</param>
   /// <param name="translatedText">The translated text result.</param>
   /// <returns><see langword="true"/> if the phrase was found and updated; otherwise <see langword="false"/>.</returns>
   public bool MarkTranslationSucceeded(string key, string translatedText)
   {
      if(string.IsNullOrWhiteSpace(key))
      {
         return false;
      }

      lock(_lock)
      {
         PhraseInQueue? existing = _queue.FirstOrDefault(p => p.Key == key);
         if(existing != null)
         {
            existing.TranslationEnds = DateTime.UtcNow;
            existing.IsTranslated = true;
            existing.TranslatedText = translatedText;
            return true;
         }
         return false;
      }
   }

   /// <summary>Marks the translation as failed for a specific phrase.</summary>
   /// <param name="key">The key identifier of the phrase.</param>
   /// <returns><see langword="true"/> if the phrase was found and updated; otherwise <see langword="false"/>.</returns>
   public bool MarkTranslationFailed(string key)
   {
      if(string.IsNullOrWhiteSpace(key))
      {
         return false;
      }

      lock(_lock)
      {
         PhraseInQueue? existing = _queue.FirstOrDefault(p => p.Key == key);
         if(existing != null)
         {
            existing.TranslationEnds = DateTime.UtcNow;
            existing.IsTranslated = false;
            existing.TranslatedText = null;
            return true;
         }
         return false;
      }
   }
}
