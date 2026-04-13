using Dita.Server.Models.Enums;

namespace Dita.Tests.Server.Models.Enums;

public class StorageLocationTests
{
   [Fact]
   public void WhenValuesReadThenExpectedLocationsAreDefined()
   {
      var values = Enum.GetValues<StorageLocation>();

      Assert.Equal(
      [
         StorageLocation.InFolderStorage,
         StorageLocation.InUserProfile,
         StorageLocation.InAppData,
         StorageLocation.InSpecifiedPath
      ], values);
   }

   [Theory]
   [InlineData(StorageLocation.InFolderStorage, 0)]
   [InlineData(StorageLocation.InUserProfile, 1)]
   [InlineData(StorageLocation.InAppData, 2)]
   [InlineData(StorageLocation.InSpecifiedPath, 3)]
   public void WhenLocationCastToIntThenExpectedNumericValueIsReturned(StorageLocation location, int expectedValue)
   {
      Assert.Equal(expectedValue, (int)location);
   }
}