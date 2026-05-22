# معماری ترجمه

این سند معماری مدولار سیستم ترجمه خودکار Dita را توصیف می کند که برای بهبود قابلیت نگهداری، آزمون پذیری و انعطاف پذیری معرفی شده است.

## اهداف طراحی

بازسازی چندین نگرانی را با طراحی منحصر به فرد اصلی بیان کرد:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- **Incremental persistence**: Files are saved per-language immediately after translation, reducing memory usage and providing earlier results.
- **Resilience **: سطوح متعدد retry بدون مسدود کردن کل خط لوله، شکست های گذرا را اداره می کنند.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Extensibility**: New translation targets can be added by implementing a single interface.

## سرویس decomposition

### BackendTranslation Service (orchestrator)

**Responsibilities**:
- مدیریت چرخه عمر (شروع، تکمیل، مدیریت خطا)
- کنترل ارز مبتنی بر Semaphore (پیش از اجراهای همپوشانی)
- اعتبار سرور (اعتبار، دسترسی به زبان، پیکربندی)
- درخواست به خدمات فرعی

**Does NOT contain**:
- منطق ترجمه
- فایل I/O برای فرمت های خاص
- بازگشت منطق

### آموزش کشورها

**Responsibilities**:
- Read from directory
- نام های کشور را به فرهنگ لغت محلی به طور پیش فرض تبدیل کنید
- ترجمه نام کشور گمشده در هر زبان مقصد
- صرفه جویی در هر فرهنگ لغت بلافاصله پس از ترجمه

**Key behaviors**:
- اگر زبان پیش فرض انگلیسی باشد: نام های کشوری که به عنوان زبان انگلیسی ذخیره می شوند
- اگر زبان پیش فرض دیگر است: نام انگلیسی به زبان پیش فرض ترجمه شده است
- هر زبان به طور مستقل با حلقه retry خود پردازش می شود

### آموزش محلی

**Responsibilities**:
- کلید های اضافه شده / متحرک با مقایسه فرهنگ لغت به طور پیش فرض فعلی با عکس فوری قبلی
- ترجمه کلید اضافه شده در هر زبان هدف
- حذف کلید های حذف شده از هر زبان هدف
- پس انداز فوری برای مقایسه بعدی

**Key behaviors**:
- ترجمه های دستی همیشه اولویت دارند (هرگز بیش از حد نوشته نشده)
- کلید های اضافه شده ترجمه و ذخیره شده در هر زبان بلافاصله
- کلید های حذف شده بلافاصله حذف می شوند
- Snapshot تنها پس از اتمام موفقیت تمام زبان ها ذخیره می شود

### انتقال اسناد

**Responsibilities**:
- پیاده روی ریشه های مارک معکوس پیکربندی شده
- Detect فایل های منبع را با استفاده از هش SHA-256 تغییر داد
- پیگیری وضعیت ترجمه هر بلوک در
- ترجمه بلوک به بلوک با هر بلوک retry
- ساختار مارک معکوس پس از ترجمه
- ذخیره هر فایل زبان هدف به طور مستقل

**Key behaviors**:
- دانه های سطح بلوک: سرفصل ها، پاراگراف ها، موارد لیست به طور جداگانه ترجمه می شوند
- متاداده ردیابی می کند که بلوک های موفق / شکست در هر زبان
- بلوک های شکست خورده در اجرای بعدی بدون تغییر بلوک های موفق
- اعتبار ساختار تضمین می کند اعداد عنوان، لیست ها، بلوک های کد و غیره منبع مطابقت

## استراتژی Retry

این سیستم در سه سطح مجددا ثبت می کند:

### سطح 1 - HTTP (LibreTranslateService)

- تا 5 تلاش برای بازگشت نمایی (1، 2، 3s، 4، 5s)
- مدیریت زمان بندی شبکه، خطای 5xx و شکست های گذرا
- ساخته شده در پیکربندی HTTP client

### سطح 2 - مرحله (TranslationRetryService)

- حداکثر 3 تلاش برای تاخیر 30 ثانیه
- Re-drives کل درخواست ترجمه پس از retries سطح HTTP خسته
- ماسک و بازسازی محل در این سطح اعمال می شود

