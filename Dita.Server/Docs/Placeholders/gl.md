# Nomeado titular na localización

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## sintaxe

Os propietarios de sitios usan a sintaxe de cerebro rizado nos valores do dicionario JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

A diferenza dos localizadores posicionais (, ), os localizadores nomeados son **linguaxe-agnostic** – os tradutores poden reordenalos para que coincidan coa gramática da lingua obxectivo sen romper o código.

## Almacenamento

As persoas interesadas teñen dúas fontes de valores:

### 1. Tempo de execución (recomendado para datos dinámicos)

Pasar valores directamente ao recuperar a cadea localizada:

```csharp
// In a Razor page or controller
@inject JsonStringLocalizer Localizer

var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

### 2. Valores almacenados (para configuración semiestática)

Xestiona un ficheiro no directorio:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Stored values act as **defaults** and are overridden by runtime values.

## API referencia

### indexador jsonstringlocalizer

```csharp
// Without placeholders (backward compatible)
LocalizedString text = localizer["SomeKey"];

// With positional formatting (backward compatible)
LocalizedString text = localizer["SomeKey", "arg1", "arg2"];

// With named placeholders (new)
LocalizedString text = localizer["SomeKey", new Dictionary<string, string>
{
    ["name"] = "value"
}];
```

### Servizos IPlaceholder

```csharp
public interface IPlaceholderService
{
    // Get stored placeholders for a key
    Dictionary<string, string> GetPlaceholders(string key);
    
    // Set a stored placeholder value
    void SetPlaceholder(string key, string placeholderName, string value);
    
    // Remove all stored placeholders for a key
    void RemoveKey(string key);
    
    // Format a template with placeholders
    string Format(string template, Dictionary<string, string>? values = null);
    
    // Extract placeholder names from template
    string[] ExtractPlaceholders(string template);
    
    // Check if template contains placeholders
    bool HasPlaceholders(string template);
    
    // Prepare text for translation (mask placeholders)
    (string preparedText, Func<string, string> restore) PrepareForTranslation(string template);
    
    // Persist/load from disk
    Task SaveAsync();
    Task LoadAsync();
}
```

### Métodos de extensión

Por comodidade ao traballar con:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Uso:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Comportamento de tradución

Cando o servizo de tradución automática atopa texto cos localizadores:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **Despois da tradución**: Os nomes orixinais dos propietarios de lugares () son restaurados nas súas posicións correctas.

### Exemplo

Fonte (inglés):

Preparados para a tradución:

Traducido ao checo:

Resultado final:

Isto asegura que:
- Os propietarios nunca foron traducidos ou corrompidos
- A gramática da lingua obxectivo pode reorganizar libremente o texto que o rodea
- O mesmo modelo funciona correctamente en todos os idiomas

## Boas prácticas

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## Integración con tradución automática

O control automático da preservación dos propietarios de lugares durante as chamadas LibreTranslate. Non se necesita configuración adicional.

Os dous usan o servizo de retry, polo que todas as traducións do dicionario JSON apoian de forma transparente os titulares de lugares nomeados.

## Compatibilidade Backward

O código existente que utiliza os localizadores posicionais ou non os localizadores non funciona sen cambios

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

A API do titular de sitio é aditivo - non rompe o uso existente.
