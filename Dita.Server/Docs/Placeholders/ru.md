# Названы держатели мест в локализации

Dita поддерживает **named placeholders** в строках локализации, позволяя вставлять динамические значения во время выполнения при сохранении правильной грамматики на разных языках.

## Синтаксис

Заполнители используют синтаксис кудрявых лучей внутри значений словаря JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

В отличие от позиционных заполнителей, названные заполнители являются **языково-агностическими ** — переводчики могут переупорядочивать их, чтобы соответствовать грамматике целевого языка, не нарушая код.

## Хранение

Названные заполнители имеют два источника значений:

### 1. Значения времени выполнения (рекомендуется для динамических данных)

Значения прохода непосредственно при извлечении локализованной строки:

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

### 2. Хранимые значения (для полустатической конфигурации)

Управляет файлом в каталоге:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Хранимые значения действуют как **по умолчанию** и перекрываются значениями времени выполнения.

## API ссылка

### Индексатор JsonStringLocalizer

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

### сервис placeholder

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

### Методы расширения

Для удобства при работе с:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Использование:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Поведение переводчика

Когда служба автоматического перевода встречает текст с именами владельцев:

1. **До перевода**: Заполнители маскируются безопасными токенами (), чтобы предотвратить их модификацию механизмом перевода.
2. ** Во время перевода**: Механизм перевода обрабатывает только переводимый текст.
3. **После перевода**: оригинальные имена заполнителей восстанавливаются в правильном положении.

### Пример

Источник (английский):

Готов к переводу:

Перевод на чешский:

Итоговый результат:

Это гарантирует, что:
- Владельцев никогда не переводят и не коррумпируют
- Грамматика целевого языка может свободно переставлять окружающий текст
- Один и тот же шаблон работает правильно на всех языках

## Лучшие практики

1. **Использовать описательные названия**: лучше, чем
2. **Сохраняйте минимальную площадь**: Слишком много заполнителей усложняют перевод
3. **Документ ожидаемых типов**: комментарии в файле JSON помогают переводчикам понять контекст
4. ** Предпочтительные значения времени выполнения**: Для действительно динамических данных (имена пользователей, подсчеты, даты), пройдите значения во время выполнения
5. ** Используйте сохраненные значения для дефолтов**: Конфигурация, которая редко меняется (имя приложения, поддержка электронной почты)
6. ** Подтвержденные заполнители**: Используйте для проверки всех ожидаемых заполнителей

## Интеграция с автоматическим переводом

Автоматически обрабатывает сохранение заполнителя во время звонков LibreTranslate. Никакой дополнительной конфигурации не требуется.

Оба используют службу повторного использования, поэтому все переводы словаря JSON прозрачно поддерживают названные заполнители.

## Обратная совместимость

Существующий код с использованием позиционных заполнителей или без них продолжает работать без изменений:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Названный API-интерфейс является аддитивным — он не нарушает существующее использование.