### سطح 3 - Block (DocumentsTranslation Service)

- بلوک های مارک معکوس فردی که شکست می خورند در متاداده مشخص می شوند
- بازگشت به طور خودکار در خط لوله بعدی
- بلوک های موفق هرگز دوباره ترجمه نمی شوند

## جریان داده ها

### ترجمه فرهنگ لغت JSON

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

### ترجمه های Markdown

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

### نام کشور ترجمه

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

## تداوم دولت

### تصاویر

- **JSON ** ذخیره شده در یک فایل در کنار دیکشنری پیش فرض (نام با ارائه دهنده ذخیره سازی متفاوت است)
- **Purpose **: ایجاد همگام سازی افزایشی با ردیابی آنچه در اجرای قبلی وجود داشت

### فایل های Hash

- **Markdown**: `{sourceFile}.hash.json` next to the source file
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### ترجمه متن ترجمه

- **Markdown **
- ** محتواها **
  - منبع محتوا هش
- وضعیت بلوک Per-language (آرزوی بولیans)
- آخرین بروزرسانی Timetamp
- **Purpose**: Enables partial re-translation of only failed blocks

### ذخیره سازی

- ** فایل **
- ** محتواها **: فرهنگ لغت کلیدها برای جفت های ارزش نام سهامدار
- **Purpose**: Provides default values for named placeholders across the application

## گزارش سیگنالR

### ناشر

خدمات ترجمه سیگنالR از ویژگی های:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### تضمین

- پیام ها در یک اجرای واحد به صورت تکتونی
- اعداد تفاوت منحصر به فرد از طریق
- مشتریان می توانند شکاف ها یا سفارش مجدد را تشخیص دهند

### نقشه برداری Hub

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## امتیازات

### اضافه کردن یک هدف ترجمه جدید

1. ایجاد یک رابط جدید با
2. پیاده سازی رابط با منطق خاص دامنه
3. ثبت نام در DI container
4. ورود به سازنده
5. تماس از مراحل موجود

### سیاست مقدماتی

پارامترهای سازنده Override:

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

### گزینه Custom Placeholder

پیاده سازی برای تغییر نحو یا ذخیره سازی سهامداران:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## پیکربندی Configuration

### تنظیمات

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

### زمان تنظیم

تنظیم
|---------|---------|--------|
80
10
3
30

## استراتژی تست استراتژی

### تست های واحد

هر سرویس فرعی به طور مستقل قابل آزمایش است:

- Mock برای شبیه سازی موفقیت / شکست
- Mock برای تأیید گزارش
- استفاده از دایرکتوری های موقت برای فایل I / O
- بررسی رفتار صرفه جویی در زبان

### تست های ادغام

- خط لوله کامل با نمونه واقعی (local) LibreTranslate اجرا می شود
- پیام های سیگنال R به مشتریان متصل تحویل داده می شوند
- تست پیشگیری همزمان (semaphore)
- ساختار مارک معکوس پس از ترجمه

### تست های پایان به پایان

- ترجمه از طریق API یا زمانبندی
- بررسی تمام فایل های زبان هدف ایجاد شده و به روز شده
- فایل های متاداده حاوی وضعیت بلوک صحیح هستند
- تأیید کنندگان در سراسر ترجمه ها حفظ می شوند

## ملاحظات عملکردی

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Metadata files add small overhead but enable incremental work
- **شبکه ** پردازش دقیق با ترتلینگ مانع از انتقال شدید Libre می شود
- **CPU ** هش کردن و اعتبار مجدد هش SHA-256 به سرعت نسبت به تاخیر ترجمه است
- **SignalR**: Lightweight messages, no payload compression needed for typical reports

## مهاجرت از طراحی یکپارچه

اصل شامل تمام منطق در یک کلاس بود. مسیر مهاجرت:

1. منطق کشور را استخراج کنید
2. منطق استخراج JSON
3. استخراج منطق Markdown
4. انتشار سیگنال R
5. استخراج مجدد منطق
6. ساده سازی ارکستر به تنها هیئت مدیره

تمام رابط های موجود () بدون تغییر باقی می مانند. مصرف کنندگان این خط لوله هیچ تغییرات شکستنی نمی بینند.
