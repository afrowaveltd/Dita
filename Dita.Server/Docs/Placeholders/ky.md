# Жергиликтүү деңгээлдеги жер ээлеринин аты-жөнү

Dita локалдаштыруу жипчелеринде ** аталган орун ээлерин ** колдойт, динамикалык маанилерди иштөө учурунда киргизүүгө мүмкүндүк берет, ошол эле учурда тилдердин ортосунда туура грамматиканы сактайт.

## Синтаксис

Жайгашкан жер ээлери JSON сөздүгүндөгү маанилердин ичинде ийри-буйру жип синтаксисин колдонушат:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Unlike positional placeholders (`{0}`, `{1}`), named placeholders are **language-agnostic** — translators can reorder them to match target-language grammar without breaking the code.

## Сактоо

Аталган жер ээлеринин баалуулуктарынын эки булагы бар:

### 1. Иш убактысынын маанилери (динамикалык маалыматтар үчүн сунушталат)

Локалдаштырылган жипти алуу учурунда маанилерди түздөн-түз өткөрүп бериңиз:

```csharp
// In a Razor page or controller
@inject JsonStringLocalizer Localizer

var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

### 2. Сакталган маанилер (жартылай статикалык конфигурация үчүн)

Каталогдогу файлды башкарат:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Сакталган маанилер ** дефолттор ** катары иштейт жана иштөө убактысынын маанилери менен жокко чыгарылат.

## API шилтемеси

### JsonStringLocalizer индекси

```csharp
// Without placeholders (backward compatible)
LocalizedString text = localizer["SomeKey"];

// With positional formatting (backward compatible)
LocalizedString text = localizer["SomeKey", "arg1", "arg2"];

// With named placeholders (new)
LocalizedString text = localizer["SomeKey", new Dictionary<string, string>
{
    ["name"] = "value"
}];
```

### IPlaholder кызмат

```csharp
public interface IPlaceholderService
{
    // Get stored placeholders for a key
    Dictionary<string, string> GetPlaceholders(string key);
    
    // Set a stored placeholder value
    void SetPlaceholder(string key, string placeholderName, string value);
    
    // Remove all stored placeholders for a key
    void RemoveKey(string key);
    
    // Format a template with placeholders
    string Format(string template, Dictionary<string, string>? values = null);
    
    // Extract placeholder names from template
    string[] ExtractPlaceholders(string template);
    
    // Check if template contains placeholders
    bool HasPlaceholders(string template);
    
    // Prepare text for translation (mask placeholders)
    (string preparedText, Func<string, string> restore) PrepareForTranslation(string template);
    
    // Persist/load from disk
    Task SaveAsync();
    Task LoadAsync();
}
```

### Кеңейтүү ыкмалары

Төмөнкү нерселер менен иштөөдө ыңгайлуулук үчүн:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Колдонуу:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Котормо жүрүм-туруму

Автоматтык котормо кызматы текстти аталган орун ээлери менен жолуктурганда:

1. ** Котормодон мурун**: Жергиликтүү тургундар котормо кыймылдаткычынын аларды өзгөртүүсүнө жол бербөө үчүн коопсуз белгилер менен маскаланган.
2. ** Котормо учурунда**: Котормо кыймылдаткычы котормочу текстти гана иштетет.
3. ** Котормодон кийин**: Түп нускадагы орун ээлеринин ысымдары () туура абалда калыбына келтирилет.

### Мисал

Булак (англисче):

Котормо үчүн даярдалган:

Чех тилине которулган:

Акыркы жыйынтык:

Бул төмөнкүлөрдү камсыз кылат:
- Жергиликтүү тургундар эч качан которулбайт же бузулбайт
- Максаттык тилдеги грамматика айланадагы текстти эркин жайгаштыра алат
- Бир эле шаблон бардык тилдерде туура иштейт

## Эң мыкты практика

1. ** Түшүндүрмө аталыштарды колдонуу **: же
2. ** Отургучтардын минималдуу санын сактоо**: Көптөгөн орун ээлери котормону кыйындатат
3. ** Документтердин күтүлгөн түрлөрү**: JSON файлындагы комментарийлер котормочуларга контекстти түшүнүүгө жардам берет
4. ** Жүргүзүлгөн иштөө убактысынын маанилери**: Чыныгы динамикалык маалыматтар үчүн (колдонуучунун аты-жөнү, саны, даталары), иштөө убактысында маанилерди өткөрүү
5. ** Демейкилер үчүн сакталган маанилерди колдонуу**: Конфигурация сейрек өзгөрөт (тиркеме аталышы, электрондук почта колдоо)
6. ** Сапаттуу орун ээлери**: Бардык күтүлгөн жер ээлерин текшерүү үчүн колдонуу

## Автоматтык котормо менен интеграциялоо

LibreTranslate чалуулары учурунда орун ээсинин сакталышын автоматтык түрдө жүргүзөт. Кошумча конфигурация талап кылынбайт.

"Эки сөз тең ""ретри"" кызматын колдонушат, ошондуктан бардык JSON сөздүктөрү ачык-айкын түрдө аталган орун ээлерин колдойт.".

## Артка шайкештик

Позиционалдык орун ээлерин же орун ээлерин колдонбогон учурдагы код өзгөрүүсүз иштей берет:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

The named placeholder API is additive — it does not break existing usage.
