using Dita.Server.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace Dita.Tests.Server.Pages;

/// <summary>
/// Unit tests for the <see cref="IndexModel"/> Razor Page.
/// </summary>
public class IndexModelTests
{
    [Fact]
    public void WhenCreatedThenInstanceIsNotNull()
    {
        // Arrange & Act
        var model = new IndexModel();

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void WhenOnGetCalledThenNoExceptionThrown()
    {
        // Arrange
        var model = new IndexModel();
        SetupPageContext(model);

        // Act
        var exception = Record.Exception(() => model.OnGet());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void WhenOnGetCalledThenModelStateIsValid()
    {
        // Arrange
        var model = new IndexModel();
        SetupPageContext(model);

        // Act
        model.OnGet();

        // Assert
        Assert.True(model.ModelState.IsValid);
    }

    /// <summary>
    /// Helper method to setup the PageContext for Razor Pages.
    /// </summary>
    private static void SetupPageContext(PageModel pageModel)
    {
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        pageModel.PageContext = pageContext;
    }
}
