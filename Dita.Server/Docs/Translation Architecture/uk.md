# Архітектура перекладу

Цей документ описує модульну архітектуру системи автоматичного перекладу Dita, яка введена для підвищення стійкості, стійкості та стійкості.

## Завдання дизайну

Рефакторинг адресований кілька питань з оригінальним монолітним дизайном:

- **Сепарація турбот**: Виділяється кожен домен перекладу (countries, JSON dictionaries, Markdown).
- **Підвищений опір**: Файли зберігаються з-замовленістю відразу після перекладу, зменшення використання пам'яті та надання раніше результатів.
- **Resilience**: Multiple retry levels handle transient failures without blocking the entire pipeline.
- **Observability**: Every significant operation is reported via SignalR for real-time monitoring.
- **Екстенсивність**: Нові цілі перекладу можуть бути додані шляхом реалізації єдиного інтерфейсу.

## Служба декомпозиції

### BackendTranslationService (абочистий)

**Особливості**:
- Управління життєвим циклом трубопроводів (починання, завершення, обробка помилок)
- Контроль температури на основі Semaphore (попереджає перекриття)
- Перевірка сервера (відсутність, наявність мови, налаштування)
- Делегація до субсервісів

**Does NOT contain**:
- Логіка перекладу
- Файл I/O для специфічних форматів
- Логічна логіка

### Послуги

**Особливості**:
- Читати з каталогу
- Синхронізувати назви країни в словнику за замовчуванням
- Переклад відсутніх назв країни на цільову мову
- Заощаджуйте кожного цільового словника відразу після перекладу

**Key поведінка**:
- Якщо мова за замовчуванням англійська: назви країни зберігаються як-is
- Якщо мова за замовчуванням є іншою мовою: англійські імена перекладені на мову за замовчуванням
- Кожна мова обробляється самостійно з власною петлею

### ЛокалізаціяТрансляціяСервіс

**Особливості**:
- Видалити додаткові ключі, порівнявши поточний словник за замовчуванням з попереднім знімком
- Переклад додаткових ключів в кожну мову
- Видалити видалені ключі з кожної цільової мови
- Збережіть знімок для наступного порівняння

**Key поведінка**:
- Посібник перекладачів завжди візьмуть пріоритети (все перезаписано)
- Додані ключі переведені і збережені мови відразу
- Вилучені ключі видаляються з мови відразу
- Знімок збережено тільки після завершення всіх мов

### Документи

**Особливості**:
- Прогулянка налаштована відмітка коренів прямо
- Виявлення змінних вихідних файлів за допомогою SHA-256
- Відстеження статусу перекладу в режимі онлайн
- перевести блок-by-block з переблоком
- Важна структура розмітки після перекладу
- Заощаджуйте кожен файл цільової мови самостійно

**Key поведінка**:
- Зменшення рівня блоку: заголовки, абзаци, елементи списку переведені окремо
- Метадані треки, що блоки досяглися/заповнюються на мову
- Змішані блоки перерозподіляють на наступному етапі без перерозподілу успішних блоків
- Введення структури забезпечує заголовки, списки, блоки коду тощо

## Стратегія дерматології

Система реалізує ретери на трьох рівнях:

### Рівень 1 — HTTP (LibreTranslateService)

- До 5 спроб з відключенням (1s, 2s, 3s, 4s, 5s)
- Зручності у мережі, помилки 5xx, переходові несправності
- Вбудована в налаштування HTTP-клієнта

### Рівень 2 — Етап (ТрансляціяRetryService)

- До 3 спроб з 30-другими затримками
- Витягувати весь запит перекладу після закінчення терміну дії HTTP
- На даному рівні наноситься маскування та відновлення

### Рівень 3 — Блок (DocumentsTranslationService)

- Індивідуальні блоки розмітки, які не позначені метаданих
- Вийшов автоматично на наступний хід трубопроводу
- Успішні блоки ніколи не перевантажуються

## Потік даних

### JSON переклад словника

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

### Переклад розміток

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

### Переклад назв країни

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

## Державна наполегливість

