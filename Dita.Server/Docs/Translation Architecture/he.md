# אדריכלות תרגום

מסמך זה מתאר את האדריכלות המודולרית של מערכת התרגום האוטומטית של דיטה, שהוצגה לשיפור התחזוקה, הרגישות והחוסנות.

## מטרות עיצוב

השינוי התייחס לכמה חששות עם העיצוב המונוליטי המקורי:

- **Separation of concerns**: Each translation domain (countries, JSON dictionaries, Markdown) is isolated.
- ** התעקשות מוגברת**: קבצים נשמרים בשפה מיד לאחר התרגום, צמצום השימוש בזיכרון ומספקים תוצאות קודמות.
- ** עמידות **: רמות מרובות של retry מטפלות בכישלונות transient מבלי לחסום את כל הצינור.
- **Observability**: כל פעולה משמעותית מדווחת באמצעות SignalR עבור ניטור בזמן אמת.
- ** אפשרות **: ניתן להוסיף מטרות תרגום חדשות על ידי יישום ממשק יחיד.

## המונחים:

### Backend Translationion Service (orchestrator)

** אחריות **
- ניהול מחזור חיים פיפיר (start, השלמת, טיפול שגיאה)
- בקרת מסחר המבוססת על Semaphore (prevents חופפים רץ)
- אימות Server (עקביות, זמינות שפה, תצורה)
- המונחים: sub-services

**Does NOT contain**:
- תרגום לוגיקה
- I/O לפורמטים ספציפיים
- לוגיקה מחדש

### מדינות תרגום

** אחריות **
- קרא מהבמאי
- שמות מדינה לסנכרון לתוך מילון ברירת המחדל
- תרגום שמות המדינה החסרים לשפת היעד
- שמור את כל מילון היעד מיד לאחר התרגום

**Key behaviors**:
- אם שפת ברירת מחדל היא אנגלית: שמות מדינה מאוחסנים כ-is
- אם שפה ברירת מחדל היא אחרת: שמות אנגליים מתורגם לשפה ברירת מחדל
- כל שפה מעובדת באופן עצמאי עם לולאה מחדש שלה

### שירות תרגום

** אחריות **
- Detect הוסיף/removed מפתחות על ידי השוואת מילון ברירת המחדל הנוכחי עם תמונה קודמת
- תרגום מפתחות לכל שפה
- הסרת מפתחות מכל שפת מטרה
- שמור תמונה להשוואה הבאה

**Key behaviors**:
- תרגום ידני תמיד צריך עדיפות (לעולם לא נכתב)
- מפתחות נוספים מתורגמים ומנצלים כל שפה באופן מיידי
- הסרת המפתחות נמחקים באופן מיידי
- Snapshot נשמר רק אחרי כל השפות

### מסמכים תרגום

** אחריות **
- הליכה להגדיר מחדש את השורשים
- Detect שינה קבצים מקור באמצעות SHA-256 hashes
- המונחים: per-block Translation Status
- תרגום בלוק-by-block with per-block retry
- מבנה גילוח לאחר התרגום
- שמור את כל קובץ שפת היעד באופן עצמאי

**Key behaviors**:
- גרפיות ברמת בלוק: כותרות, פסקאות, פריטים ברשימה מתורגמים בנפרד
- מסלולים של מטא-נתונים שחוסנים הצליחו / מזוהים לשפה
- בלוקים כושלים משוחזרים לריצה הבאה מבלי לעבור מחדש בלוקים מוצלחים
- אימות מבנה מבטיח ספירות כותרות, רשימות, בלוקים קוד וכו '

## אסטרטגיית Retry

המערכת מיישמת חזרות בשלושה רמות:

### רמה 1 - HTTP (Libre Translatione Service)

- עד 5 ניסיונות עם חזרה אקספוננציאלית (1s, 2s, 3s, 4s, 5s)
- דפי רשת Timeouts, 5xx שגיאות, וכישלונות חולפים
- נבנה לתוך תצורה של לקוח HTTP

