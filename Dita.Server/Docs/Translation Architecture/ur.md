# ترجمہ‌نگار

اس دستاویز میں دیتا کے خودکار ترجمے کے نظام کے موڈلر آرکیٹیکچر کو بیان کیا گیا ہے، جس میں پائیداری، ٹیسٹنگ کی بہتری کے لیے داخل کیا گیا تھا۔.

## منصوبہ بندی

اس ری میکنگ نے ابتدائی مونلیتھک ڈیزائن کے ساتھ کئی خدشات پر بات کی-

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## خدمت انجام دینا

### پس‌منظر‌دانوں کی مدد کرنا

**Responsibilities**:
- پائپ لائن لائف سائیکل انتظامیہ (اسمارٹ، تکمیل، غلطی دست یاب)۔
- سیماپور پر مبنی انفنٹری کنٹرول (انگریزی: Semaphore on contronomy stronomy stronomy) ہے۔
- سرور صدیقی (انگریزی:
- ذیلی کمیٹیوں کی طرف

**Does NOT contain**:
- ترجمہ منطق
- فائل/میں مخصوص فارمیٹ کے لئے
- دوبارہ منطق

### ملکوں میں ترقی

**Responsibilities**:
- ڈائریکٹری سے پڑھیں
- Synchronion country country country in the destable Dictionary
- مفقود ملک کے نام
- ترجمہ کے فوراً بعد ہر ہدف لغت محفوظ کریں

**Key behaviors**:
- اگر ڈیٹنگ زبان انگریزی ہے: ملک کے نام بطور-ایس کے محفوظ ہیں۔
- اگر کھوار زبان دیگر ہے: انگریزی نام سب سے پہلے کھوار زبان میں ترجمہ کیے گئے ہیں۔
- ہر زبان کو ایک دوسرے سے مختلف طریقے سے ترتیب دیا جاتا ہے

### مقامی طور پر نقل‌مکانی کرنا

**Responsibilities**:
- Decett نے/ایبٹ آباد کلیدوں کا موازنہ کر کے موجودہ ڈی بگ ڈکشنری کو سابقہ انفنٹریسوت سے کیا۔
- متناسقات:
- ہر ہدف زبان سے کلیدیں حذف کریں
- اگلے مقابلے کے لئے کچھ محفوظ کریں

**Key behaviors**:
- زبانی ترجمے ہمیشہ ترجیح دیتے ہیں ( زائد تحریریں)۔
- شامل کلیدیں ترجمہ کی جاتی ہیں اور فوری طور پر فی زبان محفوظ کی جاتی ہیں۔
- تنصیب کردہ کلیدیں فوری طور پر ختم کردی گئی ہیں
- Snaphot کو تمام زبانوں کی کامیابی کے بعد ہی محفوظ کیا جاتا ہے۔

### دستاویزوں کی تیاری

**Responsibilities**:
- سائیکل چلانے والے مارک ڈاؤن‌لوڈ جڑوں کو دوبارہ بحال کریں
- Dect نے SHA-256 ہیسے استعمال کرتے ہوئے ماخذ فائل تبدیل کر دی۔
- دائرۃ المعارف بریطانیکا آن لائن
- %s دوبارہ شروع کر نا
- ترجمہ کے بعد مرک ڈاؤن ترکیب
- ہر ہدف کی زبان کو محفوظ کریں

**Key behaviors**:
- بلاک کی سطح گرینویل: سمت، پیراگراف، فہرست چیزوں کا الگ الگ ترجمہ کیا جاتا ہے۔
- میتاداتا راستے جو بلاکس کو کامیاب بناتے / ہر زبان پر عبور رکھتے تھے۔
- ناکام بلاکوں کو دوبارہ نئے سرے سے چلنے والے کامیاب بلاک کے بغیر دوبارہ جاری کیا جاتا ہے۔
- قابلِ ذکر نقل و حمل شماری، فہرستوں، کوڈ بلاکس، وغیرہ۔

## دوبارہ کوشش

نظامت تین سطحوں پر رد عمل کرتی ہے:

### اوسط 1 — ایچ‌ٹی‌ٹی‌پی ( ایل‌ٹی‌ٹی‌ٹی‌ٹی‌ٹی‌ٹی‌ٹی‌ٹی‌ٹی‌ٹی )

- 5 تک انفنٹری بیکوف (1s, 2s, 3s, 4s, 5s) کے ساتھ کوشش کی۔
- نیٹ ورک ٹائم آؤٹ، 5xx غلطیوں اور عبوری ناکامیوں کی نشان دہی کرتا ہے۔
- ایچ‌ٹی‌ٹی‌پی کلائنٹ کی وضع‌قطع میں شامل

### وزن ۲ — سٹیج ( بریانی‌ڈی )

- 3 سلسلہ سہروردیہ تک 30 جلدوں کے ساتھ
- HTTP-level territories کے بعد مکمل ترجمے کی درخواست ختم ہوجاتی ہے۔
- اس سطح پر نصب شدہ ڈاک ٹکٹ اور بحالی کا اطلاق کیا جاتا ہے۔

### تیسرا مرحلہ — بلاک ( وقفہ‌شُدہ جگہ )

- انفرادی مارک ڈاؤن بلاکس جو ناکام ہوتے ہیں میٹادات میں نشان زدہ ہوتے ہیں۔
- اگلی پائپ پر خودبخود دوبارہ حل کریں
- کامیاب بلاک کبھی دوبارہ منسلک نہیں کیے جاتے ہیں۔

## ڈاٹ کام

### جے

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

### مارک ڈاؤن کا ترجمہ

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

### ملک کا نام

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

## ریاست مستقل

### غاروں میں

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incremental sync by tracking what was present in the previous run

### ہاشم فائلیں

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### ترجمہ

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Contents**:
  - ماخذ مواد
- پیر زبان بلاک حیثیت (انگریزی: Array of Booeans) ہے۔
- آخری تجدید اوقات
- **Purpose**: Enables partial re-translation of only failed blocks

### محفوظ کردہ ذخیرے

- **File**: `Locales/placeholders.json`
- **Contents**: Dictionary of keys to placeholder name-value pairs
- **Purpose**: Provides default values for named placeholders across the application

## سگنل آر آر اطلاع

### کششِ‌ثقل

علامہ اقبال کی طرف سے دیوان ترجمان خدمات:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### قابلِ‌اعتماد ضمانت

- ایک رُخ کے اندر پیغامات مستقل طور پر موجود ہوتے ہیں
- شرح نمبر منفرد فی شرح ہے۔
- کلیات (Clients) کی کمیت یا رد عمل معلوم کر سکتے ہیں۔

### ہب نقشہ

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## وسیع نکات

### ترجمہ کا نشانہ بنانا

1. ساتھ نیا ربط بنائیں
2. ڈومین نیم منطق کے ساتھ دوبارہ شروع کریں
3. ڈائری میں رجسٹر
4. تعمیراتی کام
5. حالیہ مراحل کے بعد دعوت دیں

### دوبارہ کوشش کریں

پروڈیوس کنندہ پیرامیٹرز:

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

### رنگ‌برنگی دُنیا

مقام تبدیل کرنے کے لیے متناسقات:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## مصر

### ایپیپس.json

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

### دوڑنا

پرنٹ
|---------|---------|--------|
80
10
3
30

## ٹیسٹ کا پلان

### غیر متصل امتحان

ہر ذیلی شعبے میں غیر واضح امتحانات ہوتے ہیں:

- کامیابی کی طرف قدم بڑھائیں/بچہ
- رپورٹ کی تصدیق کرنے کے لیے غلطی
- فائل/کے لئے عارضی ڈائریکٹری استعمال کریں O
- ہر زبان کو بچانے کے طریقے

### آزمائشوں کا سامنا

- مکمل پائپ لائنیں حقیقی (Local) لیبرے ٹرانسلیٹ مثال کے ساتھ چلتی ہیں۔
- متصل گاہکوں کے لیے خفیہ اشاروں کے پیغامات بھیجے جاتے ہیں۔
- ٹیسٹ ڈرائنگ رن (semaphore) چلاتی ہے۔
- ترجمہ کے بعد مرک ڈاؤن ترکیب

### فارغ التحصیل ٹیسٹ ہوئے۔

- اے آئی یا شیڈولر کے ذریعے ٹریگر ترجمہ کرتا ہے۔
- تمام نشان زدہ زبان کی فائلوں کو/ایپٹ کیا جاتا ہے۔
- چیک metadata فائلوں میں درست بلاک حالت موجود ہے۔
- ترجمے کے دوران تصدیق شدہ مقام کو محفوظ رکھا جاتا ہے۔

## غوروخوض

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 hashing and regex validation are fast relative to translation latency
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## Monolithic ڈیزائن سے ہجرت کی۔

اصل میں تمام منطق ایک درجن میں موجود تھی۔ ہجرت کا راستہ:

1. ملک منطقہ استعمال کریں
2. نکالیں جِلد
3. مارک ڈاؤن‌لوڈ منطقہ استعمال کریں
4. محفوظہ کھولیں
5. نیا منطقہ دوبارہ نکالیں
6. صرف کونسل کرنے کے لئے آسان

تمام سابقہ ترتیبات () غیر متصل رہیں۔ پائپ لائن کے پائپوں میں کوئی توڑ تبدیلی نظر نہیں آتی۔.
