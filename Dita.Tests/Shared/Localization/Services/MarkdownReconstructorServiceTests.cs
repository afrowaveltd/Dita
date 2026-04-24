using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dita.Tests.Shared.Localization.Services;

/// <summary>
/// Tests for the <see cref="MarkdownReconstructorService"/> class.
/// </summary>
public class MarkdownReconstructorServiceTests
{
   private readonly IMarkdownReconstructorService _reconstructor;

   public MarkdownReconstructorServiceTests()
   {
      _reconstructor = new MarkdownReconstructorService(NullLogger<MarkdownReconstructorService>.Instance);
   }

   [Fact]
   public void Reconstruct_WithTranslatedParagraph_ReplacesContent()
   {
      // Arrange
      string original = "This is a paragraph.";
      List<MarkdownTranslatableBlock> blocks =
      [
         new()
         {
            StartLine = 0,
            EndLine = 0,
            BlockType = "Paragraph",
            OriginalText = "This is a paragraph.",
            TranslatedText = "Toto je odstavec.",
            IsTranslated = true
         }
      ];

      // Act
      string result = _reconstructor.Reconstruct(original, blocks);

      // Assert
      Assert.Equal("Toto je odstavec.", result);
   }

   [Fact]
   public void Reconstruct_WithTranslatedHeading_PreservesATXStyle()
   {
      // Arrange
      string original = "## Main Title";
      List<MarkdownTranslatableBlock> blocks =
      [
         new()
         {
            StartLine = 0,
            EndLine = 0,
            BlockType = "Heading",
            OriginalText = "Main Title",
            TranslatedText = "Hlavní Název",
            IsTranslated = true,
            Metadata = new Dictionary<string, object>
            {
               ["Level"] = 2,
               ["HeaderChar"] = '#'
            }
         }
      ];

      // Act
      string result = _reconstructor.Reconstruct(original, blocks);

      // Assert
      Assert.Equal("## Hlavní Název", result);
   }

   [Fact]
   public void Reconstruct_WithUntranslatedBlock_KeepsOriginal()
   {
      // Arrange
      string original = "Original text.";
      List<MarkdownTranslatableBlock> blocks =
      [
         new()
         {
            StartLine = 0,
            EndLine = 0,
            BlockType = "Paragraph",
            OriginalText = "Original text.",
            TranslatedText = null,
            IsTranslated = false
         }
      ];

      // Act
      string result = _reconstructor.Reconstruct(original, blocks);

      // Assert
      Assert.Equal("Original text.", result);
   }

   [Fact]
   public void Reconstruct_WithCodeBlockBetweenParagraphs_PreservesCodeBlock()
   {
      // Arrange
      string original = """
         First paragraph.

         ```csharp
         var code = "preserved";
         ```

         Second paragraph.
         """;

      List<MarkdownTranslatableBlock> blocks =
      [
         new()
         {
            StartLine = 0,
            EndLine = 0,
            BlockType = "Paragraph",
            OriginalText = "First paragraph.",
            TranslatedText = "První odstavec.",
            IsTranslated = true
         },
         new()
         {
            StartLine = 6,
            EndLine = 6,
            BlockType = "Paragraph",
            OriginalText = "Second paragraph.",
            TranslatedText = "Druhý odstavec.",
            IsTranslated = true
         }
      ];

      // Act
      string result = _reconstructor.Reconstruct(original, blocks);

      // Assert
      Assert.Contains("První odstavec.", result);
      Assert.Contains("```csharp", result);
      Assert.Contains("var code = \"preserved\";", result);
      Assert.Contains("Druhý odstavec.", result);
   }

   [Fact]
   public void Reconstruct_WithMultilineBlock_ReplacesEntireBlockRange()
   {
      // Arrange
      string original = """
         Line 1
         Line 2
         Line 3
         """;

      List<MarkdownTranslatableBlock> blocks =
      [
         new()
         {
            StartLine = 0,
            EndLine = 2,
            BlockType = "Paragraph",
            OriginalText = "Line 1 Line 2 Line 3",
            TranslatedText = "Přeložený text",
            IsTranslated = true
         }
      ];

      // Act
      string result = _reconstructor.Reconstruct(original, blocks);

      // Assert
      Assert.Equal("Přeložený text", result);
   }

   [Fact]
   public void Reconstruct_WithEmptyBlocks_ReturnsOriginal()
   {
      // Arrange
      string original = "Unchanged content.";
      List<MarkdownTranslatableBlock> blocks = [];

      // Act
      string result = _reconstructor.Reconstruct(original, blocks);

      // Assert
      Assert.Equal("Unchanged content.", result);
   }

   [Fact]
   public void Reconstruct_WithMixedTranslatedAndUntranslated_ReplacesOnlyTranslated()
   {
      // Arrange
      string original = """
         Translated paragraph.

         Untranslated paragraph.
         """;

      List<MarkdownTranslatableBlock> blocks =
      [
         new()
         {
            StartLine = 0,
            EndLine = 0,
            BlockType = "Paragraph",
            OriginalText = "Translated paragraph.",
            TranslatedText = "Přeložený odstavec.",
            IsTranslated = true
         },
         new()
         {
            StartLine = 2,
            EndLine = 2,
            BlockType = "Paragraph",
            OriginalText = "Untranslated paragraph.",
            TranslatedText = null,
            IsTranslated = false
         }
      ];

      // Act
      string result = _reconstructor.Reconstruct(original, blocks);

      // Assert
      Assert.Contains("Přeložený odstavec.", result);
      Assert.Contains("Untranslated paragraph.", result);
   }

   [Fact]
   public void Reconstruct_WithSetextHeading_PreservesUnderlineStyle()
   {
      // Arrange
      string original = """
         Main Title
         ==========
         """;

      List<MarkdownTranslatableBlock> blocks =
      [
         new()
         {
            StartLine = 0,
            EndLine = 0,
            BlockType = "Heading",
            OriginalText = "Main Title",
            TranslatedText = "Hlavní Název",
            IsTranslated = true,
            Metadata = new Dictionary<string, object>
            {
               ["Level"] = 1,
               ["HeaderChar"] = '='
            }
         }
      ];

      // Act
      string result = _reconstructor.Reconstruct(original, blocks);

      // Assert
      Assert.Contains("Hlavní Název", result);
      Assert.Contains("==========", result);
   }
}
