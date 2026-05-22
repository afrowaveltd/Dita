# تغییرات در سرویس ترجمه خودکار

## بررسی اجمالی

این سند خلاصه ای از تمام تغییرات ایجاد شده در سرویس ترجمه خودکار Dita، از جمله بازسازی معماری، ویژگی های جدید، بهبود قابلیت نظارت و افزایش محلی سازی.

## تغییر معماری

### Refactored BackendTranslation Service

Monolithic به چهار سرویس تخصصی هماهنگ شده توسط یک ارکستر سبک تقسیم شده است:

- **BackendTranslation **Service - ارکستر خط لوله (اعتبار سرور، هیئت مدیره مرحله، مدیریت خطا)
- **CountriesTranslationService** — Country name synchronization (English → target language)
- **LocalizationTranslationService** — JSON dictionary synchronization (added/removed keys)
- **DocumentsTranslationService** — Markdown documentation translation with block-level tracking
- **SignalRPublisher** — Real-time progress reporting via SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### مزایای

- **Separation of concerns**: Each service handles a single translation domain
- **Maintainability**: Smaller classes are easier to understand and test
- **Extensibility**: New translation targets can be added via interface implementation
- **Reliability**: Independent services provide better fault isolation

## ویژگی های جدید

### نظارت بر ترجمه زنده

**Location **

یک صفحه مدیریت جدید که دید زمان واقعی را به خط لوله ترجمه ارائه می دهد:

- نمایش همه حوادث سیگنال را به عنوان آنها رخ می دهد
- انواع پیام های رنگی (Blue=started, Green=completed, Red=error)
- برچسب وضعیت اتصال با auto-reconnect
- شمارنده پیام و صادرات به JSON

### نام: Placeholder

سیستم محلی سازی در حال حاضر از صاحبان نام () برای بهبود گرامر در زبان های مختلف پشتیبانی می کند:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

ویژگی ها:
- ارزش های سهامدار ارائه شده در زمان اجرا یا ذخیره شده در
- ماسک زدن خودکار در طول ترجمه برای جلوگیری از فساد
- Backward سازگار با سهامداران فعلی

### ترجمه مقدماتی

فایل های مارک معکوس به صورت تدریجی ترجمه می شوند:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- **Selective retry**: Only failed blocks are re-translated on the next run
- **Metadata persistence**: Translation state survives application restarts

### Retry Logic

سه سطح انعطاف پذیری:

1. ** HTTP retry ** (LibreTranslateService): 5 تلاش برای بازگشت نمایی (1s-5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (DocumentsTranslationService): Failed Markdown blocks retried on next run

### گزارش سیگنالR

گزارش پیشرفت زمان واقعی برای تمام عملیات خط لوله:

- هر مرحله وقایع را منتشر می کند
- پیشرفت در زبان منتشر شده به عنوان حوادث
- حوادث خطا شامل متن دقیق (منبع، کد خطا، پیام)
- تضمین سفارش در هر اجرا

## تغییرات پیکربندی

### تنظیمات

هیچ تغییر شکستی وجود ندارد. پیکربندی موجود همچنان به کار ادامه می دهد:

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

### خدمات جدید

ثبت شده در:

- /
- `TranslationRetryService`
- /
- /
- /
- /

مرکز سیگنالR برای اتصالات مشتری نقشه برداری شده است.

## تست

### وضعیت آزمون

- **243 / 244 تست عبور ** (1 به دلیل دسترسی فایل همزمان در محیط آزمایش از بین رفته)
- پوشش تست جدید اضافه شده برای:
  - قابلیت های PlaceholderService
  - BackendTranslation ارکستر
  - JsonStringLocalizer Placeholder

### محدودیت های شناخته شده

- تست زمانی که به صورت موازی اجرا می شود، از بین می رود، زیرا چندین مورد آزمون همان فایل را به اشتراک می گذارند. هنگامی که در انزوا اجرا می شود.

## ساختار فایل جدید

### خدمات در

- ارکستر خط لوله
- نام کشور ترجمه
- هماهنگ سازی فرهنگ لغت JSON
- ترجمه های Markdown
- انتشار پیام سیگنالR
- – Retry Logic with Placeholder Masking
- رابط ناشر
- رابط خدمات کشور
- – Localization service interface
- رابط خدمات مستند
- رابط ارکستر (به روز رسانی)
- – Per-file Translation meta

### خدمات به روز رسانی در

- اضافه شده به نام حمایت از Placeholder
- به روز رسانی برای پارامتر جدید
- نام گذاری مدیریت Placeholder
- رابط کاربری Placeholder

### صفحه مدیریت جدید در

- صفحه نظارت بر زمان واقعی
- مدل صفحه

### مستندات جدید در

- - به روز رسانی مستندات خط لوله
- راهنمای سیستم Placeholder
- راهنمای استفاده از داشبورد
- - نمای معماری فنی

## بازگشت به توافق

همه تغییرات افزودنی هستند:

- کد محلی سازی موجود () بدون تغییر کار می کند
- قالب بندی موضعی () بدون تغییر کار می کند
- فرمت دیکشنری JSON بدون تغییر است
- ساختار فعلی Markdown بدون تغییر است
- پیام های سیگنال R از همان فرمت استفاده می کنند

## مسیر مهاجرت

نیازی به مهاجرت نیست. بازسازی داخلی است:

1. قدیمی به عنوان مرجع حفظ شد و سپس جایگزین شد
2. ثبت نام های DI برای استفاده از رابط های جدید به روز شدند
3. همه مصرف کنندگان موجود هیچ تغییری نمی بینند

## بهبود عملکرد

- **Reduced memory usage**: Files saved per-language immediately instead of holding all in memory
- ** افزایش تدریجی اجرا می شود ** فقط بلوک های مارک معکوس تغییر یافته / اصلاح شده دوباره ترجمه می شوند
- **Better visibility**: Real-time progress helps diagnose slow stages

## افزایش آینده

بهبود برنامه ریزی شده:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. ** تاییدیه مدیریت ** صفحات مدیریت محدود برای کاربران مجاز
3. **Dictionary editor** — Web UI for managing localization keys
4. ** آمار ترجمه ** نمودارها نشان دهنده میزان ترجمه و نرخ خطا در طول زمان
5. **Custom placeholder syntax** — Support for alternate placeholder formats

## تماس تلفنی

برای سوالات یا مسائل مربوط به خدمات ترجمه، لطفا به مستندات دقیق در دایرکتوری هر ماژول مراجعه کنید یا با تیم توسعه تماس بگیرید.
