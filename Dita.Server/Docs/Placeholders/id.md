# Nama Placeholder dalam Lokalisasi

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Sintaksis

Placeholder menggunakan sintaks curly-brace dalam nilai kamus JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## Penyimpanan

Pemilik placeholder bernama memiliki dua sumber nilai:

### 1. Nilai runtime (disarankan untuk data dinamis)

Nilai lulus secara langsung ketika mengambil string lokal:

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

### 2. Nilai tersimpan (untuk konfigurasi semi-statis)

Mengelola berkas dalam direktori:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Stored values act as **defaults** and are overridden by runtime values.

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

### Metode ekstensi

Untuk kenyamanan ketika bekerja dengan:

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

## Perilaku terjemahan

Ketika layanan terjemahan otomatis bertemu teks dengan placeholder bernama:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **After translation**: Original placeholder names (`{name}`) are restored in their correct positions.

### Contoh

Sumber (Inggris):

Siap untuk terjemahan:

Diterjemahkan ke Ceko:

Hasil akhir:

Ini memastikan bahwa:
- Placeholder tidak pernah diterjemahkan atau rusak
- Target - bahasa tata bahasa dapat mengatur ulang teks sekitarnya secara bebas
- Templat yang sama bekerja dengan benar dalam semua bahasa

## Latihan terbaik

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## Integrasi dengan terjemahan otomatis

Secara otomatis menangani pelestarian placeholder selama panggilan LibreTranslate. Tak ada konfigurasi tambahan yang diperlukan.

Keduanya menggunakan layanan ulang, sehingga semua kamus terjemahan JSON mendukung pemilik placeholder bernama.

## Kompabilitas mundur

Terdapat kode menggunakan placeholder posisi atau tidak ada placeholder terus bekerja tanpa perubahan:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Placeholder API bernama additive - tidak merusak penggunaan yang ada.
