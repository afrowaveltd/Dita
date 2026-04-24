namespace Dita.Shared.Localization.Enums;

/// <summary>
/// Localization and translation-related error codes (range 5000-5999).
/// </summary>
public enum LocalizationError
{
   /// <summary>
   /// Dictionary or translation file is corrupted.
   /// </summary>
   DictionaryCorrupted = 5001,

   /// <summary>
   /// Dictionary or translation file not found.
   /// </summary>
   DictionaryNotFound = 5002,

   /// <summary>
   /// Encoding conversion failed.
   /// </summary>
   EncodingConversionFailed = 5003,

   /// <summary>
   /// Invalid locale or culture code.
   /// </summary>
   InvalidLocale = 5004,

   /// <summary>
   /// Invalid translation format.
   /// </summary>
   InvalidTranslationFormat = 5005,

   /// <summary>
   /// Language detection failed.
   /// </summary>
   LanguageDetectionFailed = 5006,

   /// <summary>
   /// Requested language is not supported.
   /// </summary>
   LanguageNotSupported = 5007,

   /// <summary>
   /// Locale file parsing failed.
   /// </summary>
   LocaleParsingFailed = 5008,

   /// <summary>
   /// Missing translation for the specified key.
   /// </summary>
   MissingTranslation = 5009,

   /// <summary>
   /// No error occurred (success).
   /// </summary>
   None = 5000,

   /// <summary>
   /// Plural form resolution failed.
   /// </summary>
   PluralFormResolutionFailed = 5010,

   /// <summary>
   /// Resource bundle loading failed.
   /// </summary>
   ResourceBundleLoadFailed = 5011,

   /// <summary>
   /// String formatting or interpolation failed.
   /// </summary>
   StringFormattingFailed = 5012,

   /// <summary>
   /// Translation API authentication failed.
   /// </summary>
   TranslationApiAuthenticationFailed = 5013,

   /// <summary>
   /// Translation API is unavailable or unreachable.
   /// </summary>
   TranslationApiUnavailable = 5014,

   /// <summary>
   /// Translation failed due to unknown reason.
   /// </summary>
   TranslationFailed = 5015,

   /// <summary>
   /// Translation queue is full and cannot accept more items.
   /// </summary>
   TranslationQueueFull = 5016,

   /// <summary>
   /// Translation service error occurred.
   /// </summary>
   TranslationServiceError = 5017,

   /// <summary>
   /// Translation timeout occurred.
   /// </summary>
   TranslationTimeout = 5018,

   /// <summary>
   /// Unknown language code specified.
   /// </summary>
   UnknownLanguage = 5019,

   /// <summary>
   /// Unknown localization error occurred.
   /// </summary>
   UnknownLocalizationError = 5020,

   /// <summary>
   /// Unsupported character set or encoding.
   /// </summary>
   UnsupportedEncoding = 5021
}
