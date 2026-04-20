using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Dita.Server.Pages;

/// <summary>
/// Razor Page model for the home/index page.
/// </summary>
public class IndexModel(IStringLocalizer<IndexModel> localizer) : PageModel
{
   /// <summary>
   /// Translation service for localizing strings in the Index page.
   /// </summary>
   public readonly IStringLocalizer<IndexModel> _localizer = localizer;
   /// <summary>
   /// Handles GET requests to the home page.
   /// </summary>
   public void OnGet()
   {
   }
}