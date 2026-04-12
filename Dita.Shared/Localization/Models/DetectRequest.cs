using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Dita.Shared.Localization.Models;

/// <summary>
/// Represents a request to detect the language of a given text using an API key and a query string.
/// </summary>
public class DetectRequest
{
   /// <summary>
   /// Gets or sets the API key used for authenticating requests to external services.
   /// </summary>
   [JsonPropertyName("api_key")]
   public string? ApiKey { get; set; }

   /// <summary>
   /// Gets or sets the search query string used to filter or retrieve results.
   /// </summary>
   [JsonPropertyName("q")]
   public string Query { get; set; } = string.Empty;
}