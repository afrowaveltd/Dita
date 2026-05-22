# لوحة لترجمة مباشرة

The Live Translation Dashboard is an admin page that provides real-time visibility into the automatic translation pipeline. إنه يتواصل مع مركز الإشارة ويعرض جميع أحداث خط الأنابيب عند حدوثها.

## URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## المعالم

### مسار الأحداث في الوقت الحقيقي

وتُعرض جميع أحداث الإشارة من خط الترجمة التحريرية في جدول مستكمل:

- ** العدد المتعاقب** - منضدة متنقلة داخل كل خط من خطوط الأنابيب
- ** Timestamp** - Local time when the event was received
- ** هوية راون** - مختصرة من طراز GUID لربطها
- **Stage** - Pipeline stage card (CheckServers, TranslateCountries, etc.)
- ** Type** - Message type card (Stagestarted, Progress, StageCompleted, etc.)
- **Message** - Human-readable description
- ** بيانات مفصلة** - حمولة كاملة لبيانات الحدث

### الترميز باللون

اللون
|-------|---------|
الأزرق ()
غرين ()
أحمر ()
أبيض (قصير)

### حالة الارتباط

لافتة مركز في أعلى العروض:
- ** الاتصال** - إنشاء وصلة الإشارة
- ** متصلون** - أنشطة استقبال عادة
- ** الوصل** - الانتصاب المفقود، محاولة إعادة الاتصال
- ** متصل** - أغلق الاتصال

The connection uses automatic reconnect with exponential backoff: 0s, 2s, 5s, 10s, 30s.

### الضوابط

- ** Clear Feed** - Removes all displayed messages and resets the counter
- **Export JSON** - Downloads all received messages as a JSON file for analysis
- ** مكافحة التسمية** - يبين العدد الإجمالي للأحداث التي وردت في هذه الدورة

## مركز الإشارة

ويربط لوحة المتابعة بما يلي:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### عقد الرسالة

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

### أنواع الأحداث

اللوحة تُعالج جميع القيم:

النوع
|------|---------|
الشارة الزرقاء
الشارة الخضراء
الشارة الحمراء
الشارة الخضراء
الشارة الحمراء
شارة المعلومات
شارة تحذير

## التنفيذ التقني

### الخلفية

- ** LLocalizationHub** () - SignalR hub that broadcasts messages to all connected clients
- ** ناشر رئيسي** - موقف بشأن المركز لاستخدامه في خدمات الترجمة التحريرية
- **SignalRPublisher** - Default implementation that increments a monotonic sequence and broadcasts

### الجبهة

- نقي html/js with bootstrap 5 styling
- يستعمل مكتبة عملاء ميكروسوفت سينالر جافاسكريبت (المحملة من CDN)
- لا حاجة إلى جانب الخادم لإطعام الحدث

### الهيكل الصفحة

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## الاستخدام أثناء التنمية

1. ابدأي (ديتا) طلب خدمة
2. الملاحة إلى
3. :: إجراء عملية ترجمة (إما انتظار الجدول الزمني أو الاتصال بالمبادرة)
4. مشاهدة الأحداث تظهر في الوقت الحقيقي
5. استخدام زر التصدير لالتقاط أثر كامل للتدمير

## التحسينات المقبلة

التحسينات المخطط إدخالها على لوحة المتابعة:

- ** التوثيق** - تقييد الوصول إلى المستعملين الذين لهم دور
- ** Filtering** - Filter events by stage, type, or run ID
- ** فحوصات متحركة** - تم تشغيلها من قاعدة بيانات أو ملف سجل
- ** الإحصاءات** - المواد التي تبين حسابات الترجمة التحريرية، ومعدلات الخطأ، والتساهل على مر الزمن
- ** محفزات مانية** - بوتون لبدء مراحل خط أنابيب محددة يدويا
- ** التنظيم** - الإصدار مباشرة من لوحة المتابعة
- ** إدارة اللغات** - الرؤية والتحرير اللغات المدعومة
- ** استعراض أولي وبحثي**

## الاضطرابات

### (داش لوت) يُظهر "مُقنع بالتواصل"

1. التحقق من الخادم يجري ويسهل الوصول إليه
2. كشغّل مصفّح لشركات CORS أو أخطاء الشبكات
3. تم تأكيد وجوده
4. التأكد من عدم قيام أي جدار لإطلاق النار بحجب الاتصالات عبر الشبكة

### الأحداث لا تظهر

1. تحقق من أن مركز الإشارة يطابق بين الخادم () والزبون ()
2. تحقق من الجدول الزمني
3. انظر إلى سجلات الخواديم لأخطاء خط أنابيب الترجمة
4. دفتر شبكه مصفوفه عن رسائل

### الرسائل خارج النظام

الحقل يضمن النظام في ركض واحد وإذا ظهرت الرسائل خارج النظام، يمكن أن تشير إلى ما يلي:
- تداخل خطوط الأنابيب المتعددة (لا يمكن أن يحدث بسبب القفل المناعي)
- قضايا (إعادة تنشيط الصفحة)
