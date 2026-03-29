using Dita.Server.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics;

namespace Dita.Tests.Server.Pages;

/// <summary>
/// Unit tests for the <see cref="ErrorModel"/> Razor Page.
/// </summary>
public class ErrorModelTests
{
   [Fact]
   public void WhenCreatedThenInstanceIsNotNull()
   {
      // Arrange & Act
      var model = new ErrorModel();

      // Assert
      Assert.NotNull(model);
   }

   [Fact]
   public void WhenCreatedThenRequestIdIsNull()
   {
      // Arrange & Act
      var model = new ErrorModel();

      // Assert
      Assert.Null(model.RequestId);
   }

   [Fact]
   public void WhenRequestIdIsNullThenShowRequestIdIsFalse()
   {
      // Arrange
      var model = new ErrorModel
      {
         RequestId = null
      };

      // Act
      var showRequestId = model.ShowRequestId;

      // Assert
      Assert.False(showRequestId);
   }

   [Fact]
   public void WhenRequestIdIsEmptyThenShowRequestIdIsFalse()
   {
      // Arrange
      var model = new ErrorModel
      {
         RequestId = string.Empty
      };

      // Act
      var showRequestId = model.ShowRequestId;

      // Assert
      Assert.False(showRequestId);
   }

   [Fact]
   public void WhenRequestIdIsSetThenShowRequestIdIsTrue()
   {
      // Arrange
      var model = new ErrorModel
      {
         RequestId = "test-request-id"
      };

      // Act
      var showRequestId = model.ShowRequestId;

      // Assert
      Assert.True(showRequestId);
   }

   [Fact]
   public void WhenOnGetCalledWithActiveActivityThenRequestIdIsSet()
   {
      // Arrange
      var model = new ErrorModel();
      SetupPageContext(model);

      using var activity = new Activity("TestActivity").Start();

      // Act
      model.OnGet();

      // Assert
      Assert.NotNull(model.RequestId);
      Assert.Equal(Activity.Current?.Id, model.RequestId);
   }

   [Fact]
   public void WhenOnGetCalledWithoutActiveActivityThenRequestIdUsesTraceIdentifier()
   {
      // Arrange
      var model = new ErrorModel();
      var httpContext = new DefaultHttpContext();
      var expectedTraceId = httpContext.TraceIdentifier;

      SetupPageContext(model, httpContext);

      // Act
      model.OnGet();

      // Assert
      Assert.NotNull(model.RequestId);
      Assert.Equal(expectedTraceId, model.RequestId);
   }

   [Fact]
   public void WhenOnGetCalledThenModelStateIsValid()
   {
      // Arrange
      var model = new ErrorModel();
      SetupPageContext(model);

      // Act
      model.OnGet();

      // Assert
      Assert.True(model.ModelState.IsValid);
   }

   /// <summary>
   /// Helper method to setup the PageContext for Razor Pages.
   /// </summary>
   private static void SetupPageContext(PageModel pageModel, HttpContext? httpContext = null)
   {
      httpContext ??= new DefaultHttpContext();
      var modelState = new ModelStateDictionary();
      var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
      var pageContext = new PageContext(actionContext);

      pageModel.PageContext = pageContext;
   }
}