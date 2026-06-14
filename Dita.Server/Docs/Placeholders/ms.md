# Pemegang Tempat Bernama Bernama di Lokalisasi

Luadon Dita mendukung **named placeholders** dalam string lokalisasi, memungkinkan nilai dinamis untuk disisipkan pada waktu jalan sambil melestarikan tata bahasa yang benar di seluruh bahasa.

## Sintaksis

Pemegang tempat menggunakan sintaks curly-brace di dalam nilai kamus JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Tidak seperti pemegang tempat kedudukan (, ), pemegang tempat bernama adalah **language-agnostik** — penerjemah dapat memesan ulang untuk mencocokkan tata bahasa target tanpa melanggar kode.

## Penyimpanan

Pemegang tempat yang dinamai memiliki dua sumber nilai:

### 1. Nilai runtime (disarankan untuk data dinamis)

Nilai Lulus secara langsung ketika mengambil kembali string terlokalisasi:

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

### 2. Nilai penyimpanan (untuk konfigurasi semi-statis)

The mengelola file di direktori:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Nilai-nilai yang tersimpan sebagai **defaults** dan ditindih oleh nilai-nilai runtime.

## Referensi API

### Pengindeks JsonStringLocalizer

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

### layanan pemegang tempat

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

### Metode Sambungan Infonic

Untuk kenyamanan ketika bekerja dengan :

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Penggunaan:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Peri laku Terjemahan Bahasan

Ketika layanan penerjemahan otomatis bertemu teks dengan pemegang tempat bernama:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **Setelah terjemahan **: Nama pemegang tempat asal () dipulihkan dalam posisi yang benar.

### Contoh senam

Sumber (Inggris):

Persiapan untuk terjemahan:

Terjemahan diterjemahkan ke bahasa Ceko:

Hasil akhir:

Ini memastikan bahwa:
- Pemegang tempat tidak pernah diterjemahkan atau dirusak
- Tata bahasa Target-bahasa dapat mengatur ulang teks sekitarnya secara bebas
- Glines tidak menjumpai fail imej: %s

## Praktek terbaik

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## Integrasi dengan terjemahan otomatis

Secara otomatis, placeholder awet selama panggilan LibreTranslate. Konfigurasi tambahan tidak diperlukan.

Kekhanan dan keduanya menggunakan layanan retry, sehingga semua kamus JSON terjemahan transparan mendukung pemegang tempat bernama.

## Keserasian Kwarnas Mundur

Kode yang ada menggunakan pemegang tempat kedudukan atau tidak ada pemegang tempat yang terus bekerja tidak berubah:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

API pemegang tempat yang bernama API adalah aditif — tidak merusak penggunaan yang ada.
