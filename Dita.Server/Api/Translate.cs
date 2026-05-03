using Dita.Shared.Localization.Models;
using Dita.Shared.Localization.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dita.Server.Api;

/// <summary>
/// Provides API endpoints for translation-related server operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TranslateController(ITranslateService translateService) : ControllerBase
{
   private readonly ITranslateService _translateService = translateService;

   /// <summary>
   /// Translates dynamic client text without writing to localization dictionaries.
   /// </summary>
   /// <remarks>
   /// The endpoint checks the requested target dictionary first as a fast read-only cache. If the phrase is not found,
   /// it calls the configured translation server and returns the result without saving it.
   /// </remarks>
   /// <param name="request">The translation request containing the client text and requested language pair.</param>
   /// <param name="cancellationToken">Token used to cancel request processing.</param>
   /// <returns>The translated text and metadata describing whether a dictionary or the translation server was used.</returns>
   [HttpPost]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType(typeof(TextTranslationResponse), StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
   public async Task<ActionResult<TextTranslationResponse>> PostAsync(
      [FromBody] TextTranslationRequest request,
      CancellationToken cancellationToken)
   {
      var response = await _translateService.TranslateAsync(request, cancellationToken).ConfigureAwait(false);
      if (!response.Success || response.Data is null)
      {
         return BadRequest(CreateProblem("Translation failed.", response.Message));
      }

      return Ok(response.Data);
   }

   /// <summary>
   /// Translates dynamic client text using query-string parameters.
   /// </summary>
   /// <param name="text">The phrase or sentence supplied by the client.</param>
   /// <param name="targetLanguage">Optional target language or culture code. Defaults to the current request UI culture.</param>
   /// <param name="sourceLanguage">Optional source language code. Defaults to the configured default language.</param>
   /// <param name="cancellationToken">Token used to cancel request processing.</param>
   /// <returns>The translated text and metadata describing whether a dictionary or the translation server was used.</returns>
   [HttpGet]
   [Produces("application/json")]
   [ProducesResponseType(typeof(TextTranslationResponse), StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
   public async Task<ActionResult<TextTranslationResponse>> GetAsync(
      [FromQuery] string text,
      [FromQuery] string? targetLanguage,
      [FromQuery] string? sourceLanguage,
      CancellationToken cancellationToken)
   {
      var request = new TextTranslationRequest
      {
         Text = text,
         TargetLanguage = targetLanguage,
         SourceLanguage = sourceLanguage
      };

      var response = await _translateService.TranslateAsync(request, cancellationToken).ConfigureAwait(false);
      if (!response.Success || response.Data is null)
      {
         return BadRequest(CreateProblem("Translation failed.", response.Message));
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
