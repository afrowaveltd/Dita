# داشبورد ترجمه زنده

داشبورد ترجمه زنده یک صفحه مدیریت است که دید زمان واقعی را به خط لوله ترجمه خودکار ارائه می دهد. این اتصال به مرکز سیگنالR و نمایش همه حوادث خط لوله به عنوان آنها رخ می دهد.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## ویژگی های

### جریان رویداد در زمان واقعی

تمام رویدادهای سیگنالR از خط لوله ترجمه در یک جدول زنده نمایش داده می شوند:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID ** کوتاه مدت GUI برای همبستگی
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- ** نوع ** نشان پیام (StageStarted، Progress، Stagecompleted و غیره)
- ** پیام ** توصیف قابل خواندن انسان
- **Details** — Full JSON payload of the event data

### برنامه نویسی رنگی

رنگ رنگی
|-------|---------|
آبی ()
سبز ()
قرمز ()
سفید (شکست)

### وضعیت اتصال

یک بنر وضعیت در نمایش های بالا:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

این اتصال از اتصال اتوماتیک با backoff نمایی استفاده می کند: 0s، 2، 5s، 10s، 30s.

### کنترل

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON ** دانلود تمام پیام های دریافت شده به عنوان فایل JSON برای تجزیه و تحلیل
- ** پیام ضد ** نشان می دهد تعداد کل حوادث دریافت شده در این جلسه

## سیگنالR Hub

داشبورد متصل به:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### قرارداد پیام

```typescript
interface LocalizationHubMessage {
    runId: string;        // Guid
    sequence: number;     // long
    type: LocalizationMessageType;
    stage: ProcessStage;
    timestampUtc: string; // ISO 8601
    isError: boolean;
    message: string;
    data: object | null;
}
```

### انواع رخداد

داشبورد همه ارزش ها را مدیریت می کند:

نوع
|------|---------|
Blue نشان
نشان سبز
نشان قرمز
نشان سبز
نشان قرمز
برچسب های Info
هشدار

## پیاده سازی فنی

### بازگشت

- **LocalizationHub ** () – SignalR Hub که پیام ها را به تمام مشتریان متصل می کند
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher ** پیاده سازی پیش فرض که یک توالی تکتونیک را افزایش می دهد و پخش می کند

### Frontend

- HTML /JS با مدل بوت استرپ 5
- استفاده از کتابخانه مشتری Microsoft SignalR JavaScript ( بارگذاری شده از CDN)
- هیچ ارائه دهنده سمت سرور مورد نیاز برای تغذیه رویداد

### ساختار صفحه

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## استفاده در هنگام توسعه

1. Dita را شروع کنید. Server Application
2. حرکت به سمت
3. یک ترجمه را اجرا کنید (یا منتظر برنامه نویس باشید یا API را فراخوانی کنید)
4. مشاهده حوادث در زمان واقعی
5. از دکمه صادرات برای ثبت یک رد کامل برای اشکال زدایی استفاده کنید

## پیشرفت های آینده

بهبود برنامه ریزی شده برای داشبورد:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration ** ویرایش مستقیم از داشبورد
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## عیب یابی

### داشبورد نشان می دهد "Failed toconnect"

1. بررسی سرور در حال اجرا و در دسترس است
2. بررسی کنسول مرورگر برای CORS یا خطای شبکه
3. تایید در
4. اطمینان حاصل کنید که هیچ فایروال اتصالات WebSocket را مسدود نمی کند

### حوادث ظاهر نمی شوند

1. بررسی کنید که آدرس URL سیگنالR بین سرور () و مشتری () مطابقت دارد
2. بررسی برنامه ریزی شده در
3. بررسی ورود سرور برای خطاهای خط لوله ترجمه
4. بررسی برگه شبکه مرورگر برای پیام های WebSocket

### پیام ها از دستور خارج می شوند

این زمینه تضمین سفارش در یک اجرای واحد. اگر پیام ها از دستور ظاهر شوند، ممکن است نشان دهد:
- خط لوله چندگانه همپوشانی دارد (با توجه به قفل semaphore اتفاق نمی افتد)
- موضوعات ارائه دهنده مرورگر (سعی کنید صفحه را بازسازی کنید)
