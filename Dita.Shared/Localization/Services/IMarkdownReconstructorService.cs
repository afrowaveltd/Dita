using Dita.Shared.Localization.Models;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Service for reconstructing Markdown documents by replacing translatable blocks
/// with their translated equivalents while preserving document structure.
/// </summary>
public interface IMarkdownReconstructorService
{
   /// <summary>
   /// Reconstructs a Markdown document by replacing original translatable blocks with translated content.
   /// </summary>
   /// <param name="originalMarkdown">The original Markdown content serving as the template.</param>
   /// <param name="translatableBlocks">The list of blocks containing original and translated text.</param>
   /// <returns>The reconstructed Markdown document with translated content.</returns>
   string Reconstruct(string originalMarkdown, List<MarkdownTranslatableBlock> translatableBlocks);
}
