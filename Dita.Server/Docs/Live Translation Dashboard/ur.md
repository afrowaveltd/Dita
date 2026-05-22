# بائبل ترجمے کی بورڈ

لائیو ترجمہ داش بورڈ ایک اشتہاری صفحہ ہے جو خودکار ترجمہ پائپ لائنوں میں حقیقی وقت کی نگرانی فراہم کرتا ہے۔ یہ سائنل ربڑ سے منسلک ہوتا ہے اور جب وہ واقع ہوتے ہیں تو تمام پائپ لائن واقعات دکھاتا ہے۔.

## عرفیت

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## محفوظات

### حقیقی وقت آنے والی نہر

ترجمہ پائپ لائن سے تمام سگنل واقعات زندہ میز میں دکھائے جاتے ہیں:

- **Sequence number** — Monotonic counter within each pipeline run
- **Timestamp** — Local time when the event was received
- **Run ID** — Shortened GUID for correlation
- **Stage** — Pipeline stage badge (CheckServers, TranslateCountries, etc.)
- **Type** — Message type badge (StageStarted, Progress, StageCompleted, etc.)
- **Message** — Human-readable description
- **Details** — Full JSON payload of the event data

### رنگوں کا رنگ

رنگ
|-------|---------|
( ب )
سبز
سُرخ
سفید رنگ

### اتصال کی حالت

اوپر کی جانب ایک حیثیت بینر:
- **Connecting** — Establishing SignalR connection
- **Connected** — Receiving events normally
- **Reconnecting** — Connection lost, attempting to reconnect
- **Disconnected** — Connection closed

رابطہ خودکار طور پر استعمال ہوتا ہے جس میں برقی رو (constantial backff) استعمال ہوتا ہے : 0s, 2s, 5s, 10, 30s)۔.

### بند

- **Clear Feed** — Removes all displayed messages and resets the counter
- **Export JSON** — Downloads all received messages as a JSON file for analysis
- **Message counter** — Shows total number of events received in this session

## اشاروں کی دُنیا

نیٹ ورک منیجر سے متصل ہیں:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### پیام کا معاہدہ

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

### واقعاتی نوعیت

تمام قدروں کو ہینڈل کرتا ہے:

قسم
|------|---------|
نیلا رنگ
سبز رنگ
سرخ رنگ
سبز رنگ
سرخ رنگ
معلومات
ڈر سنانے والوں کو

## تکنیکی عمل

### فولڈرز

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** — Abstraction over the hub for use in translation services
- **SignalRPublisher** — Default implementation that increments a monotonic sequence and broadcasts

### مخالفت

- بوٹسٹرپ 5 اسکیلنگ کے ساتھ خالص ایچ ٹی ایم ایل/JS
- مائیکروسافٹ سگنل آر جاوا ایسکریپٹ کلائنٹ لائبریری (سی ڈی این سے لی گئی) استعمال کرتا ہے۔
- تقریب کے لیے سرور جانبداری کا کوئی ذریعہ نہیں

### صفحہ ۱۹

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## ارتقا کے دوران استعمال ہوتا ہے۔

1. دیتا کا آغاز کیا۔ سرور اطلاقیے
2. جگہ
3. ٹریگر ایک ترجمہ جاری کرتا ہے (کوئی شیڈولر کا انتظار نہیں کرتا یا اے پی آئی کہلاتا ہے۔
4. حقیقی وقت میں نظر آنے والے واقعات
5. گر تے هو ئے رنگ کے ليے پورا تختے پر قبضہ کر نے کے ليے بٹن کا استعمال کريں

## مستقبل کے واقعات

ترقی پذیری کے لیے منصوبہ بندی:

- **Authentication** — Restrict access to users with the `Admin` role
- **Filtering** — Filter events by stage, type, or run ID
- **Historical runs** — View completed runs from a database or log file
- **Statistics** — Charts showing translation counts, error rates, and latency over time
- **Manual triggers** — Buttons to manually start specific pipeline stages
- **Configuration** — Edit `AutomaticTranslationSettings` directly from the dashboard
- **Language management** — View and edit supported languages
- **Dictionary preview** — Browse and search localization dictionaries

## مشکلات

### داس بورڈ "مشت " کو ملانے کے لیے "فیض" دکھاتا ہے۔

1. سرور کو چلانے اور رسائی حاصل ہے۔
2. کروس یا نیٹ ورک غلطیوں کے لیے صارف کی چیک کریں
3. تصدیق میں موجود ہے۔
4. نیٹ ورک منیجر بند نہیں ہے

### واقعات سامنے نہیں آ رہے ہیں

1. چیک کریں کہ سرور (ع) اور کلائنٹ (علاقہ) کے درمیان میں اشارۃً ربط
2. شیڈول بنانے والا قابل ہوتا ہے۔
3. ترجمہ پائپ لائن غلطیوں کے لیے سرور لاگس دیکھیں
4. ویب سوcket پیغامات کے لیے براڈ بینڈ نیٹ ورک ٹیبل چیک کریں

### پیغامات ترتیب سے خارج ہیں۔

میدان ایک رن کے اندر ہدایات دیتا ہے۔ اگر ترتیب سے پیغامات ظاہر ہوں تو یہ اس بات کی نشان دہی کر سکتا ہے کہ:
- متعدد پائپ لائنوں پر زیادہ سے زیادہ چلا جاتا ہے (سیماپور لاک کی وجہ سے نہیں ہونا چاہیے)۔
- بریسر مسائل ( صفحے کو تازگی بخشتی ہے)۔