### Знімки

- **JSON**: Stored in a file next to the default dictionary (name varies by storage provider)
- **Purpose**: Увімкнено початкову синхронізацію, відстежуючи те, що був присутній у попередньому режимі

### Hash файли

- **Markdown**: поруч із вихідним файлом
- **Fallback**: `{tempDir}/{sanitizedPath}.hash.json` if primary location is read-only
- **Purpose**: Зміни джерела Detects, щоб уникнути зайвих переїздів

### Переклад метаданих

- **Markdown**: `{sourceFile}.translation-meta.json`
- **Контенти**:
  - Джерело вмісту хеш
- Статус на сервери
- Останнє оновлення timestamp
- **Purpose**: Увімкнено часткове ретрансляції тільки нездійснених блоків

### Зберігання власників

- **File**: `Locales/placeholders.json`
- **Contents**: Словник ключів для розміщення пар із іменами акціонерів
- **Purpose**: Забезпечує значення за замовчуванням для іменованих власників сайтів через додаток

## Звіт про реєстрацію

### Референція видавців

послуги перекладу з специфікацій SignalR:

```csharp
public interface ISignalRPublisher
{
    Task PublishStageAsync<T>(Guid runId, ProcessStage stage, T data, ...);
    Task PublishMessageAsync(Guid runId, LocalizationMessageType type, ProcessStage stage, ...);
}
```

### Гарантія якості

- Повідомлення в рамках одноступеневого етапу
- Найпопулярніші номери
- Клієнти можуть виявити зазори або переадресацію

### Хаб мапінг

```csharp
app.MapHub<LocalizationHub>("/hubs/localization");
```

## Розширення точок

### Додавання нової мети перекладу

1. Створення нового інтерфейсу з
2. Реалізувати інтерфейс з логічною логікою
3. Реєстрація в контейнері
4. Введення в конструктор
5. Зв'язатися з існуючими етапами

### Політика конфіденційності

Параметри конструктора Override:

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

### Індивідуальне обслуговування клієнтів

Впровадження зміни синтаксису або сховища:

```csharp
public class CustomPlaceholderService : IPlaceholderService
{
    // Use {{name}} syntax instead of {name}
    // Store in database instead of JSON file
    // Encrypt sensitive placeholder values
}
```

## Налаштування

### додатки.json

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

### Тренінги

Налаштування
|---------|---------|--------|
80 р
10 хв
3 хв
30 хв

## Стратегія тестування

### Тести

Кожен субсервіс самостійно тестується:

- Мок для імітації успіху /failure
- З метою перевірки звітності
- Використовуйте тимчасові каталоги для файлу I/ О
- Перевірити поведінку про збереження мови

### Інтеграційні тести

- Повний трубопровід з реальним (локальним) екземпляром LibreTranslate
- Перевірити повідомлення SignalR до підключених клієнтів
- Профілактика поточної дії (semaphore)
- Важна структура розмітки після перекладу

### Тести End-to-end

- Переклад тригерів за допомогою API або графіка
- Перевірити всі цільові файли мови створюються/оновлюються
- Перевірити файли метаданих містять правильний статус блоку
- Підтверджувати власників місць у перекладі

## Врахування продуктивності

- **Memory**: Per-language saving prevents holding all dictionaries in memory
- **Disk I/O**: Файли метаданих додають невеликий наклад, але дозволяють безпідставну роботу
- **Network**: Sequential processing with throttling prevents overwhelming LibreTranslate
- **CPU**: SHA-256 хешування та регістрація regex швидко відносно затримки перекладу
- **SignalR**: Легкі повідомлення, не потрібно стиснення завантаження для типових звітів

## Міграція з монолітного дизайну

Визначені всі логіки в одному класі. Шлях міграції:

1. Вилучення логіки країни →
2. Вилучення логіки JSON →
3. Витягувати логіка →
4. Виписка SignalR →
5. Вилучення логіки птиця →
6. Підсилювач для делегування

Всі існуючі інтерфейси () залишаються незмінними. Споживачі трубопроводу не порушують зміни.
