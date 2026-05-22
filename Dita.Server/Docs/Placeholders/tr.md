# yerelleşmede yer sahipleri

Dita, ** adı verilen yer sahipleri ** yerelleştirme dizelerinde, dinamik değerlerin dillerin doğru gramerleri korumak için tükenmesine izin veriyor.

## gazetecilik

Yerleşim sahipleri JSON sözlüğü değerlerinin içinde eğrilikli sözelleri kullanır:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Konumal yer sahipleri aksine (, ), adı verilen yer sahipleri ** dil-agnostic** - çevirmenler, kodu bozmadan hedef dil gramerlerini yeniden sipariş edebilirler.

## Depolama

Adlandırılmış yer sahipleri iki değer kaynağı vardır:

### 1. Runtime values (recommended for dynamic data)

Yerelleştirilmiş dizeyi retrievlendirmede doğrudan para kazanır:

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

### 2. Mağaza değerleri ( yarı-statik konfigürasyon için)

Rehberde bir dosyayı yönetir:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Mağazalı değerler **defaults** olarak hareket eder ve zaman değerleri ile aşırıdır.

## API referans

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

### Dahili yöntemler

Çalışma yaparken rahatlık için:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Kullanım:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Çeviri davranışı

Otomatik çeviri servisi isim yer sahipleri ile metin karşılaştığında:

1. ** Çeviriden önce**: Yer sahipleri güvenli jetonlarla maskelenir () çeviri motorun onları değiştirmesini önlemek için.
2. ** Çeviri sırasında**: Çeviri motoru süreçleri sadece translatable metin.
3. ** Çeviri sonrası**: Orijinal yer sahipleri isimleri () doğru pozisyonlarda restore edilir.

### Örnek

Kaynak (İngilizce):

Çeviri için hazırlanın:

Çek'e Çeviri:

Son sonuç:

Bu bunu sağlar:
- Placeholder asla tercüme edilmez veya bozulmuş
- Hedef dil grameri çevreleyen metni özgürce yeniden düzenleyebilir
- Aynı şablon tüm dillerde doğru çalışır

## En iyi uygulamalar

1. **Use descriptive names **: is better than or or
2. **Yer sahipleri minimum**: Çok fazla yer sahibi çevirisi daha zor yapar
3. **Belirli türleri**: JSON dosyasında yorumlar, çevirmenlerin bağlam anlamalarına yardımcı olur
4. **Prefer runtime values**: Gerçekten dinamik veriler için (kullanıcı isimler, sayılar, tarihler), runtime values at runtime
5. ** varsayılanlar için depolanmış değerleri kullanın**: Nadiren değişiklikler olan yapılandırma için (örneğin, e-posta desteği)
6. **Validate yer sahipleri**: Tüm beklenen yer sahipleri için doğrulamak için kullanılır

## Otomatik çeviri ile entegrasyon

LibreTranslate çağrıları sırasında otomatik olarak yer sahibi korumayı ele alır. Ek yapılandırmaya gerek yok.

Ve her ikisi de yeniden deneme hizmeti kullanır, bu yüzden tüm JSON sözlüğü çevirileri, yer sahipleri adında şeffaf bir şekilde destek verir.

## Backward uyumluluk

Pozisyonal yer sahipleri veya yer sahipleri kullanan mevcut kod, değişmeden çalışmaya devam ediyor:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Adı yer sahibi API katkıdır - mevcut kullanımı bozmaz.
