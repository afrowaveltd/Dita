# Markdown Translation

The `Dita.Shared.Localization.Services` namespace provides a three-stage Markdown translation pipeline: **parse → translate → reconstruct**. This document describes how each stage works and how they compose into the full translation flow.

## Pipeline overview

```
Source Markdown
      │
      ▼
┌─────────────────────┐
│ MarkdownParserService │  Extract translatable blocks
└─────────────────────┘
      │
      ▼  List<MarkdownTranslatableBlock>
┌──────────────────────────────┐
│ MarkdownTranslationService    │  Translate each block
│  (with LibreTranslateService) │  Validate structure
└──────────────────────────────┘
      │
      ▼  List<MarkdownTranslatableBlock> (translated)
┌──────────────────────────────┐
│ MarkdownReconstructorService │  Rebuild document
└──────────────────────────────┘
      │
      ▼  Translated Markdown
```

## Stage 1: Parsing

### MarkdownParserService

Uses **Markdig** to parse raw Markdown into an abstract syntax tree (AST) and extract translatable blocks.

### Block classification

| Block type | Translatable? | Reason |
|---|---|---|
| Headings (ATX `## ` and Setext) | ✅ | Heading text needs translation |
| Paragraphs | ✅ | Body text needs translation |
| List items (ordered and unordered) | ✅ | List item text needs translation |
| Fenced code blocks | ❌ | Code should never be translated |
| Indented code blocks | ❌ | Code should never be translated |
| Block quotes | ❌ | Typically citations, not original content |
| HTML blocks | ❌ | Markup should not be altered |
| Thematic breaks (`---`) | ❌ | No translatable content |

### Extraction output

Each `MarkdownTranslatableBlock` carries:

| Property | Type | Description |
|---|---|---|
| `Key` | `Guid` | Unique identifier |
| `OriginalText` | `string` | Raw text of the block |
| `StartLine` | `int` | 0-based line where the block begins |
| `EndLine` | `int` | 0-based line where the block ends |
| `BlockType` | `string` | "Heading", "Paragraph", "ListItem" |
| `Metadata` | `Dictionary<string, object>` | Extra info (heading level, list marker, etc.) |
| `IsTranslated` | `bool` | Whether translation succeeded |
| `TranslatedText` | `string?` | Translated text (null if not yet translated) |

### Inline content handling

Within translatable blocks, inline elements are preserved:

- **Emphasis**: `**bold**`, `*italic*`, `~~strikethrough~~` — markers kept, inner text translated
- **Inline code**: `` `code` `` — kept as-is, not translated
- **Links**: `[link text](url)` — link text translated, URL preserved
- **Images**: `![alt text](url)` — alt text translated, URL preserved
- **HTML tags**: `<strong>`, `<em>`, `<code>` — kept in output

## Stage 2: Translation

### MarkdownTranslationService

Orchestrates per-block translation with structural validation.

### Translation flow per block

1. **Unwrap symmetric wrappers** — remove `**`, `*`, `~~`, `` ` `` markers from both ends
2. **Translate inner text** — call LibreTranslate via `ILibreTranslateService`
3. **Re-wrap** — restore the original markers around the translated text
4. **Validate inline tag structure** — verify HTML tags, Markdown formatting tokens, and links match between original and translated blocks
5. **Accept or reject** — if validation passes, mark `IsTranslated = true`; otherwise, keep original text

### Symmetric wrapper handling

The service detects and unwraps paired formatting markers before translation:

```text
**bold text**    →  translate "bold text"  →  **tučný text**
*italic text*    →  translate "italic text" →  *kurzíva text*
~~strike text~~  →  translate "strike text" →  ~~přeškrtnutý text~~
`code`           →  NOT translated (inline code)
```

This prevents ML engines from corrupting Markdown syntax elements.

### Structural validation

After translating and reconstructing the entire document, the service verifies:

| Check | Method |
|---|---|
| Heading count matches source | Regex `^#{1,6}\s` count |
| List item count matches source | Regex `^[\-\*\+]\s` + `^\d+\.\s` count |
| Code block count matches source | Regex `` ^``` `` count / 2 |
| Blockquote count matches source | Regex `^>\s` count |
| HTML tag structure matches | Extract `<?…>` tags, compare arrays |
| Inline markers match | Count `**`, `*`, `~~`, `` ` ``, `[]()`, `![]()` occurrences |

If any check fails, the translation is rejected and the original Markdown is preserved.

### Fallback behaviour

- **Block-level failure**: individual failed blocks keep their original text; other blocks are still translated
- **Document-level failure**: if the overall structure doesn't match, the entire target file is not saved; it will be retried on the next pipeline run
- **Retry is automatic**: the `DocumentsTranslationService` in the scheduled pipeline tracks per-block failure status in `MarkdownTranslationMetadata`

## Stage 3: Reconstruction

### MarkdownReconstructorService

Line-by-line document reconstruction that places translated blocks back into the original document structure.

### Reconstruction strategy

1. Iterate through original lines
2. For each line, check if it falls within a `MarkdownTranslatableBlock.StartLine..EndLine` range
3. If the block was translated (`IsTranslated = true`):
   - **Headings**: preserve ATX prefix (`## `) and Setext underline style
   - **Paragraphs**: preserve leading whitespace
   - **List items**: preserve list markers (`- `, `* `, `+ `, `1. `)
4. If the block was not translated: keep the original text
5. Skip lines covered by multi-line blocks (already replaced by the block reconstruction)

### Heading reconstruction detail

ATX-style headings preserve their prefix level and optional closing sequence:

```markdown
## Original Heading ##    →    ## Přeložený Nadpis ##
```

Setext-style headings preserve their underline:

```markdown
Original Heading
-----------------    →    Přeložený Nadpis
                         -----------------
```

## Integration with the scheduled pipeline

The `DocumentsTranslationService` (in `ScheduledTranslationService`) uses these services as follows:

1. **Parse** each `{DefaultLanguage}.md` file via `IMarkdownParserService`
2. **Check metadata** — skip blocks already marked as translated
3. **Translate** untranslated blocks via `ILibreTranslateService` (with `TranslationRetryService` for resilience)
4. **Validate** inline tags and document structure
5. **Reconstruct** via `IMarkdownReconstructorService`
6. **Save** the target file and update `MarkdownTranslationMetadata`
7. **Write hash** — SHA-256 of the source file for incremental change detection

This ensures that:
- Only changed or previously-failed blocks are re-translated
- Already-successful translations are never touched
- Document structure integrity is guaranteed before saving