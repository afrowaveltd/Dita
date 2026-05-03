using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dita.Server.Pages.Admin;

/// <summary>
/// Razor Page model for the live translation monitoring dashboard.
/// This page subscribes to the <see cref="Dita.Shared.Localization.Hubs.LocalizationHub"/>
/// SignalR hub to display real-time translation pipeline events.
/// </summary>
public class LiveTranslationModel : PageModel
{
    /// <summary>
    /// Handles GET requests to the live translation monitor page.
    /// </summary>
    public void OnGet()
    {
    }
}
