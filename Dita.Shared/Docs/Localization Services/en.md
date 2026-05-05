# Localization Services

The `Dita.Shared.Localization.Services` namespace contains the core service layer for Dita's localization system. It provides JSON-based string localization, LibreTranslate API integration, Markdown translation, placeholder management, and a thread-safe translation queue.

## Service overview

| Service | Interface | Purpose |
|---|---|---|
| JsonStringLocalizer | `IStringLocalizer` | ASP.NET Core JSON-backed string localizer with write-through on miss |
| JsonStringLocalizerFactory | `IStringLocalizerFactory` | Factory for creating localizer instances |
| LanguageService | `ILanguageService` | Locale file I/O, language metadata, dictionary CRUD |
| LibreTranslateService | `ILibreTranslateService` | LibreTranslate API client with retry, throttling, validation |
| LibreTranslateHttpClientFactory | `ILibreTranslateHttpClientFactory` | Pre-configured HttpClient factory for LibreTranslate |
| LocalizeService | `ILocalizeService` | Writable dictionary-based localization (adds missing keys) |
| TranslateService | `ITranslateService` | Read-only dynamic translation (never writes to dictionaries) |
| MarkdownParserService | `IMarkdownParserService` | Markdig-based Markdown block extraction |
| MarkdownReconstructorService | `IMarkdownReconstructorService` | Translated block reassembly into Markdown |
| MarkdownTranslationService | `IMarkdownTranslationService` | Full Markdown translation orchestrator |
| PlaceholderService | `IPlaceholderService` | Named placeholder management with translation-safe masking |
| TranslationQueue | `ITranslationQueue` | Thread-safe in-memory queue for translation work items |

## JsonStringLocalizer

ASP.NET Core `IStringLocalizer` implementation backed by JSON locale files (`Locales/{language}.json`). This is the primary localization API consumed by Razor pages and controllers.

### Resolution chain

1. **Distributed cache** — check `IDistributedCache` for a cached value
2. **Target locale file** — read `Locales/{culture}.json`
3. **Default locale file** — fall back to `Locales/{defaultLanguage}.json`
4. **Auto-add** — if key is missing, add `key=key` to the default dictionary
5. **Live translation** — translate via LibreTranslate and return

### Indexers

```csharp
// Simple key lookup
LocalizedString text = localizer["WelcomeMessage"];

// Positional formatting (backward compatible with ASP.NET Core)
LocalizedString text = localizer["Value is {0}", 42];

// Named placeholder formatting
LocalizedString text = localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
```

### Write-through behaviour

When a key is not found in any dictionary, the localizer:
1. Adds the key to the default dictionary as `key=key`
2. Translates it live via LibreTranslate
3. Returns the translated value

This ensures that the next scheduled translation pipeline run will pick up the key and translate it into all target languages.

## LanguageService

Manages language metadata and locale file I/O. Reads from `Jsons/languages.json` and `Locales/{code}.json`.

### Key operations

- **GetDictionaryAsync** — loads a locale JSON file as a `Dictionary<string, string>`
- **SaveDictionaryAsync** — writes a locale file (always sorted by key)
- **GetAllDictionariesAsync** — loads all locale files
- **CreateMissingLanguageFilesAsync** — creates empty locale files for supported languages
- **AddTranslationEntryAsync / RemoveTranslationEntryAsync / UpdateTranslationEntryAsync** — per-key CRUD
- **IsRtl** — checks if a language code is right-to-left
- **TranslationsPresented** — lists languages that have locale files

### Thread safety

All file mutations are serialized via `SemaphoreSlim`. Backup files (`.bak`) are created in the temp directory before overwriting.

## LibreTranslateService

Full LibreTranslate API client — the largest and most complex service in the system.

### Features

- **Text translation** — `TranslateTextAsync` with source/target language
- **File translation** — `TranslateFileAsync` (multipart upload)
- **Language detection** — `DetectLanguageAsync`
- **Available languages** — `GetAvailableLanguagesAsync` with 10-minute cache
- **Server latency** — `ServerLatency` for health checks

### Retry logic

- **Up to 10 retries** with exponential backoff (`2^n × 500ms`, capped)
- **Retryable status codes**: 429, 408, 5xx
- **Non-retryable**: other 4xx (immediate failure)
- **Intelligent validation**: if the server returns untranslated text (case-insensitive match), retries with lowercase to break potential server-side caches

### Adaptive throttling

AIMD-style rate limiter that adjusts the delay between translation requests:

- **On error**: doubles the throttle interval (up to `_maxIntervalMs`)
- **On 3 consecutive successes**: halves the interval (down to `_baseIntervalMs`)

This prevents overwhelming the LibreTranslate server while maintaining throughput under stable conditions.

### Language code normalization

