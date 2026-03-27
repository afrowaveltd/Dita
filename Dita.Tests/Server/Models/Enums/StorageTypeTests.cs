using Dita.Server.Models.Enums;

namespace Dita.Tests.Server.Models.Enums;

/// <summary>
/// Unit tests for the <see cref="StorageType"/> enum.
/// </summary>
public class StorageTypeTests
{
   [Fact]
   public void WhenAjisFilesThenEnumIsDefined()
   {
      // Arrange & Act
      var storageType = StorageType.AjisFiles;

      // Assert
      Assert.True(Enum.IsDefined(storageType));
   }

   [Fact]
   public void WhenJsonFilesThenEnumIsDefined()
   {
      // Arrange & Act
      var storageType = StorageType.JsonFiles;

      // Assert
      Assert.True(Enum.IsDefined(storageType));
   }

   [Fact]
   public void WhenEFCoreSqliteThenEnumIsDefined()
   {
      // Arrange & Act
      var storageType = StorageType.EFCoreSqlite;

      // Assert
      Assert.True(Enum.IsDefined(storageType));
   }

   [Fact]
   public void WhenEFCoreSqlServerThenEnumIsDefined()
   {
      // Arrange & Act
      var storageType = StorageType.EFCoreSqlServer;

      // Assert
      Assert.True(Enum.IsDefined(storageType));
   }

   [Theory]
   [InlineData(StorageType.AjisFiles)]
   [InlineData(StorageType.JsonFiles)]
   [InlineData(StorageType.EFCoreSqlite)]
   [InlineData(StorageType.EFCoreSqlServer)]
   public void WhenValidStorageTypeThenToStringReturnsName(StorageType storageType)
   {
      // Arrange & Act
      var name = storageType.ToString();

      // Assert
      Assert.NotNull(name);
      Assert.NotEmpty(name);
   }

   [Theory]
   [InlineData("AjisFiles", StorageType.AjisFiles)]
   [InlineData("JsonFiles", StorageType.JsonFiles)]
   [InlineData("EFCoreSqlite", StorageType.EFCoreSqlite)]
   [InlineData("EFCoreSqlServer", StorageType.EFCoreSqlServer)]
   public void WhenParsingValidStringThenReturnsCorrectEnum(string value, StorageType expected)
   {
      // Arrange & Act
      var result = Enum.Parse<StorageType>(value);

      // Assert
      Assert.Equal(expected, result);
   }

   [Fact]
   public void WhenParsingInvalidStringThenThrowsException()
   {
      // Arrange
      var invalidValue = "InvalidStorage";

      // Act & Assert
      Assert.Throws<ArgumentException>(() => Enum.Parse<StorageType>(invalidValue));
   }
}
