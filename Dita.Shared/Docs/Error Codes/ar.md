# رموز الأخطاء

Dita uses a **range-partitioned, unified error code architecture** that provides both domain-specific enums and a single catch-all `ErrorCode` type. Every error in the system — from network failures to disk I/O, from authentication to configuration — is represented by a member of this hierarchy.

## الهندسة

### مخصصات هامشية

النطاق
|-------|----------|----------|
1000-1999
2000-2999
٣٠٠٠ - ٣٩٩
٠٠٠ ٤-٤٩٩
5000-5999
6000-6999
٧٠٠٠-٧٩٩
٠٠٠ ٨-٩٩٩
٩٠٠٠-٩٩٩

### النمط المزدوج

ويُمثَّل كل مجال من مجالات الأخطاء** على حدة** نُظمة فرعية مركَّزة (مثلاً) ومدخلات في الضميمة الموحدة. وتستعمل الندوات الفرعية أسماء عارية؛ وتُحدّد الأسماء المجمّعة بالفئة التالية:

```csharp
// Sub-enum — domain-specific context
NetworkError.ConnectionRefused    // 1003

// Unified enum — catch-all reference
ErrorCode.NetworkConnectionRefused  // 1003 — same numeric value
```

This allows code to work with domain-specific types when the context is known, while also supporting general error handling that works across all domains.

### sentinel

وتُعرّف كل وحدة فرعية بأنها القيمة الأساسية لنطاقها (مثلاً). وتعترف الطريقة بذلك وتعود.

## درجة الشرف

The enum consolidates all sub-enum values into a single type with **non-overlapping** integer ranges. وتقدم الفئة الساكنة الرفيقة الإنسانية:

```csharp
// Get human-readable text for any error code
string text = ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused);
// → "Network connection refused"

string text2 = ErrorCodeText.ErrorText(5005);
// → "Localization invalid translation format"
```

### منطق الإنسانية

(أ) اتباع نهج الاتفاقية فيما يتعلق بالثقة:

1. "أسماء "باسكال كاسي تقسم إلى كلمات عن طريق "ريجكس
2. يتم تطبيع الأسماء المستعارة المعروفة (Io , I/O, Api , API, Dns → DNS, Htp → HTTP, Ssl , SSL, Mfa , MFA, OAuth ) OAuth, Sso ) SSO, Xml → XML, Json ) JSON, Url URL)
3. All-caps tokens (e.g.) are preserved
4. القيمة تنتهي في المقابل

## الوحدات الخاصة

### الشبكة )٠٠٠ ١-١٩٩٩(

وتشمل الدونيات، والقطع، والوكلاء، والبوابات، وتتبع أخطاء البروتوكول، والربط، وطلب مشاكل دورة الحياة.

الأعضاء الملحوظون
|---|---|
1000
1001
1002
1003
1004
1005
1006
1007
1008
1009
1010
1019
1020
1021

### المخزن )٢٠٠٠-٢٩٩٩(

Covers database connections, transactions (commit/rollback/timeout), integrity (constraints, deadlocks, foreign keys), schema management, support/restore, replication, and quota.

الأعضاء الملحوظون
|---|---|
2000
2003
2004
2007
2010
2012
2013
2018
2023
2029

### المشغل )٣٠٠٠-٩٩٣(

Covers low-level physical disk and drive errors: bad sectors, SMART failures, RAID degradation, partition tables, equipment failures, mount/unmount, format, and eject operations.

الأعضاء الملحوظون
|---|---|
3000
3001
3010
3012
3021
3027
3032

### محل بيع الملفات )٤٠٠٠-٤٩٩٩(

أخطاء تشغيل نظام الملفات: الوصول/البعثة، وقفل الملفات، والضغط/الضغط/الضغط/التشفير/التشفير، والمسائل المتعلقة بالمسار، والروابط الرمزية، وتقاسم الانتهاكات، والعمليات العامة للمنظمة.

الأعضاء الملحوظون
|---|---|
4000
4001
4013
4011
4023
4024
4028