### דרגה 2 - שלב (תרגום חופשי)

- עד 3 ניסיונות עם עיכובים של 30 שניות
- להפעיל מחדש את כל בקשת התרגום לאחר HTTP-level Retries מותשת
- המסיכה והשיקום מוחלים ברמה זו

### רמה 3 - בלוק (Documents Translationion Service)

- בלוקים בודדים שאינם מסומנים ב metadata
- עקבו אחרי The Next tube run
- בלוקים מוצלחים אף פעם לא עוברים מחדש

## זרימת נתונים

### תרגום של JSON

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

### תרגום לעברית

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

### שם המדינה תרגום

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

## עקשנות המדינה

### תמונות

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Enables incrementalSync על ידי מעקב אחר מה שהיה נוכח בריצה הקודמת

### קבצים

- **Markdown**: ליד קובץ המקור
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Detects source changes to avoid unnecessary re-translation

### תרגום metadata

- **Markdown**: `{sourceFile}.translation-meta.json`
- **המשך **:
  - מקור התוכן Hash
- סטטוס בלוק שפה (array of booleans)
- עדכון אחרון
- **Purpose**: Enables partial re-translation of only failed blocks

### מקום אחסון

- **File**:
- **contents **: מילון מפתחות למקם שם-ערך זוגות
- **Purpose**: מספק ערכי ברירת מחדל עבור בעלי מקומות שונים ברחבי היישום

## דיווח SignalR

### המונחים:

שירותי תרגום של אותות:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### המונחים:

- הודעות בתוך ריצה אחת הן רצף מונוטוני
- מספרי שקיפות הם ייחודיים לריצה באמצעות
- לקוחות יכולים לזהות פערים או לתקן מחדש

### מיפוי

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## נקודות הרחבה

### הוספת יעד תרגום חדש

1. ליצור ממשק חדש עם
2. ליישם את הממשק עם לוגיקה ספציפית דומיין
3. תגית: DI מכולה
4. כניסה ל-Buildor
5. קריאה לאחר שלבים קיימים

### מדיניות retry

המונחים:

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

### מטפל במקום

יישום שינוי syntax או אחסון:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## המונחים:

### תגית:json

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

### זמן ריצה

קביעת
|---------|---------|--------|
80
10
3
30

## אסטרטגיות בדיקה

### בדיקות יחידה

כל שירות תת-שירות הוא באופן עצמאי:

- הצצה להצלחה / Filure
- Mock כדי לאמת את הדיווח
- השתמש במדריכים זמניים לקובץ I/ O O
- בדוק את התנהגות החיסכון בשפה

### בדיקות אינטגרציה

- צינורות מלאים לרוץ עם אמת (local) Libreתרגםe
- בדוק הודעות SignalR מועברות ללקוחות מחוברים
- מבחן מניעה זמנית (semaphore)
- מבנה גילוח לאחר התרגום

### בדיקות מקצה לקצה

- תרגום טריגר באמצעות API או לוח הזמנים
- בדוק את כל קבצי שפת היעד נוצרים /
- קבצי metadata מכילים מצב בלוק נכון
- בעלי המקום נשמרים בתרגומים

## שיקולים

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **דיסק I/O**: קבצי Metadata מוסיפים ראש קטן אך מאפשרים עבודה מצטברת
- ** Network**: עיבוד חיוני עם התכווט מונע Libreתרגם
- **CPU**: SHA-256 hashing and regation הם מהירים יחסית לתרגום
- **SignalR**: הודעות משקל אור, אין דחיסת תשלום הדרושה לדיווחים טיפוסיים

## הגירה מעיצוב מונוליטי

המקור הכיל את כל ההיגיון בכיתה אחת. נתיב ההגירה:

1. לוגיקה המדינה
2. לוגיקה JSON
3. המונחים:
4. הוצאה לאור:
5. חידוש לוגיקה
6. לצטט את התזמורת למשלחת בלבד

כל הממשקים הקיימים נותרו ללא שינוי. הצרכנים של הצינור לא רואים שינויים פורצים.
