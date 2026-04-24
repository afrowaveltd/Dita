using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dita.Tests.Shared.Localization.Services;

/// <summary>
/// Tests for the <see cref="MarkdownParserService"/> class.
/// </summary>
public class MarkdownParserServiceTests
{
   private readonly IMarkdownParserService _parserService;

   public MarkdownParserServiceTests()
   {
      _parserService = new MarkdownParserService(NullLogger<MarkdownParserService>.Instance);
   }

   [Fact]
   public void ExtractTranslatableBlocks_WithSimpleParagraph_ExtractsCorrectly()
   {
      // Arrange
      string markdown = "This is a simple paragraph.";

      // Act
      List<MarkdownTranslatableBlock> blocks = _parserService.ExtractTranslatableBlocks(markdown);

      // Assert
      Assert.Single(blocks);
      Assert.Equal("This is a simple paragraph.", blocks[0].OriginalText);
      Assert.Equal("Paragraph", blocks[0].BlockType);
   }

   [Fact]
   public void ExtractTranslatableBlocks_WithHeading_ExtractsCorrectly()
   {
      // Arrange
      string markdown = "# Main Title";

      // Act
      List<MarkdownTranslatableBlock> blocks = _parserService.ExtractTranslatableBlocks(markdown);

      // Assert
      Assert.Single(blocks);
      Assert.Equal("Main Title", blocks[0].OriginalText);
      Assert.Equal("Heading", blocks[0].BlockType);
      Assert.Equal(1, blocks[0].Metadata["Level"]);
   }

   [Fact]
   public void ExtractTranslatableBlocks_WithCodeBlock_SkipsCodeBlock()
   {
      // Arrange
      string markdown = """
         This is text.

         ```csharp
         var code = "should be skipped";
         ```

         More text.
         """;

      // Act
      List<MarkdownTranslatableBlock> blocks = _parserService.ExtractTranslatableBlocks(markdown);

      // Assert
      Assert.Equal(2, blocks.Count);
      Assert.Contains(blocks, b => b.OriginalText == "This is text.");
      Assert.Contains(blocks, b => b.OriginalText == "More text.");
      Assert.DoesNotContain(blocks, b => b.OriginalText.Contains("code"));
   }

   [Fact]
   public void ExtractTranslatableBlocks_WithEmphasis_PreservesMarkup()
   {
      // Arrange
      string markdown = "This is *italic* and **bold** text.";

      // Act
      List<MarkdownTranslatableBlock> blocks = _parserService.ExtractTranslatableBlocks(markdown);

      // Assert
      Assert.Single(blocks);
      Assert.Contains("*italic*", blocks[0].OriginalText);
      Assert.Contains("**bold**", blocks[0].OriginalText);
   }

   [Fact]
   public void ExtractTranslatableBlocks_WithInlineCode_SkipsInlineCode()
   {
      // Arrange
      string markdown = "Use the `Console.WriteLine()` method.";

      // Act
      List<MarkdownTranslatableBlock> blocks = _parserService.ExtractTranslatableBlocks(markdown);

      // Assert
      Assert.Single(blocks);
      // Inline code should be skipped, not included in translatable text
      Assert.DoesNotContain("`", blocks[0].OriginalText);
   }

   [Fact]
   public void ExtractTranslatableBlocks_WithList_ExtractsListItems()
   {
      // Arrange
      string markdown = """
         - Item one
         - Item two
         - Item three
         """;

      // Act
      List<MarkdownTranslatableBlock> blocks = _parserService.ExtractTranslatableBlocks(markdown);

      // Assert
      Assert.Equal(3, blocks.Count);
      Assert.All(blocks, b => Assert.Equal("Paragraph", b.BlockType));
   }

   [Fact]
   public void ExtractTranslatableBlocks_WithQuoteBlock_SkipsQuote()
   {
      // Arrange
      string markdown = """
         Normal text.

         > This is a quote
         > that should be skipped.

         More normal text.
         """;

      // Act
      List<MarkdownTranslatableBlock> blocks = _parserService.ExtractTranslatableBlocks(markdown);

      // Assert
      Assert.Equal(2, blocks.Count);
      Assert.Contains(blocks, b => b.OriginalText == "Normal text.");
      Assert.Contains(blocks, b => b.OriginalText == "More normal text.");
      Assert.DoesNotContain(blocks, b => b.OriginalText.Contains("quote"));
   }
}
