namespace Dita.Shared.Localization.Models;

/// <summary>
/// Identifies where a localization or translation response was resolved from.
/// </summary>
public enum TextResolutionSource
{
   /// <summary>
   /// The text was found in a locale dictionary for the requested target language.
   /// </summary>
   TargetDictionary,

   /// <summary>
   /// The text was found in the configured default language dictionary.
   /// </summary>
   DefaultDictionary,

   /// <summary>
   /// The text was not present and was added to the configured default language dictionary.
   /// </summary>
   DefaultDictionaryCreated,

   /// <summary>
   /// The text was returned by the configured translation server.
   /// </summary>
   TranslationServer,

   /// <summary>
   /// The original client text was returned without dictionary or translation-server resolution.
   /// </summary>
   OriginalText
}

/// <summary>
/// Request body for resolving a client phrase through the application's localization dictionaries.
/// </summary>
public sealed class TextLocalizationRequest
{
   /// <summary>
   /// Gets or sets the phrase supplied by the client. The phrase is used as the dictionary key.
   /// </summary>
   public string Text { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the target language or culture code. When omitted, the current request UI culture is used.
   /// </summary>
   public string? TargetLanguage { get; set; }

   /// <summary>
   /// Gets or sets optional runtime placeholder values used to format named placeholders such as <c>{age}</c>.
   /// </summary>
   public Dictionary<string, string>? Values { get; set; }
}

/// <summary>
/// Response returned after resolving a phrase through the localization dictionary workflow.
/// </summary>
public sealed class TextLocalizationResponse
{
   /// <summary>
   /// Gets or sets the original phrase supplied by the client.
   /// </summary>
   public string Text { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the localized result after dictionary lookup and optional placeholder formatting.
   /// </summary>
   public string LocalizedText { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the target language or culture that was used for dictionary lookup.
   /// </summary>
   public string TargetLanguage { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the configured default language used as the writable fallback dictionary.
   /// </summary>
   public string DefaultLanguage { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets a value indicating whether the phrase was found in the requested target dictionary.
   /// </summary>
   public bool FoundInTargetDictionary { get; set; }

   /// <summary>
   /// Gets or sets a value indicating whether the phrase was added to the default dictionary during the request.
   /// </summary>
   public bool AddedToDefaultDictionary { get; set; }

   /// <summary>
   /// Gets or sets where the returned text was resolved from.
   /// </summary>
   public TextResolutionSource ResolvedFrom { get; set; }
}

/// <summary>
/// Request body for translating dynamic client text without writing to locale dictionaries.
/// </summary>
public sealed class TextTranslationRequest
{
   /// <summary>
   /// Gets or sets the phrase or sentence supplied by the client.
   /// </summary>
   public string Text { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the source language code. When omitted, the configured default language is used.
   /// </summary>
   public string? SourceLanguage { get; set; }

   /// <summary>
   /// Gets or sets the target language or culture code. When omitted, the current request UI culture is used.
   /// </summary>
   public string? TargetLanguage { get; set; }

   /// <summary>
   /// Gets or sets optional runtime placeholder values used as translation references and final formatting values.
   /// </summary>
   public Dictionary<string, string>? Values { get; set; }
}

/// <summary>
/// Response returned after resolving or translating dynamic client text.
/// </summary>
public sealed class TextTranslationResponse
{
   /// <summary>
   /// Gets or sets the original phrase supplied by the client.
   /// </summary>
   public string Text { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the translated text returned to the client.
   /// </summary>
   public string TranslatedText { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the source language used for translation-server calls.
   /// </summary>
   public string SourceLanguage { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the target language or culture that was requested.
   /// </summary>
   public string TargetLanguage { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets a value indicating whether the phrase was found in a target dictionary before calling the translation server.
   /// </summary>
   public bool FoundInTargetDictionary { get; set; }

   /// <summary>
   /// Gets or sets a value indicating whether the translation server was used to produce the response.
   /// </summary>
   public bool TranslationServerUsed { get; set; }

   /// <summary>
   /// Gets or sets where the returned text was resolved from.
   /// </summary>
   public TextResolutionSource ResolvedFrom { get; set; }
}
