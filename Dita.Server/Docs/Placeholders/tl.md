# Ipinangalan na mga May - ari ng Lugar sa Lokalisasyon

Ang Data ay sumusuporta sa **named placeholders** sa localization stranses, na nagpapahintulot sa dynamic na mga halaga na ipasok sa runtime habang iniingatan ang tamang balarila sa ibayo ng mga wika.

## kabutihan

Ginagamit ng mga may - ari ng lugar ang curly-bace contraction sa loob ng diksyunaryong JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Hindi tulad ng mga positional placeholder (, ), na pinangalanang placeholders ay **wika-agnostic** — ang mga tagapagsalin ay maaaring mag-ayos sa kanila na tumugma sa target-wikang balarila nang hindi nilalabag ang kodigo.

## Pagkawasak

Ang mga may - ari ng lugar ay may dalawang pinagmumulan ng mga pamantayan:

### 1. Tumatakbo ng mga halaga ng panahon (isinaayos para sa dynamic data)

Pasahin nang tuwiran ang mga pamantayan kapag kinukuha ang lokal na tali:

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

### 2. Nakaimbak na mga halaga (para sa semi-statikong pagsasaayos)

Ang namamahala ay isang file sa directory:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Ang mga naka-imbak na halaga ay gumaganap bilang **defaults** at nangingibabaw sa pamamagitan ng runtime na mga halaga.

## Ang API reference

### json frylocalizer indexer

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

### pag - aayos sa dako

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

### Mga Paraan ng Paglitaw

Para sa kaginhawahan kapag gumagawang kasama ng :

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Paggamit:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Pag - uugali sa pagsasalin

Nang ang automatic translation service ay makasalubong ng pangalang placeholders:

1. ** Bago isalin**: Ang mga may-ari ng lugar ay nakabalatkayo na may ligtas na mga token () upang hindi mabago ng transaksyon engine ang mga ito.
2. **During translation**: The translation engine processes only the translatable text.
3. ** Pagkatapos ng pagsasalin**: Ang mga orihinal na placeholder na pangalan () ay ibinalik sa kanilang mga tamang posisyon.

### Halimbawa

Pinagmulan (Ingles):

Handa para sa pagsasalin:

Isinalin sa wikang Czech:

Huling resulta:

Tinitiyak nito:
- Ang mga may - ari ng lugar ay hindi kailanman isinasalin o sinisira
- Maaaring malayang baguhin ng balarila ng Target-wika ang nakapaligid na teksto
- Ang gayunding template ay wastong gumagana sa lahat ng wika

## Pinakamahuhusay na kaugalian

1. **Gumamit ng naglalarawang mga pangalan**: ay mas mabuti kaysa o
2. ** Panatilihing kaunti ang mga may - ari ng lugar**: Ginagawang mas mahirap ng napakaraming may - ari ng lugar ang pagsasalin
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Ginagamit upang tiyakin ang lahat ng inaasahang may - ari ng lugar

## Pandarayuhan na may awtomatikong salin

Ang awtomatikong humahawak ng placeholder preserve sa panahon ng LibreTranslate calls. Hindi na kailangan ang karagdagang pagsasaayos.

Ang at parehong gumagamit ng serbisyong retry, kaya ang lahat ng mga salin ng diksyunaryong JSON ay malinaw na sumusuporta sa mga pinangalanang placeholder.

## Panunumbalik

Ang pag-iral ng code gamit ang mga positional placeholder o walang mga placeholder ay patuloy na gumagana nang hindi nagbabago:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Ang pangalang placeholder API ay additive — hindi nito sinisira ang umiiral na gamit.
