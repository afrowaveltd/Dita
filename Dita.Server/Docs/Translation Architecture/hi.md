# अनुवाद वास्तुकला

यह दस्तावेज़ डीटा की स्वचालित अनुवाद प्रणाली की मॉड्यूलर वास्तुकला का वर्णन करता है, जिसे रखरखाव, परीक्षण क्षमता और लचीलापन में सुधार करने के लिए पेश किया गया है।.

## डिजाइन लक्ष्य

रिफैक्टरिंग ने मूल मोनोलिथिक डिजाइन के साथ कई चिंताओं को संबोधित किया:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## सेवा विघटन

### बैकएंडट्रांसलेशन सर्विस (orchestrator)

**Responsibilities**:
- पाइपलाइन जीवनचक्र प्रबंधन (शुरू, पूरा होने, त्रुटि हैंडलिंग)
- Semaphore आधारित concurrency नियंत्रण (ओवरलैपिंग रन को रोकता है)
- सर्वर सत्यापन (लेटेंसी, भाषा उपलब्धता, विन्यास)
- उप-सेवाओं के लिए प्रतिनिधिमंडल

**Does NOT contain**:
- अनुवाद तर्क
- विशिष्ट प्रारूपों के लिए फ़ाइल I/O
- Retry तर्क

### देश

**Responsibilities**:
- निर्देशिका से पढ़ें
- देश के नामों को डिफ़ॉल्ट स्थानीय शब्दकोश में सिंक्रनाइज़ करें
- प्रति लक्ष्य भाषा में लापता देश के नामों का अनुवाद करें
- अनुवाद के तुरंत बाद प्रत्येक लक्ष्य शब्दकोश को सहेजें

**Key behaviors**:
- यदि डिफ़ॉल्ट भाषा अंग्रेजी है: देश के नाम के रूप में संग्रहीत है
- यदि डिफ़ॉल्ट भाषा अन्य है: अंग्रेजी नाम पहले डिफ़ॉल्ट भाषा में अनुवादित
- प्रत्येक भाषा को अपने स्वयं के रीट्री लूप के साथ स्वतंत्र रूप से संसाधित किया जाता है

### स्थानान्तरणसेवा

**Responsibilities**:
- पिछले स्नैपशॉट के साथ वर्तमान डिफ़ॉल्ट शब्दकोश की तुलना करके अतिरिक्त / हटाई गई कुंजी का पता लगाएं
- प्रत्येक लक्ष्य भाषा में जोड़ा चाबियाँ अनुवाद करें
- प्रत्येक लक्ष्य भाषा से हटाई गई कुंजी निकालें
- अगले तुलना के लिए स्नैपशॉट सहेजें

**Key behaviors**:
- मैन्युअल अनुवाद हमेशा प्राथमिकता लेते हैं (कभी कभी ओवरराइट नहीं)
- जोड़ा गया चाबियाँ तुरंत अनुवादित और सहेजी जाती हैं
- हटाए गए कुंजी को तुरंत प्रति भाषा में हटा दिया जाता है
- सभी भाषाओं को सफलतापूर्वक पूरा करने के बाद स्नैपशॉट को बचाया जाता है

### दस्तावेज़

**Responsibilities**:
- वॉक ने मार्कडाउन जड़ों को बार-बार कॉन्फ़िगर किया
- SHA-256 hashes का उपयोग करके परिवर्तित स्रोत फ़ाइलों का पता लगाएं
- ट्रैक प्रति ब्लॉक अनुवाद स्थिति में
- प्रति ब्लॉक रीट्री के साथ ब्लॉक-बाय-ब्लॉक को ट्रांसलेट करें
- अनुवाद के बाद मार्कडाउन संरचना को मान्य करें
- प्रत्येक लक्ष्य भाषा फ़ाइल को स्वतंत्र रूप से सहेजें

**Key behaviors**:
- ब्लॉक-स्तर दानेदारता: शीर्षक, पैराग्राफ, सूची आइटम अलग से अनुवाद किए जाते हैं
- मेटाडाटा ट्रैक जो ब्लॉक प्रति भाषा में सफल / विफल रहता है
- विफल ब्लॉकों को अगले रन पर वापस ले लिया जाता है बिना सफल ब्लॉकों को फिर से अनुवाद किए बिना
- संरचना सत्यापन शीर्षक गिनती, सूचियों, कोड ब्लॉक आदि मैच स्रोत सुनिश्चित करता है

## रीट्री रणनीति

सिस्टम तीन स्तरों पर रिट्रीज़ को लागू करता है:

### लेवल 1 - (LibreTranslateservice HTTP)

- एक्सोनेंशियल बैकऑफ (1s, 2s, 3s, 4s, 5s) के साथ 5 प्रयासों तक
- नेटवर्क टाइमआउट, 5xx त्रुटियों और क्षणिक विफलताओं को संभालती है
- HTTP क्लाइंट कॉन्फ़िगरेशन में निर्मित

### स्तर 2 - स्टेज (ट्रांसलेशनरी सर्विस)

- 30 सेकंड की देरी के साथ 3 प्रयासों तक
- HTTP-level retries समाप्त होने के बाद पूरे अनुवाद अनुरोध को फिर से चला जाता है
- इस स्तर पर प्लेसहोल्डर मास्किंग और बहाली लागू की जाती है

### स्तर 3 - ब्लॉक (दस्तावेज़ ट्रांसलेशन सर्विस)

- व्यक्तिगत मार्कडाउन ब्लॉक जो असफल होते हैं, मेटाडाटा में चिह्नित होते हैं
- अगली पाइप लाइन रन पर स्वचालित रूप से पुनर्प्राप्त किया गया
- सफल ब्लॉक कभी ट्रांसलेट नहीं होते हैं

