# ترجمہ‌نگاروں کی خدمات

## نظر

اس دستاویز میں دیٹا خودکار ترجمے کی خدمت میں تمام تبدیلیاں کی گئی ہیں جن میں آرکیٹیکچر دوبارہ تعمیر کرنا ، نئی خصوصیات ، غیرمعمولی بہتری اور مقامی ترقی شامل ہیں ۔.

## آثارِقدیمہ کی تبدیلیوں

### اِس سے ظاہر ہوتا ہے کہ اُس کی زندگی میں بہتری آئی ہے ۔

مولوی صاحب کو چار انمول خدمات میں نامزد کیا گیا ہے جو ایک ہلکے وزن کے حساب سے کی گئی ہیں:

- **BackendTranslationService** — Pipeline orchestrator (server validation, stage delegation, error handling)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### فوائد

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## نئی معلومات

### ترجمہ‌نگار

**Location**: `/Admin/LiveTranslation`

ایک نیا اشتہارین صفحہ جو ترجمہ پائپ لائن میں حقیقی وقت کی بصیرت فراہم کرتا ہے:

- تمام اشاراتی واقعات کو جب وہ واقع ہوتے ہیں ظاہر کرتے ہیں۔
- رنگ-کوڈ پیغام کی اقسام (انگریزی: Construction=, Greene= مکمل، سرخ= دہشت گردی) ہیں۔
- اتصال کی حالت پہچاننے میں غلطی
- جے

### نام تبدیل کرنے والے

مقامی طور پر بننے والا نظام اب مختلف زبانوں میں بہتری لانے کے لیے اس کے نام سے مرتب کردہ مقام کی حمایت کرتا ہے ():

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

معلومات:
- ریکی یا محفوظ کرنے پر فراہم کی جانے والی اقدار
- ترجمہ کے دوران ترجمہ کے دوران میں خودکار نقاب لگانا / کرپشن کو روکنے کے لیے
- موجودہ کھڑے کھڑے مقام کے ساتھ پشتون تعلقات

### ترجمہ

مارک ڈاؤن فائلوں کا ترجمہ انورٹر میں کیا جاتا ہے:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### حقیقت‌پسندی

مندرجہ ذیل تین درجے:

1. **HTTP retry** (LibreTranslateService): 5 attempts with exponential backoff (1s–5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### اشاروں کی رپورٹ

تمام پائپ لائن آپریشنز کے لئے حقیقی وقتی پیش رفت

- ہر مرحلہ واقعات کو نشر کرتا ہے۔
- پری زبان کی ترقی بطور واقعات شائع ہوئی۔
- خامی واقعات میں تفصیلی سیاق و سباق (source, غلطی کوڈ، پیام) شامل ہیں۔
- قابلِ استعمال اعداد و شمار ہر دوڑ کے اندر ترتیب دینے کی ضمانت دیتے ہیں۔

## تبدیلیاں

### ایپیپس.json

کوئی توڑ تبدیل نہیں. پرورش کا کام جاری ہے:

```json
{
  "AutomaticTranslationSettings": {
    "DefaultLanguage": "en",
    "IgnoredLanguages": ["auto", "detect"],
    "MarkdownRoots": ["/Docs"],
    "AutomaticRun": true,
    "CheckingPeriod": 30,
    "WaitingTime": "00:00:00"
  }
}
```

### نئی خدمت

درج ذیل :

- /
- `TranslationRetryService`
- /
- /
- /
- /

نشان شناخت کنندہ اتصال کے لیے متعین کیا گیا ہے.

## جانچ

### جانچ سٹیٹس

- **243/244 tests passing** (1 skipped due to concurrent file access in test environment)
- نئے ٹیسٹ کوریج نے مزید کہا:
  - ترقی‌پسندانہ کام
  - مَیں نے اُن سے کہا : ” مَیں نے . .
  - Json Straring settlester indexers

### معلومات

- ٹیسٹ جمع ہوتا ہے جب ہم برابر چلاتے ہیں کیونکہ کئی ٹیسٹ حالات ایک ہی فائل کو شیئر کرتے ہیں۔ یہ علیحدگی میں دوڑنے پر گزرتا ہے۔.

## نیا فائل رولر

### خدمت

- — پی‌پی‌پی‌پی‌لی‌ی‌ڈی کی نقل
- — ملک میں ترجمہ
- — جے
- — ترجمہ
- — نشانل پیغام نشر کرنا
- — جگہ جگہ پر نقاب‌ریزی کیساتھ منطقی منطق
- — ⁠ حقیقت یہ ہے کہ انسان کا نظام
- — ملک کی خدمت کے ترتیبات
- — مقامی طور پر خدمت میں تبدیلی
- — دستاویزی سروس مواجہ —
- — اُصولوں کے مطابق
- — پیر وارثی ترجمہ مسعوداتا –

### یہوواہ کے گواہوں کے طور پر خدمت

- — نامزد مقام حمایت —
- — نئے پیرامیٹر کے لیے تجدید شدہ
- — نامزد مقام انتظامیہ —
- — جگہ جگہ

### نیا ناظم صفحہ

- — حقیقی وقت کی نگرانی صفحہ
- — صفحہ:

### نئی دستاویزات

- — نسخہ‌جات —
- — سٹیج سسٹم گائیڈ
- — ڈاک ٹکٹ کا استعمال ہدایت کار
- فن‌لینڈ — فنِ‌تعمیر کی بابت

## پُرآسائش کمپنیاں

تمام تبدیلیوں کا اضافہ کر رہے ہیں:

- جگہ جگہ بنانے کا عمل
- جگہ جگہ
- جے
- مارک ڈاؤن‌لوڈ کی تعمیر غیرمعمولی ہے
- اشاراتی رے پیغامات اسی شکل کا استعمال کرتے ہیں۔

## ہجرت راہ

ہجرت کی ضرورت نہیں۔ بازنطینی اندرونی ہے:

1. پرانے کو بطور حوالہ محفوظ کیا گیا اور بعد میں اس کی جگہ لے لی گئی۔
2. نئے ٹرمینل استعمال کرنے کے لیے ڈی آئی رجسٹریشن کی تجدید کی گئی۔
3. دیکھنے کے تمام پہلے صارفین کوئی تبدیلی نہیں کرتے

## پرفارمنس سروسز

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- **Faster incremental runs**: Only changed/failed Markdown blocks are re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## مستقبل کے معاملات

نتیجہ:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. **Admin authentication** — Restrict admin pages to authorized users
3. **Dictionary editor** — Web UI for managing localization keys
4. **Translation statistics** — Charts showing translation counts and error rates over time
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## رابطہ

سوالات یا ترجمے کی خدمت سے متعلق مسائل کے لیے ہر مدارس کی ڈائریکٹری میں تفصیلی دستاویزات یا ترقیاتی ٹیم سے رابطہ کرنے کے لیے رجوع کریں۔.
