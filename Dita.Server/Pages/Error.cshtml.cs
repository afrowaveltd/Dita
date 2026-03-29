using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Dita.Server.Pages;

/// <summary>
/// Razor Page model for the error page that displays error information to users.
/// </summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
   /// <summary>
   /// Gets or sets the request identifier for tracking the error.
   /// </summary>
   public string? RequestId { get; set; }

   /// <summary>
   /// Gets a value indicating whether the request ID should be displayed.
   /// </summary>
   public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

   /// <summary>
   /// Handles GET requests to the error page and initializes the request ID.
   /// </summary>
   public void OnGet()
   {
      RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
   }
}