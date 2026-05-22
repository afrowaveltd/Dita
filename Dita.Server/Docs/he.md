# תוצאות לתרגום אוטומטי

## סקירה

מסמך זה מסכם את כל השינויים שנעשו בשירות התרגום האוטומטי של Dita, כולל אדריכלות המספקת, תכונות חדשות, שיפורים observability ושיפורים ההכללה.

## אדריכלות שינויים

### שירות תרגום מחדש

המונוליטית הופצה לארבעה שירותים מיוחדים המתואמים על ידי תזמורת קלה:

- **Backend Translationion Service** - תזמורת פילין (server אימות, משלחת שלב, טיפול שגיאות)
- **Countries Translationion Service** - שם המדינה סינכרוניזציה (אנגלית)
- ** Localization Translationion Service** - JSON מילון סינכרון (המפתחות שהועברו)
- **Documents Translationion Service** - תרגום מסמכים עם מעקב ברמת בלוק
- **SignalRPublisher** - דיווח בזמן אמת באמצעות SignalR
- **TranslationRetryService** — Stage-level retry with placeholder preservation

### יתרונות

- **Separation of concerns**: Each service handles a single translation domain
- ** אחריות **: שיעורים קטנים יותר קלים להבנה ולמבחן
- ** אפשרות **: ניתן להוסיף מטרות תרגום חדשות באמצעות יישום ממשק
- ** אחריות **: שירותים עצמאיים מספקים בידוד טוב יותר

## תכונות חדשות

### תרגום חי

**Location**: `/Admin/LiveTranslation`

דף ניהול חדש המספק חשיפה בזמן אמת לתוך צינור התרגום:

- מציג את כל האירועים של אותות כפי שהם מתרחשים
- סוגי הודעות קודקוד צבע (כחול = מוקרן, ירוק = שלם, אדום=טרור)
- המונחים: Auto-reconnect
- הודעות נגד ויצוא ל-JSON

### שם: Placemakers

מערכת התאזרחות תומכת כיום בשם בעלי מקומות () לשיפור הדקדוק בשפות שונות:

```csharp
// Usage in code
var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

תכונות:
- ערכי בעלי מקומות הניתנים בזמן ריצה או מאוחסנים ב
- מסיכה אוטומטית / עצירות במהלך התרגום כדי למנוע שחיתות
- Backward תואם את בעלי המיקום הקיימים

### תרגום מובנה

קבצי ההרחבה מתורגמים באופן מצטבר:

- **Per-language saving**: Each target language is saved immediately after translation, reducing memory pressure
- **Block-level tracking**: `.translation-meta.json` tracks translation status per block
- ** retry**: רק בלוקים כושלים מתחדשים על הפרק הבא
- ** מידע מתמשך **: מדינת התרגום שורדת מחדש

### המונחים: retry Logic

שלוש רמות של עמידות:

1. **HTTP retry** (Libreתרגםeservice): 5 ניסיונות עם גיבוי אקספוננציאלי (1s-5s)
2. **Stage retry** (TranslationRetryService): 3 additional attempts with 30s delays
3. **Block retry** (Documents Translationion Service): נכשל בלוקים מחדש בריצה הבאה

### דוח SignalR

דיווח על התקדמות בזמן אמת לכל פעולות הצנרת:

- כל שלב מפרסם אירועים
- התקדמות שפה שפורסמה כאירועים
- אירועי שגיאה כוללים חיבור מפורט (מקור, קוד שגיאה, הודעה)
- מספר מקריות מבטיח להזמין בתוך כל ריצה

## שינויים

### תגית:json

לא לשבור שינויים. התצורה הקיימת ממשיכה לפעול:

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

### שירותים חדשים

רשום:

- /
- `TranslationRetryService`
- /
- /
- /
- /

מרכז אותר ממפה לחיבורי לקוחות.

## בדיקות

### מבחן מצב

- **243/244 בדיקות העוברות** (1 לדלג עקב גישה לקובץ במקביל בסביבת הבדיקה)
- סיקור חדש נוסף ל:
  - פונקציונליות
  - תרגום לעברית
  - JsonString Localizer Place indexers

### הגבלות ידועות

- הבדיקה מוזנחת בעת ריצה במקביל, כי מספר מקרי מבחן חולקים את אותו קובץ. זה עובר כאשר לרוץ בבידוד.

## מבנה קובץ חדש

### שירותים

- תגית: Pipeline Orchestrator
- שם המדינה
- ג'ייסון מילון סינכרוניזציה
- תרגום מובנה
- הודעה מיידית
- לוגיקה חוזרת עם מסיכה
- ממשק Publisher
- ממשק שירות המדינה
- ממשק שירות מקומיות
- ממשק שירות המסמכים
- ממשק Orchestrator (upated)
- המונחים: Per-file Translation metadata

### שירותים מעודכנים

- המונחים: placekeeper
- עדכון לפרמטר חדש
- ניהול בעלי מקומות
- ממשק בעלי מקומות

### New Admin Page

- דף ניטור בזמן אמת
- מודל Page

### מסמך חדש

- תיעוד צינורות מעודכן
- שם: Placekeeper
- המונחים: Dashboard use guide
- אדריכלות טכנית

## חזרה Compatibility

כל השינויים הם תוספת:

- קוד מקומי קיים () עובד ללא שינוי
- עיצוב מיקום () עובד ללא שינוי
- פורמט מילון JSON קיים ללא שינוי
- המבנה הקיים ללא שינוי
- הודעות SignalR משתמשות באותו פורמט

## נתיב הגירה

לא נדרשת הגירה. הרצון הוא פנימי:

1. הישן נשמר כתזכורת ולאחר מכן הוחלף
2. רישום DI עודכן לשימוש בממשקים חדשים
3. כל הצרכנים הקיימים לא רואים שינויים

## שיפור ביצועים

- ** שימוש בזיכרון **: קבצים ניצלו באופן מיידי במקום להחזיק את כולם בזיכרון
- **התמריץ של ה-Faster פועל**: רק שינוי / failed Blos re-translated
- **Better visibility**: Real-time progress helps diagnose slow stages

## שיפורים עתידיים

שיפור מתוכנן:

1. **AI fine-tuning** — Post-machine translation review for phrases > 5 words
2. ** אימות המודעה** - הגבלת דפי ניהול למשתמשים מורשים
3. ** עורך דיסלקטיבי ** - Web UI for Management Localization
4. ** סטטיסטיקות תרגום** - תרשימים המציגים ספירות תרגום ושיעורי שגיאה לאורך זמן
5. ** syntax** - תמיכה בפורמטים חלופיים

## צור קשר

לשאלות או בעיות עם שירות התרגום, נא להפנות את התיעוד המפורט בספריה של כל מודול או ליצור קשר עם צוות הפיתוח.
