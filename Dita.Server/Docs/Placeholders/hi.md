# स्थानीयकरण में नामित प्लेसहोल्डर

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## सिंटैक्स

प्लेसहोल्डर JSON शब्दकोश मूल्यों के अंदर घुंघराले ब्रेस सिंटैक्स का उपयोग करते हैं:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

पोजीशनल प्लेसहोल्डर (, ) के विपरीत, नामित प्लेसहोल्डर **language-agnostic** — अनुवादक कोड को तोड़ने के बिना उन्हें लक्ष्य-भाषा व्याकरण से मिलान करने का आदेश दे सकते हैं।.

## भंडारण

नामित प्लेसहोल्डर के दो स्रोत हैं:

### 1. रनटाइम मान (डायनामिक डेटा के लिए सिफारिश की गई)

स्थानीयकृत स्ट्रिंग को पुनः प्राप्त करते समय सीधे मूल्यों को पास करें:

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

### 2. संग्रहीत मान (सेमी-स्टेटिक कॉन्फ़िगरेशन के लिए)

निर्देशिका में एक फ़ाइल प्रबंधित करता है:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Stored values act as **defaults** and are overridden by runtime values.

## एपीआई संदर्भ

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

### IPlaceholderसेवा

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

### विस्तार विधि

सुविधा के लिए जब साथ काम करना :

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

उपयोग:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## अनुवाद व्यवहार

जब स्वचालित अनुवाद सेवा नामित प्लेसहोल्डर के साथ पाठ का सामना करती है:

1. **Before translation**: Placeholders are masked with safe tokens (`___PH_0___`) to prevent the translation engine from modifying them.
2. **During translation**: The translation engine processes only the translatable text.
3. **After translation**: Original placeholder names (`{name}`) are restored in their correct positions.

### उदाहरण

स्रोत (अंग्रेजी):

अनुवाद के लिए तैयार:

चेक में अनुवादित:

अंतिम परिणाम:

यह सुनिश्चित करता है कि:
- प्लेसहोल्डर कभी अनुवादित या भ्रष्ट नहीं होते हैं
- लक्ष्य-भाषा व्याकरण आसपास के पाठ को स्वतंत्र रूप से व्यवस्थित कर सकता है
- समान टेम्पलेट सभी भाषाओं में सही ढंग से काम करता है

## सर्वोत्तम प्रथाओं

1. **Use descriptive name**: बेहतर है
2. **Keep placeholders minimal**: Too many placeholders make translation harder
3. **Document expected types**: Comments in the JSON file help translators understand context
4. **Prefer runtime values**: For truly dynamic data (user names, counts, dates), pass values at runtime
5. **Use stored values for defaults**: For configuration that rarely changes (app name, support email)
6. **Validate placeholders**: Use `ExtractPlaceholders()` to verify all expected placeholders are provided

## स्वत: अनुवाद के साथ एकीकरण

LibreTranslate कॉल के दौरान स्वचालित रूप से प्लेसहोल्डर संरक्षण को संभालती है। कोई अतिरिक्त विन्यास की आवश्यकता नहीं है।.

और दोनों पुनः सेवा का उपयोग करते हैं, इसलिए सभी JSON शब्दकोश अनुवाद पारदर्शी रूप से नामित प्लेसहोल्डरों का समर्थन करते हैं।.

## अनुकूलता

मौजूदा कोड पोजीशनल प्लेसहोल्डर या नो प्लेसहोल्डर का उपयोग करके अपरिवर्तित काम जारी रहता है:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

नामित प्लेसहोल्डर एपीआई योजक है - यह मौजूदा उपयोग को नहीं तोड़ता है।.
