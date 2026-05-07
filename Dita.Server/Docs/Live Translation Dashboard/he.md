# תגית: Dashboard

Live Translation Dashboard הוא דף ניהול המספק חשיפה בזמן אמת לתוך צינור התרגום האוטומטי. הוא מתחבר למרכז אותות ומציג את כל אירועי הצינור כפי שהם מתרחשים.

## כתובת URL

`/Admin/LiveTranslation`

> Note: Authentication and authorization are not yet implemented. Future versions will restrict this page to admin users only.

## תכונות

### זרם אירועים בזמן אמת

כל אות אירועי R מצנרת התרגום מוצגים בטבלה חיה:

- ** מספר מקריות** - מול מונוטוני בתוך כל צינור
- **Timestamp** — Local time when the event was received
- ** Run ID** - קיצור של GUID
- **Stage** - תג שלב פילין (CheckServers, TranslationCountries, וכו ')
- **Type** - סוג ההודעה תג (StageStarted, Progress, StageCompleted וכו ')
- **Message** - תיאור אנושי לקריאה
- **Details** - תשלום מלא של נתונים לאירוע

### צבע coding

צבע
|-------|---------|
כחול ()
ירוק ()
אדום ()
לבן (default)

### מצב

דגל סטטוס במופעים המובילים:
- **Connecting** - יצירת קשר אותות
- **Connected** — Receiving events normally
- **חיבור ** - חיבור אבוד, מנסה להתחבר מחדש
- **Disconnected** — Connection closed

החיבור משתמש להתחבר מחדש אוטומטית עם backoff אקספוננציאלי: 0s, 2s, 5s, 10s, 30s.

### בקרה

- **Clear Feed** - מסיר את כל ההודעות המוצגות ולאפס את הדלפק
- **Export JSON** - הורד את כל ההודעות שהתקבלו כקובץ JSON לניתוח
- **Message counter** — Shows total number of events received in this session

## אותות רכז R

לוח המחוונים מתחבר ל:

```javascript
const connection = new signalr.HubConnectionBuilder()
    .withUrl("/hubs/localization")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();
```

### הסכם הודעה

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

### אירועים

לוח המחוונים מטפל בכל הערכים:

סוג
|------|---------|
תג כחול
תג ירוק
תג אדום
תג ירוק
תג אדום
תגית:
תג אזהרה

## יישום טכני

### חזרה

- **LocalizationHub** (`/hubs/localization`) — SignalR hub that broadcasts messages to all connected clients
- **ISignalRPublisher** - קיצור של המרכז לשימוש בשירותי תרגום
- **SignalRPublisher**- Default application that increments a Monotonicרצף ו-Switchs

### החזית

- HTML/JS עם Bootstrap 5
- השתמש ב- Microsoft SignalR JavaScript Customer Library (מטען מ- CDN)
- אין הוראות בצד השרת הנדרש להזנת האירוע

### מבנה Page

```
Dita.Server/Pages/Admin/
├── LiveTranslation.cshtml      — Razor page markup + JavaScript
└── LiveTranslation.cshtml.cs     — Page model (empty, data comes via SignalR)
```

## שימוש במהלך פיתוח

1. התחל את Dita יישום Server
2. לנווט כדי
3. Trigger a Translation run (או לחכות ללוח הזמנים או להתקשר ל- API)
4. האירועים מופיעים בזמן אמת
5. השתמש כפתור הייצוא כדי ללכוד עקבות מלאים עבור debugging

## שיפורים עתידיים

שיפורים מתוכננים עבור לוח המחוונים:

- ** Authentication** - הגבלת גישה למשתמשים עם התפקיד
- **Filtering** — Filter events by stage, type, or run ID
- ** ריצות היסטוריות ** - תצוגה הושלמה פועל ממסד נתונים או קובץ יומן
- **Statistics** - תרשימים המציגים ספירות תרגום, שיעורי שגיאה, ושקיפות לאורך זמן
- ** גורמים מנליים** - Buttons כדי להתחיל באופן ידני שלבים ספציפיים של צינורות
- **Configuration** - עריכה ישירות מן המחוונים
- ** ניהול לבנגואז** - View and Edit Languages
- ** תצוגה מקדימה ** - Browse and Search Localization dictionaries

## פתרון בעיות

### דשורד מציג "Failed to connect"

1. בדוק את השרת פועל וזמין
2. בדוק את קונסולת הדפדפן עבור תיקון או שגיאות רשת
3. אישור הוא נוכח
4. ודא שאף חומת אש אינה חוסמת חיבורי WebSocket

### אירועים אינם מופיעים

1. בדוק כי כתובת ה-URL של SignalR בין השרת () ללקוח ()
2. בדוק את לוח הזמנים ניתן
3. ראה יומני השרת עבור שגיאות צינור תרגום
4. בדוק את הדפדפן כרטיסיית רשת עבור הודעות WebSocket

### הודעות הן מתוך סדר

התחום מבטיח להזמין בתוך ריצה אחת. אם הודעות מופיעות מתוך סדר, זה עשוי להצביע:
- מספר רב של צינורות חפיפה (לא צריך לקרות בגלל מנעול סלמר)
- דפדפנים (נסו מרענן את הדף)
