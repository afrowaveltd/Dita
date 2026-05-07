# ডিসপ্লের নাম:

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## সিন্টেক্স

JSON অভিধানের মানের মধ্যে সুসংগত- ব্যাক- এন্ড অনুসরণ করা হবে:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## সংগ্রহস্থল

ফটোর মান:

### ১. সময়ের মান প্রয়োগের জন্য Run stelf-test

স্থানীয় পংক্তি পুনরুদ্ধারের সময় ব্যবহারযোগ্য মান নির্বাচন করুন:

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

### ২. সংরক্ষিত মান সংরক্ষণের জন্য ধার্য করা হবে (সেচ্যাটিক কনফিগারেশন)

ডিরেক্টরির মধ্যে ফাইল পরিচালনার সুবিধা উপস্থিত রয়েছে:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Stored values act as **defaults** and are overridden by runtime values.

## API রেফারেন্স

### JoonsQubianber ইন্ডেক্স ব্যবস্থা

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

### IPlaceherser পরিসেবা

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

### এক্সটেনশন

শপথ তাদের , যারা সকল কর ্ মনির ্ বাহ করে , কেয়ামত অবশ ্ যই হবে ।

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

ব্যবহারপ্রণালী:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## অনুবাদের আচরণ

সি. পি. এল. - র স্বয়ংক্রিয় অনুবাদ সার্ভিস যখন placessএর সঙ্গে লেখা চিহ্নিত করে:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **After translation**: Original placeholder names (`{name}`) are restored in their correct positions.

### উদাহরণ

উৎস (ইংরেজি):

অনুবাদ করার জন্য প্রস্তুত:

অনুবাদক:

চূড়ান্ত ফলাফল:

এটি নিশ্চিত করতে হবে:
- অবস্থান্তর কখনোই অনুবাদ বা অপরিবর্তিত রাখা হবে না
- টার্গেট ব্যাকগ্রাউন্ড ব্যাকরণ চারপাশে টেক্সট বিচ্ছিন্ন করতে পারে
- একই টেমপ্লেট সব ভাষায় সঠিকভাবে কাজ করে

## সর্বোত্তম চর্চা

1. **Use descriptive names**: `{userName}` is better than `{0}` or `{name}`
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Cocles** সব রকমের part ব্যবহার করা হবে

## স্বয়ংক্রিয় অনুবাদ সহ অনুবাদ

স্বয়ংক্রিয়ভাবে Libretranlate কল করার সময় স্বয়ংক্রিয়ভাবে অ্যাকাউন্ট সংরক্ষণ করা হবে। অতিরিক্ত কনফিগারেশনের প্রয়োজন নেই।.

আবার পরীক্ষা সার্ভিস ব্যবহার করে, সুতরাং সব JSON অভিধান অনুবাদ স্বচ্ছভাবে plas সমর্থন করে।.

## পূর্ব সংস্করণের সাথে সামঞ্জস্য

বর্তমানে নির্ধারিত অবস্থানের তথ্য ব্যবহার করে পৃথককরণ অথবা প্রতিস্থাপিত না হওয়া বার্তা সর্বদা পরিবর্তন করা হবে:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Part API দ্বারা উল্লেখ করা হয়েছে — এটি ব্যবহার করা হচ্ছে না ।.
