using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dita.Server.Api;

/// <summary>
/// Provides API endpoints for localization-related server operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class LocalizeController(ILocalizeService localizeService) : ControllerBase
{
   private readonly ILocalizeService _localizeService = localizeService;

   /// <summary>
   /// Resolves a client phrase through application dictionaries and creates a default-language key when missing.
   /// </summary>
   /// <remarks>
   /// The endpoint first checks the requested target dictionary. If the phrase is not found, it checks the default
   /// dictionary and finally creates a missing default entry as <c>key = value = text</c>. It does not call the
   /// translation server directly; scheduled translation can translate newly created keys later.
   /// </remarks>
   /// <param name="request">The localization request containing the client phrase and optional target language.</param>
   /// <param name="cancellationToken">Token used to cancel request processing.</param>
   /// <returns>The localized text and metadata describing where the value was resolved from.</returns>
   [HttpPost]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType(typeof(TextLocalizationResponse), StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
   public async Task<ActionResult<TextLocalizationResponse>> PostAsync(
      [FromBody] TextLocalizationRequest request,
      CancellationToken cancellationToken)
   {
      var response = await _localizeService.LocalizeAsync(request, cancellationToken).ConfigureAwait(false);
      if (!response.Success || response.Data is null)
      {
         return BadRequest(CreateProblem("Localization failed.", response.Message));
      }

      return Ok(response.Data);
   }

   /// <summary>
   /// Resolves a client phrase through application dictionaries using query-string parameters.
   /// </summary>
   /// <param name="text">The phrase supplied by the client. The phrase is used as the dictionary key.</param>
   /// <param name="targetLanguage">Optional target language or culture code. Defaults to the current request UI culture.</param>
   /// <param name="cancellationToken">Token used to cancel request processing.</param>
   /// <returns>The localized text and metadata describing where the value was resolved from.</returns>
   [HttpGet]
   [Produces("application/json")]
   [ProducesResponseType(typeof(TextLocalizationResponse), StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
   public async Task<ActionResult<TextLocalizationResponse>> GetAsync(
      [FromQuery] string text,
      [FromQuery] string? targetLanguage,
      CancellationToken cancellationToken)
   {
      var request = new TextLocalizationRequest
      {
         Text = text,
         TargetLanguage = targetLanguage
      };

      var response = await _localizeService.LocalizeAsync(request, cancellationToken).ConfigureAwait(false);
      if (!response.Success || response.Data is null)
      {
         return BadRequest(CreateProblem("Localization failed.", response.Message));
      }

      return Ok(response.Data);
   }

   private ProblemDetails CreateProblem(string title, string? detail)
      => new()
      {
         Title = title,
         Detail = detail,
         Status = StatusCodes.Status400BadRequest,
         Instance = HttpContext.Request.Path
      };
}
