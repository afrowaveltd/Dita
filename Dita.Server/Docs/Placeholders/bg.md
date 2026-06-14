# Назовавани участници в локализацията

Dita поддържа **назовани placeholders ** в локализиране низове, което позволява динамични стойности да бъдат поставени в работно време, като същевременно се запазва правилната граматика на различните езици.

## Синтаксис

Държачите на местата използват синтаксиса в речника JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

За разлика от позиционните placeholders (, ), назовани placeholders са **език-агностик **  готворци могат да ги поръчат да съответстват на целева език граматика, без да се нарушава кода.

## Съхранение

Имената на собствениците имат два източника на стойности:

### 1. Времеви стойности (препоръчва се за динамични данни)

Премини стойности директно при извличане на локализирания низ:

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

### 2. Съхранявани стойности (за полустатична конфигурация)

Управлява файл в директорията:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Запаметените стойности действат като ** по подразбиране ** и са завишени от стойностите за времето за изпълнение.

## Референтен номер на API

### JsonStringLocalizer индексатор

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

### Упражнения на място

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

### Методи за разширяване

За удобство при работа с:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Употреба:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Поведение на превода

Когато услугата за автоматичен превод среща текст с назовани притежатели:

1. ** Преди превод **: Държачи на места са маскирани с безопасни символи (), за да се предотврати промяната им в превода на двигателя.
2. **During translation**: The translation engine processes only the translatable text.
3. **След превод **: Оригиналните имена на титуляра () са възстановени в техните правилни позиции.

### Пример

Източник (английски):

Готови за превод:

Преведено на чешки:

Крайен резултат:

Това гарантира, че:
- Собствениците никога не са превеждани или корумпирани
- Граматиката може свободно да пренарежда заобикалящия текст
- Един и същ шаблон работи правилно на всички езици

## Най-добри практики

1. **Използвайте описателни имена **: е по-добре от или
2. **Запазвайте държателите минимални **: Твърде много собственици правят превода по-труден
3. **Документ очаквани типове **: Коментарите във файла JSON помагат на преводачите да разберат контекста
4. ** За предпочитане стойности на времето за изпълнение **: За наистина динамични данни (имена на потребителите, бройки, дати), подава стойности в работно време
5. **Използвайте съхранени стойности за по подразбиране **: За конфигурация, която рядко се променя (име на приложение, поддръжка на електронна поща)
6. ** Validate placeholders **: Използване за проверка на всички очаквани титуляри

## Интеграция с автоматичен превод

Автоматичното управление на съхранение на място по време на LiberTranslate повиквания. Не е необходима допълнителна конфигурация.

И двете използват услугата retri, така че всички преводи JSON речник прозрачно подкрепа на името на собствениците.

## Назад

Съществуващ код с използване на позиционни притежатели или не, притежателите на места продължават да работят без промяна:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Наименованието placeholder API е добавка  го не нарушава съществуващото използване.
