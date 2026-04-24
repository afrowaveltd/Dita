using Dita.Shared.Localization.Enums;

namespace Dita.Tests.Shared.Localization;

public class ErrorCodeTextTests
{
    [Fact]
    public void WhenErrorTextCalledForBadSectorThenExpectedMessageIsReturned()
    {
        var text = ErrorCodeText.ErrorText(ErrorCode.BadSector);

        Assert.Equal("Bad sector found", text);
    }

    [Fact]
    public void WhenErrorTextCalledForIntegerCodeThenExpectedMessageIsReturned()
    {
        var text = ErrorCodeText.ErrorText(3001);

        Assert.Equal("Bad sector found", text);
    }

    [Fact]
    public void WhenErrorTextCalledForUnknownIntegerCodeThenUnknownMessageIsReturned()
    {
        var text = ErrorCodeText.ErrorText(123456);

        Assert.Equal("Unknown error (123456)", text);
    }
}