Language codes are normalized via `CultureInfo` — e.g. `"cs-CZ"` → `"cs"`, `"zh-Hans"` → `"zh"`. The `AreLanguagesEquivalent` method compares `TwoLetterISOLanguageName` values, so `"en-US"` and `"en"` are treated as equivalent.

## LocalizeService vs TranslateService

Two services provide distinct localization strategies:

| Aspect | LocalizeService | TranslateService |
|---|---|---|
| Interface | `ILocalizeService` | `ITranslateService` |
| Dictionary reads | Yes | Yes |
| Dictionary writes | **Yes** (auto-adds missing keys) | **No** |
| LibreTranslate fallback | Yes | Yes |
| Use case | Application UI strings | Dynamic/runtime text |
| Write behaviour | Write-through on cache miss | Read-only |

### LocalizeService chain

1. Search target language dictionary
2. Fall back to default language dictionary
3. If not found, **add `key=key` to default dictionary**
4. Return with `TextResolutionSource` indicating where the value came from

### TranslateService chain

1. Search target language dictionary
2. If source = target, return original text
3. Translate via LibreTranslate (with `TranslationRetryService` + placeholder masking)
4. Return without persisting

## MarkdownParserService

Uses **Markdig** to parse Markdown into an AST and extract translatable blocks.

### Extracted block types

| Block type | Extracted? |
|---|---|
| Headings (ATX `##` and Setext) | ✅ |
| Paragraphs | ✅ |
| List items (ordered and unordered) | ✅ |
| Code blocks (fenced, indented) | ❌ |
| Block quotes | ❌ |
| HTML blocks | ❌ |
| Thematic breaks | ❌ |

### Inline content preservation

Within translatable blocks, the parser preserves:
- Emphasis markers (`**`, `*`, `~~`)
- Link structure (`[text](url)`)
- Image syntax (`![alt](url)`)
- Inline HTML tags
- Inline code

## MarkdownReconstructorService

Line-by-line reconstruction of translated Markdown documents. Replaces content at `StartLine` positions from the parsed blocks.

### Supported block types

- **Headings**: preserves ATX prefix/suffix (`## `) and Setext underline style
- **Paragraphs**: preserves leading whitespace and list markers (`- `, `* `, `+ `, `1. `)

## MarkdownTranslationService

Orchestrates the full Markdown translation pipeline:

1. **Parse** → extract translatable blocks via `IMarkdownParserService`
2. **Translate** → each block per target language sequentially
3. **Validate** → structural integrity check
4. **Reconstruct** → rebuild via `IMarkdownReconstructorService`

### Symmetric wrapper handling

Markdown formatting (`**bold**`, `*italic*`, `~~strike~~`, `` `code` ``) is unwrapped before translation and re-wrapped after. This prevents the translation engine from corrupting Markdown syntax.

### Structural validation

After reconstruction, the service verifies that the translated document has:
- The same heading count
- The same list item count
- The same code block count
- The same blockquote count
- Matching inline tag structure (HTML tags, bold/italic markers, links)

If validation fails, the original text is preserved.

## PlaceholderService

Manages named placeholders (`{name}` syntax) in localization strings.

### Translation-safe token masking

Placeholders are replaced with Unicode bracket tokens `⟦N⟧` (U+27E6/U+27E7) before translation. Translation engines typically leave these untouched because they are mathematical delimiters, not common-language punctuation.

```csharp
var (preparedText, restore) = placeholderService.PrepareForTranslation(
    "Hello {userName}, welcome to {appName}!");
// preparedText → "Hello ⟦0⟧, welcome to ⟦1⟧!"
var translated = await TranslateAsync(preparedText, "en", "cs");
// translated → "Ahoj ⟦0⟧, vítej v ⟦1⟧!"
var final = restore(translated);
// final → "Ahoj {userName}, vítej v {appName}!"
```

### Legacy token support

The service also recognizes legacy `___PH_N___` tokens and tolerates spaces that machine translation engines may insert around them.

### Placeholder storage

Default placeholder values can be stored in `Locales/placeholders.json`:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Runtime values override stored defaults.

## TranslationQueue

Thread-safe in-memory queue serving as the single source of truth for translation work items.

### Lifecycle tracking

```
Enqueue → MarkTranslationStarted → MarkTranslationSucceeded / MarkTranslationFailed
```

### Change types

| `PhraseChange` | Meaning |
|---|---|
| `NoChange` | No modification needed |
| `Added` | New key to translate |
| `Updated` | Existing key with new value |
| `Removed` | Key to delete |

### Query methods

- `GetPendingAdditions` — items awaiting translation (Added)
- `GetPendingRemovals` — items marked for deletion (Removed)
- `GetPendingUpdates` — items with new values (Updated)
- `GetUntranslated` — items not yet translated
- `GetTranslated` — items successfully translated

All query methods return **snapshot copies** (new lists) to prevent external mutation.