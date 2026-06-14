# Завірені особи в локалізації

Dita supports **named placeholders** in localization strings, allowing dynamic values to be inserted at runtime while preserving correct grammar across languages.

## Синтаксис

Власники програми використовують синтаксис кривих у значеннях словника JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

На відміну від адресних власників місця (, ), іменованих власників місць **language-agnostic** — перекладачі можуть переадресувати їх, щоб відповідати цільовій граматиці без розриву коду.

## Зберігання

Завірені особи мають два джерела значень:

### 1. Значення пробігу (рекомендовані для динамічних даних)

Передача значень безпосередньо при перерозподілі локалізованого рядка:

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

### 2. Збережені значення (для напівстатичного налаштування)

Управління файлом в каталозі:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Зберігайте значення, як **Defaults** і перейменуйте за значеннями runtime.

## Довідка API

### JsonStringLocalizer індексатор

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

### Статус на сервери

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

### Методи розширення

Для зручності при роботі з :

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Використання:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Поведінка перекладу

Коли автоматична служба перекладу зустріне текст з іменними власниками:

1. **Переклад**: Власники розміщуються на безпечних жетонах () для запобігання їх модифікації.
2. **During translation**: Процеси перекладу тільки перекладеного тексту.
3. **Після перекладу**: Оригінальні імена власників () реставруються в своїх правих положеннях.

### Приклад

Джерело (Ukrainian):

Підготовка до перекладу:

Translated до чеська:

Остаточний результат:

Це забезпечує:
- Власники не переведені або пошкоджені
- Цільова граматика може вільно змінювати навколишній текст
- Те ж саме шаблон працює правильно на всіх мовах

## Кращі практики

1. **Використовувати дескриптивні імена**: краще або
2. **Кеп-резиденти мінімальні**: Занадто багато власників сайтів роблять переклад harder
3. **Документ очікуваних типів**: Коментарі до JSON файл допомагають перекладачам зрозуміти контекст
4. **Prefer значень runtime**: Для справді динамічних даних (користувачів, підрахунків, дат), пропускають значення в режимі runtime
5. **Використовувати збережені значення для за замовчуванням**: Для налаштування, які рідко змінюють (податкове ім'я, support email)
6. **Validate placeholders**: Використовуйте для перевірки всіх зацікавлених сторін

## Інтеграція з автоматичним перекладом

При збереженні вкладів ЛібреТранслата автоматично керує збереженням коштів. Немає додаткових налаштувань.

І як використовувати службу птиху, так і всі переклади словників JSON прозоро підтримують іменовані держателі.

## Зворотна сумісність

Виконуючи код, використовуючи представницькі власники або нерезиденти, продовжують працювати незмінно:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Названий API-резидента — це не розрив існуючого використання.