### موظف محلي )٥٠٠٠-٥٩٩(

وتشمل الأخطاء الخاصة بخط الأنابيب المحلي: القاموس، والتشريد، والتحقق المحلي، والاستمارات التعددية، ونسخ الترجمة الخارجية (التاريخ، والتوافر، والسؤال، والتوقيت) وشكل الخيوط.

الأعضاء الملحوظون
|---|---|
5000
5001
5007
5014
5015
5016
5018

### AuthenticationError (6000-6999)

Covers authentication and authorization: accreditation, tokens (refresh/access), sessions, MFA/2FA, biometrics, certificates, OAuth, SSO, and account states (disabled, expired, locked).

الأعضاء الملحوظون
|---|---|
6000
6001
6004
6015
6024
6026

### ValidationError (7000-7999)

يشمل التحقق من صحة المدخلات: التحقق من الشكل (البريد الإلكتروني، الهاتف، اليرل، json، xml، وقت التأريخ)، وقيود النطاق/الطول، وإخفاقات التحويل، والمجالات المطلوبة، والنمط/الregex، وتعقيد كلمة السر.

الأعضاء الملحوظون
|---|---|
7000
7003
7016
7018

### )٨٠٠٠-٩٩٩(

تشكيلة المكوِّنات والأماكن: الوصول إلى الملفات، والفرز، والمصادقة، والأسرار/خزنة المفاتيح، وسلاسل الاتصال، وأجهزة الاستدلال الذاتي، وأعلام المعالم، والمتغيرات البيئية، وضغوط الكيماوي/التحويل.

الأعضاء الملحوظون
|---|---|
8000
8001
8016
8019

### GeneralError (9000-99)

Catch-all for application-wide errors: memory, concurrency, licensing, rate limiting, reading, resource management, feature support, and unhandled exceptions.

الأعضاء الملحوظون
|---|---|
9000
9004
9007
9015
9014

## الأنابيب

### العملية

يحدد المراحل التسلسلية من خط الترجمة الآلي:

القيمة
|-------|------|-------------|
صفر
1
2
3
4
5

### مركب محلي

نوع من رسالة الزمن الحقيقي التي انبثقت عن خط الأنابيب:

القيمة
|-------|------|---------|
صفر
1
2
3
4
5
6

### الترجمة التحريرية الهدف

يحدد نوع المحتوى الذي يترجم:

القيمة
|-------|------|---------------|
صفر
1
2

### عبارة

تعقّب دولة التغيير المشابهة للطيور القاموسية:

القيمة
|-------|------|
صفر
1
2
3

### المقارنة

مشغلو المقارنات المستخدمون في تقييم/تقييم القيم:

القيمة
|-------|------|----------|
صفر
1
2
3
4
5
6

### نوع الجنس

نوع الجنس التخرجي/الاجتماعي للتمركز:

القيمة
|-------|------|
صفر
1
2
3

## استخدام رموز الخطأ

### التقارير قيد التنفيذ

تُنقل أخطاء الترجمة في السجلات:

```csharp
public class TranslationError
{
    public string Source { get; set; }        // Language code, file path, or stage name
    public ErrorCode Code { get; set; }       // Unified error code
    public string ErrorMessage { get; set; }  // Human-readable text
}
```

### في الردود

```csharp
var result = Response<TranslateResult>.Fail(
    ErrorCodeText.ErrorText(ErrorCode.NetworkConnectionRefused),
    ErrorCode.NetworkConnectionRefused);
```

### إضفاء الطابع الإنساني على أي رمز

```csharp
// From enum value
string text = ErrorCodeText.ErrorText(ErrorCode.StorageDeadlockDetected);
// → "Storage deadlock detected"

// From raw integer (validates against defined values)
string text2 = ErrorCodeText.ErrorText(2010);
// → "Storage deadlock detected"

// Undefined code
string text3 = ErrorCodeText.ErrorText(99999);
// → "Unknown error (99999)"
```
