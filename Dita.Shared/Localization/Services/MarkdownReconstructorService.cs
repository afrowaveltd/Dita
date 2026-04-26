using Dita.Shared.Localization.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Reconstructs Markdown documents by replacing translatable blocks with translated content
/// while preserving structure, formatting, and non-translatable elements.
/// </summary>
/// <param name="logger">Logger for diagnostic output.</param>
public class MarkdownReconstructorService(ILogger<MarkdownReconstructorService> logger) : IMarkdownReconstructorService
{
   private readonly ILogger<MarkdownReconstructorService> _logger = logger;

   /// <summary>
   /// Reconstructs a Markdown document by replacing original translatable blocks with translated content.
   /// Uses a line-by-line replacement strategy based on block position metadata.
   /// </summary>
   /// <param name="originalMarkdown">The original Markdown content serving as the template.</param>
   /// <param name="translatableBlocks">The list of blocks containing original and translated text.</param>
   /// <returns>The reconstructed Markdown document with translated content.</returns>
   public string Reconstruct(string originalMarkdown, List<MarkdownTranslatableBlock> translatableBlocks)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(originalMarkdown);
      ArgumentNullException.ThrowIfNull(translatableBlocks);

      try
      {
         string[] lines = originalMarkdown.Split(["\r\n", "\n"], StringSplitOptions.None);
         StringBuilder result = new(originalMarkdown.Length);

         // Build a lookup for translated blocks by line range
         Dictionary<int, MarkdownTranslatableBlock> blocksByStartLine = translatableBlocks
            .Where(b => b.IsTranslated && !string.IsNullOrWhiteSpace(b.TranslatedText))
            .ToDictionary(b => b.StartLine, b => b);

         int skipUntilLine = -1;

         for(int i = 0; i < lines.Length; i++)
         {
            // If we're inside a multi-line block that was replaced, skip until block ends
            if(i <= skipUntilLine)
            {
               continue;
            }

            // Check if this line starts a translatable block
            if(blocksByStartLine.TryGetValue(i, out MarkdownTranslatableBlock? block))
            {
               // Replace the entire block range with translated content
               string translatedLine = ReconstructBlock(block, lines[i]);
               result.AppendLine(translatedLine);

               // Skip original lines that were part of this block
               skipUntilLine = block.EndLine;

               _logger.LogTrace("Replaced block at lines {Start}-{End} with translated content.", block.StartLine, block.EndLine);
            }
            else
            {
               // Preserve non-translatable line as-is
               result.AppendLine(lines[i]);
            }
         }

         _logger.LogDebug("Reconstructed Markdown document with {Count} translated blocks.", blocksByStartLine.Count);

         return result.ToString().TrimEnd();
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Failed to reconstruct Markdown document.");
         throw;
      }
   }

   private string ReconstructBlock(MarkdownTranslatableBlock block, string originalLine)
   {
      string translatedText = block.TranslatedText ?? block.OriginalText;

      return block.BlockType switch
      {
         "Heading" => ReconstructHeading(block, translatedText, originalLine),
         "Paragraph" => ReconstructParagraph(block, translatedText, originalLine),
         _ => translatedText
      };
   }

   private string ReconstructHeading(MarkdownTranslatableBlock block, string translatedText, string originalLine)
   {
      if(!block.Metadata.TryGetValue("Level", out object? levelObj) || levelObj is not int level)
      {
         _logger.LogWarning("Heading block missing Level metadata; using plain text.");
         return translatedText;
      }

      // Preserve original heading style (ATX: ##, Setext: underline)
      if(block.Metadata.TryGetValue("HeaderChar", out object? headerCharObj) && headerCharObj is char headerChar)
      {
         if(headerChar == '#')
         {
            int headingPrefixLength = originalLine.TakeWhile(ch => ch == '#').Count();
            int firstTextIndex = headingPrefixLength;

            while(firstTextIndex < originalLine.Length && char.IsWhiteSpace(originalLine[firstTextIndex]))
            {
               firstTextIndex++;
            }

            string prefix = originalLine[..firstTextIndex];
            string suffix = string.Empty;

            int trailingHashesStart = originalLine.Length;
            while(trailingHashesStart > firstTextIndex && originalLine[trailingHashesStart - 1] == '#')
            {
               trailingHashesStart--;
            }

            if(trailingHashesStart < originalLine.Length)
            {
               int suffixStart = trailingHashesStart;
               while(suffixStart > firstTextIndex && char.IsWhiteSpace(originalLine[suffixStart - 1]))
               {
                  suffixStart--;
               }

               suffix = originalLine[suffixStart..];
            }

            return $"{prefix}{translatedText}{suffix}";
         }

         if(headerChar is '=' or '-')
         {
            // Setext-style heading keeps underline on following line; this line contains only text.
            return translatedText;
         }
      }

      // Fallback: keep prefix level markers.
      return $"{new string('#', level)} {translatedText}";
   }

   private static string ReconstructParagraph(MarkdownTranslatableBlock block, string translatedText, string originalLine)
   {
      if(string.IsNullOrWhiteSpace(originalLine))
      {
         return translatedText;
      }

      int firstContentIndex = 0;
      while(firstContentIndex < originalLine.Length && char.IsWhiteSpace(originalLine[firstContentIndex]))
      {
         firstContentIndex++;
      }

      string leadingWhitespace = originalLine[..firstContentIndex];
      string remainder = originalLine[firstContentIndex..];

      if(remainder.StartsWith("- ", StringComparison.Ordinal) || remainder.StartsWith("* ", StringComparison.Ordinal) || remainder.StartsWith("+ ", StringComparison.Ordinal))
      {
         string marker = remainder[..2];
         return $"{leadingWhitespace}{marker}{translatedText}";
      }

      int orderedIndex = 0;
      while(orderedIndex < remainder.Length && char.IsDigit(remainder[orderedIndex]))
      {
         orderedIndex++;
      }

      if(orderedIndex > 0 && orderedIndex + 1 < remainder.Length && remainder[orderedIndex] == '.' && remainder[orderedIndex + 1] == ' ')
      {
         string marker = remainder[..(orderedIndex + 2)];
         return $"{leadingWhitespace}{marker}{translatedText}";
      }

      return $"{leadingWhitespace}{translatedText}";
   }
}