## डेटा प्रवाह

### JSON शब्दकोश अनुवाद

```
[Default Dictionary]
       ↓
[Compare with Snapshot]
       ↓
[Detect Added/Removed Keys]
       ↓
For each target language:
   ├─ Load existing dictionary
   ├─ Apply removals
   ├─ Translate additions (with retry)
   └─ Save dictionary immediately
       ↓
[Save new snapshot]
```

### मार्कडाउन अनुवाद

```
[Source File]
       ↓
[Compute Hash]
       ↓
[Compare with stored hash]
       ↓
[Load translation metadata]
       ↓
For each target language:
   ├─ Extract translatable blocks
   ├─ For each block:
   │   ├─ Check metadata (already translated?)
   │   ├─ Translate with retry
   │   └─ Mark success/failure in metadata
   ├─ Reconstruct Markdown
   ├─ Validate structure
   ├─ Save target file
   └─ Save metadata
```

### देश का नाम अनुवाद

```
[countries.json]
       ↓
[Build set of country keys]
       ↓
[Update default dictionary]
       ↓
For each target language:
   ├─ Find missing country entries
   ├─ Translate each missing entry (with retry)
   └─ Save dictionary immediately
```

## राज्य दृढ़ता

### स्नैपशॉट

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### हैश फाइलें

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### अनुवाद मेटाडाटा

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - स्रोत सामग्री हैश
- प्रति भाषा ब्लॉक स्थिति (booleans की सरणी)
- अंतिम अद्यतन टाइमस्टैम्प
- **Purpose**: Enables partial re-translation of only failed blocks

### प्लेसहोल्डर स्टोरेज

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## संकेत रिपोर्टिंग

### प्रकाशक अमूर्तता

सिग्नलआर विनिर्देशों से decouples अनुवाद सेवाएं:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### अनुक्रम गारंटी

- एक ही रन के भीतर संदेश मोनोटोनिक रूप से अनुक्रमित होते हैं
- अनुक्रम संख्या के माध्यम से प्रति रन अद्वितीय हैं
- ग्राहक अंतराल का पता लगा सकते हैं या फिर ऑर्डर कर सकते हैं

### हब मैपिंग

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## एक्सटेंशन पॉइंट

### नए अनुवाद लक्ष्य को जोड़ना

1. साथ एक नया इंटरफेस बनाएं
2. इंटरफ़ेस को डोमेन-विशिष्ट तर्क के साथ कार्यान्वित करें
3. डीआई कंटेनर में पंजीकरण
4. निर्माता में इंजेक्शन
5. मौजूदा चरणों के बाद से कॉल करें

### रिट्री नीति

ओवरराइड निर्माता पैरामीटर:

```csharp
services.AddSingleton<TranslationRetryService>(
    sp => new TranslationRetryService(
        sp.GetRequiredService<ILibreTranslateService>(),
        sp.GetRequiredService<IPlaceholderService>(),
        sp.GetRequiredService<ILogger<TranslationRetryService>>(),
        stageMaxRetries: 5,        // More retries
        stageRetryDelaySeconds: 60  // Longer delays
    ));
```

### कस्टम प्लेसहोल्डर हैंडलिंग

प्लेसहोल्डर सिंटैक्स या स्टोरेज को बदलने के लिए लागू करें:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## विन्यास

### appsettings.json

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs", "/Help"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00",
    "RequestThrottleMs": 80,
    "RequestTimeoutSeconds": 10
  }
}
```

### रनटाइम ट्यूनिंग

सेटिंग
|---------|---------|--------|
80
10
3
30

## परीक्षण रणनीति

### यूनिट परीक्षण

प्रत्येक उप-सेवा स्वतंत्र रूप से परीक्षण योग्य है:

- सफलता / विफलता का अनुकरण करने के लिए मॉक
- Mock रिपोर्टिंग सत्यापित करने के लिए
- फ़ाइल I/O के लिए अस्थायी निर्देशिका का उपयोग करें
- प्रति भाषा बचत व्यवहार सत्यापित करें

### एकीकरण परीक्षण

- वास्तविक (स्थानीय) Libretranslate उदाहरण के साथ पूर्ण पाइपलाइन रन
- सिग्नल सत्यापित करें आर संदेश कनेक्ट ग्राहकों को दिया जाता है
- टेस्ट समवर्ती रन रोकथाम (semaphore)
- अनुवाद के बाद मार्कडाउन संरचना को मान्य करें

### अंत से अंत परीक्षण

- एपीआई या शेड्यूलर के माध्यम से ट्रिगर अनुवाद
- सभी लक्षित भाषा फ़ाइलों को सत्यापित / अद्यतन किया जाता है
- चेक मेटाडाटा फ़ाइलों में सही ब्लॉक स्थिति होती है
- कन्फर्म प्लेसहोल्डर अनुवाद में संरक्षित हैं

## प्रदर्शन विचार

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## मोनोलिथिक डिजाइन से प्रवास

मूल एक वर्ग में सभी तर्क निहित है। प्रवास पथ:

1. देश तर्क निकालें →
2. JSON तर्क निकालें →
3. Markdown तर्क निकालें →
4. सिग्नल निकालें आर प्रकाशन →
5. Retry तर्क निकालें →
6. प्रतिनिधिमंडल के लिए ऑर्केस्टेटर को सरल बनाएं

सभी मौजूदा इंटरफेस () अपरिवर्तित रहते हैं। पाइप लाइन के उपभोक्ता कोई ब्रेकिंग बदलाव नहीं देखते हैं।.
