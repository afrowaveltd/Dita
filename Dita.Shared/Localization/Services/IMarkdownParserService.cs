using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Service for parsing Markdown documents and extracting translatable content blocks
/// while preserving non-translatable elements like code, quotes, and HTML.
/// </summary>
public interface IMarkdownParserService
{
   /// <summary>
   /// Parses a Markdown document and extracts all translatable blocks.
   /// </summary>
   /// <param name="markdownContent">The raw Markdown content to parse.</param>
   /// <returns>A list of translatable blocks with unique GUID keys and metadata.</returns>
   List<MarkdownTranslatableBlock> ExtractTranslatableBlocks(string markdownContent);
}
