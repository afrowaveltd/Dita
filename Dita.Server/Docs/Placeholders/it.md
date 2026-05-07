# Destinatari in Localizzazione

Dita supporta **named placeholders** nelle stringhe di localizzazione, permettendo di inserire valori dinamici in runtime mantenendo la grammatica corretta nelle lingue.

## Traduzione:

I segnaposto utilizzano la sintassi curly-brace all'interno dei valori del dizionario JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

A differenza dei segnaposto posizionali (, ), i segnaposto sono **language-agnostic** — i traduttori possono riordinarli a corrispondere la grammatica in lingua di destinazione senza rompere il codice.

## Stoccaggio

I segnaposto nominati hanno due fonti di valori:

### 1. Valori runtime (consigliati per dati dinamici)

Passare i valori direttamente quando si recupera la stringa localizzata:

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

### 2. Valori archiviati (per configurazione semistatica)

La gestione di un file nella directory:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

I valori archiviati agiscono come **defaults** e sono sovrascritti dai valori runtime.

## API

### Indice JsonStringLocalizer

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

### Servizio di protezione IPlace

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

### Metodi di estensione

Per comodità quando si lavora con:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Utilizzo:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Comportamento della traduzione

Quando il servizio di traduzione automatico incontra il testo con i segnaposto nominati:

1. **Prima traduzione ** I segnaposto sono mascherati con gettoni sicuri () per impedire al motore di traduzione di modificarli.
2. **During translation**: The translation engine processes only the translatable text.
3. **Dopo la traduzione**: I nomi dei segnaposto originali () vengono ripristinati nelle loro posizioni corrette.

### Esempio

Fonte (inglese):

Preparato per la traduzione:

Tradotto in ceco:

Risultato finale:

Questo assicura che:
- I segnaposto non sono mai tradotti o corrotti
- La grammatica in lingua mirata può riorganizzare liberamente il testo circostante
- Lo stesso modello funziona correttamente in tutte le lingue

## Migliori pratiche

1. ** Utilizzare nomi descrittivi**: è meglio di o
2. **Ottimo segnaposto minimo**: Troppi segnaposto rendono la traduzione più difficile
3. **I tipi attesi**: I commenti nel file JSON aiutano i traduttori a comprendere il contesto
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Utilizzare i valori memorizzati per i valori predefiniti**: Per la configurazione che raramente cambia (nome app, email di supporto)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## Integrazione con traduzione automatica

La conservazione automatica dei segnaposto durante le chiamate LibreTranslate. Non è necessaria alcuna configurazione aggiuntiva.

Entrambi usano il servizio di riprovazione, quindi tutte le traduzioni del dizionario JSON supportano in modo trasparente i segnaposto.

## Compatibilità del retro

Il codice esistente utilizzando segnaposto posizionale o nessun segnaposto continua a funzionare invariato:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

L'API segnaposto di nome è additivo — non rompe l'uso esistente.
