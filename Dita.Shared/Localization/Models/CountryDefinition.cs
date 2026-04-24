using System.Text.Json.Serialization;

namespace Dita.Shared.Localization.Models;

/// <summary>
/// Read-only country entry loaded from countries.json.
/// </summary>
public class CountryDefinition
{
    /// <summary>
    /// English country name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// International dial code.
    /// </summary>
    [JsonPropertyName("dial_code")]
    public string DialCode { get; set; } = string.Empty;

    /// <summary>
    /// Country emoji flag.
    /// </summary>
    [JsonPropertyName("emoji")]
    public string Emoji { get; set; } = string.Empty;

    /// <summary>
    /// ISO country code.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}