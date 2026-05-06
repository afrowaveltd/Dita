namespace Dita.Shared.Localization.Services;

/// <summary>
/// Defines operations for managing named placeholders in localization strings.
/// Placeholders use the syntax {name} and are stored in a separate JSON file (placeholders.json)
/// alongside locale dictionaries.
/// </summary>
public interface IPlaceholderService
{
    /// <summary>
    /// Gets all placeholder values registered for a specific key.
    /// </summary>
    /// <param name="key">The localization key containing placeholders.</param>
    /// <returns>Dictionary of placeholder names to their values, or empty if none registered.</returns>
    Dictionary<string, string> GetPlaceholders(string key);

    /// <summary>
    /// Sets a placeholder value for a specific key.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="placeholderName">The placeholder name without braces (e.g. "name").</param>
    /// <param name="value">The value to substitute.</param>
    void SetPlaceholder(string key, string placeholderName, string value);

    /// <summary>
    /// Removes all placeholder values for a specific key.
    /// </summary>
    /// <param name="key">The localization key.</param>
    void RemoveKey(string key);

    /// <summary>
    /// Formats a template string by replacing named placeholders with provided values.
    /// Supports {name} syntax. If a placeholder is not found in the provided values,
    /// falls back to values from the placeholders.json file, then leaves the placeholder unchanged.
    /// </summary>
    /// <param name="key">The localization key associated with this template (used for stored placeholder lookup).</param>
    /// <param name="template">The string template containing {name} placeholders.</param>
    /// <param name="values">Optional runtime values for placeholders.</param>
    /// <returns>The formatted string with placeholders replaced.</returns>
    string Format(string key, string template, Dictionary<string, string>? values = null);

    /// <summary>
    /// Formats a template string by replacing named placeholders with provided values.
    /// Supports {name} syntax. If a placeholder is not found in the provided values,
    /// falls back to values from the placeholders.json file, then leaves the placeholder unchanged.
    /// </summary>
    /// <param name="template">The string template containing {name} placeholders.</param>
    /// <param name="values">Optional runtime values for placeholders.</param>
    /// <returns>The formatted string with placeholders replaced.</returns>
    string Format(string template, Dictionary<string, string>? values = null);

    /// <summary>
    /// Extracts all placeholder names from a template string.
    /// </summary>
    /// <param name="template">The template to analyze.</param>
    /// <returns>Array of placeholder names without braces.</returns>
    string[] ExtractPlaceholders(string template);

    /// <summary>
    /// Checks if a template string contains any named placeholders.
    /// </summary>
    /// <param name="template">The template to check.</param>
    /// <returns>True if the template contains {name} placeholders.</returns>
    bool HasPlaceholders(string template);

    /// <summary>
    /// Replaces placeholders in text with temporary values suitable for translation,
    /// then restores the original placeholder names after translation.
    /// </summary>
    /// <param name="template">The template with {name} placeholders.</param>
    /// <returns>
    /// A tuple containing:
    /// - preparedText: Text ready for translation (placeholders masked)
    /// - restore: Function to restore placeholders in translated text
    /// </returns>
    (string preparedText, Func<string, string> restore) PrepareForTranslation(string template);

    /// <summary>
    /// Replaces placeholders with reference values when available before translation,
    /// then restores the placeholder names in translated text.
    /// Missing reference values are masked with translation-safe tokens.
    /// </summary>
    /// <param name="template">The template with {name} placeholders.</param>
    /// <param name="referenceValues">Optional example values used to give the translation service context.</param>
    /// <returns>
    /// A tuple containing:
    /// - preparedText: Text ready for translation
    /// - restore: Function to restore placeholders in translated text
    /// </returns>
    (string preparedText, Func<string, string> restore) PrepareForTranslation(
        string template,
        Dictionary<string, string>? referenceValues);

    /// <summary>
    /// Repairs placeholder artifacts in translated text by recovering canonical placeholders
    /// from the original source text when translation engines rewrite placeholder tokens.
    /// </summary>
    /// <param name="sourceText">The original source text containing canonical placeholders.</param>
    /// <param name="translatedText">The translated text to repair.</param>
    /// <returns>The translated text with recovered placeholders where possible.</returns>
    string RestorePlaceholdersFromSource(string sourceText, string translatedText);

    /// <summary>
    /// Saves the current placeholder values to disk.
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Loads placeholder values from disk.
    /// </summary>
    Task LoadAsync();
}
