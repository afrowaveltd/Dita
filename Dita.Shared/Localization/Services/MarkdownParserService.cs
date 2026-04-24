using Dita.Shared.Localization.Models;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Logging;

namespace Dita.Shared.Localization.Services;

/// <summary>
/// Parses Markdown documents using Markdig and extracts translatable content blocks.
/// Skips non-translatable elements: code blocks, inline code, fenced code, HTML, YAML front matter.
/// </summary>
/// <param name="logger">Logger for diagnostic output.</param>
public class MarkdownParserService(ILogger<MarkdownParserService> logger) : IMarkdownParserService
{
   private readonly ILogger<MarkdownParserService> _logger = logger;

   /// <summary>
   /// Parses a Markdown document and extracts all translatable blocks.
   /// </summary>
   /// <param name="markdownContent">The raw Markdown content to parse.</param>
   /// <returns>A list of translatable blocks with unique GUID keys and metadata.</returns>
   public List<MarkdownTranslatableBlock> ExtractTranslatableBlocks(string markdownContent)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(markdownContent);

      List<MarkdownTranslatableBlock> blocks = [];

      try
      {
         MarkdownDocument document = Markdown.Parse(markdownContent, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

         foreach(Block block in document)
         {
            ProcessBlock(block, blocks);
         }

         _logger.LogDebug("Extracted {Count} translatable blocks from Markdown content.", blocks.Count);
      }
      catch(Exception ex)
      {
         _logger.LogError(ex, "Failed to parse Markdown content.");
         throw;
      }

      return blocks;
   }

   private void ProcessBlock(Block block, List<MarkdownTranslatableBlock> blocks)
   {
      switch(block)
      {
         case HeadingBlock heading:
            ExtractFromHeading(heading, blocks);
            break;

         case ParagraphBlock paragraph:
            ExtractFromParagraph(paragraph, blocks);
            break;

         case ListBlock list:
            ExtractFromList(list, blocks);
            break;

         case QuoteBlock:
            // Skip quote blocks – not safe for translation
            _logger.LogTrace("Skipping QuoteBlock at line {Line}.", block.Line);
            break;

         case CodeBlock:
            // Skip code blocks – never translate
            _logger.LogTrace("Skipping CodeBlock at line {Line}.", block.Line);
            break;

         case HtmlBlock:
            // Skip raw HTML – not safe for translation
            _logger.LogTrace("Skipping HtmlBlock at line {Line}.", block.Line);
            break;

         case ThematicBreakBlock:
            // Skip horizontal rules – no translatable content
            break;

         case ContainerBlock container:
            // Recursively process nested blocks (e.g., list items, blockquotes)
            foreach(Block child in container)
            {
               ProcessBlock(child, blocks);
            }
            break;

         default:
            _logger.LogTrace("Unhandled block type {BlockType} at line {Line}.", block.GetType().Name, block.Line);
            break;
      }
   }

   private void ExtractFromHeading(HeadingBlock heading, List<MarkdownTranslatableBlock> blocks)
   {
      string text = ExtractTextFromInlines(heading.Inline);

      if(string.IsNullOrWhiteSpace(text))
      {
         return;
      }

      blocks.Add(new MarkdownTranslatableBlock
      {
         OriginalText = text,
         StartLine = heading.Line,
         EndLine = heading.Span.End,
         BlockType = "Heading",
         Metadata = new Dictionary<string, object>
         {
            ["Level"] = heading.Level,
            ["HeaderChar"] = heading.HeaderChar
         }
      });

      _logger.LogTrace("Extracted Heading (level {Level}): \"{Text}\"", heading.Level, text);
   }

   private void ExtractFromParagraph(ParagraphBlock paragraph, List<MarkdownTranslatableBlock> blocks)
   {
      string text = ExtractTextFromInlines(paragraph.Inline);

      if(string.IsNullOrWhiteSpace(text))
      {
         return;
      }

      blocks.Add(new MarkdownTranslatableBlock
      {
         OriginalText = text,
         StartLine = paragraph.Line,
         EndLine = paragraph.Span.End,
         BlockType = "Paragraph"
      });

      _logger.LogTrace("Extracted Paragraph: \"{Text}\"", text);
   }

   private void ExtractFromList(ListBlock list, List<MarkdownTranslatableBlock> blocks)
   {
      foreach(Block item in list)
      {
         if(item is ListItemBlock listItem)
         {
            foreach(Block child in listItem)
            {
               ProcessBlock(child, blocks);
            }
         }
      }
   }

   private string ExtractTextFromInlines(ContainerInline? inline)
   {
      if(inline == null)
      {
         return string.Empty;
      }

      List<string> parts = [];

      foreach(Inline child in inline)
      {
         switch(child)
         {
            case LiteralInline literal:
               parts.Add(literal.Content.ToString());
               break;

            case EmphasisInline emphasis:
               // Recursively extract text but preserve emphasis markers
               string innerText = ExtractTextFromInlines(emphasis);
               if(!string.IsNullOrWhiteSpace(innerText))
               {
                  string marker = emphasis.DelimiterCount == 2 ? "**" : "*";
                  parts.Add($"{marker}{innerText}{marker}");
               }
               break;

            case LineBreakInline:
               parts.Add(" ");
               break;

            case CodeInline:
               // Skip inline code – not safe for translation
               _logger.LogTrace("Skipping inline code in text extraction.");
               break;

            case LinkInline link:
               // Extract link text but preserve link structure
               string linkText = ExtractTextFromInlines(link);
               if(!string.IsNullOrWhiteSpace(linkText))
               {
                  parts.Add($"[{linkText}]({link.Url})");
               }
               break;

            case HtmlInline:
               // Skip inline HTML
               _logger.LogTrace("Skipping inline HTML in text extraction.");
               break;

            case ContainerInline container:
               parts.Add(ExtractTextFromInlines(container));
               break;

            default:
               _logger.LogTrace("Unhandled inline type {InlineType}.", child.GetType().Name);
               break;
         }
      }

      return string.Join(string.Empty, parts).Trim();
   }
}
