# Titulares designados en Localización

Dita soporta **nombrados titulares de posición** en cadenas de localización, permitiendo que los valores dinámicos se inserten en tiempo de ejecución, preservando la gramática correcta entre los idiomas.

## Sintaxis

Los marcadores de posición usan la sintaxis de grosella dentro de los valores del diccionario JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

A diferencia de los titulares de posición (, ), nombrados titulares de lugares son ** lingüaje-agnostic** — traductores pueden reordenarlos para que coincidan con la gramática de lengua de destino sin romper el código.

## Almacenamiento

Los titulares designados tienen dos fuentes de valores:

### 1. Valores de tiempo de ejecución (recomendados para datos dinámicos)

Pase valores directamente al recuperar la cadena localizada:

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

El directorio gestiona un archivo:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Los valores almacenados actúan como **defaults** y son superados por valores de tiempo de ejecución.

## Referencia de API

### JsonStringLocalizer indexer

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

### IPlaceholderService

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

Para mayor comodidad al trabajar con:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

uso:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Comportamiento de traducción

Cuando el servicio de traducción automática encuentra texto con los titulares nombrados:

1. **Antes de la traducción**: Los propietarios de puestos están enmascarados con fichas seguras () para evitar que el motor de traducción los modifique.
2. **Durante la traducción**: El motor de traducción sólo procesa el texto translatable.
3. **Después de la traducción**: Los nombres originales de los marcadores de posición () se restauran en sus posiciones correctas.

### Ejemplo

Fuente:

Preparado para la traducción:

Traducido al Checo:

Resultado final:

Esto garantiza que:
- Los propietarios nunca se traducen o corrompen
- La gramática en el idioma objetivo puede reorganizar libremente el texto circundante
- La misma plantilla funciona correctamente en todos los idiomas

## Buenas prácticas

1. **Use nombres descriptivos**: es mejor que o
2. **Mantenga a los propietarios mínimos**: Demasiados propietarios hacen la traducción más difícil
3. **Tipos previstos en el documento**: Comentarios en el archivo JSON ayuda a los traductores a entender contexto
4. ** Valores de tiempo de ejecución prefijado**: Para datos realmente dinámicos (nombres de usuario, cuentas, fechas), pase valores a tiempo de ejecución
5. **Utilice valores almacenados por defectos**: Para la configuración que rara vez cambia (nombre de solicitud, correo electrónico de soporte)
6. ** Accionistas validados**: Use to verify all expected placeholders are provided

## Integración con traducción automática

El manipula automáticamente la preservación del marcador de posición durante llamadas LibreTranslate. No se necesita configuración adicional.

Los y ambos utilizan el servicio de retry, así que todas las traducciones del diccionario JSON apoyan transparentemente a los titulares de lugares nombrados.

## Compatibilidad de retroceso

El código existente utilizando los marcadores de posición o ningún titular sigue funcionando sin cambios:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

El marcador de posición llamado API es aditivo — no rompe el uso existente.
