# Моделі даних

Простір імен визначає всі структури даних, що використовуються в системі локалізації та перекладу — від API-запитів/відповідних пар до звітів про трубопроводи та знімків.

## Огляд моделі

### Налаштування

#### АвтоматичніПереклади

Модель конфігурації, обмежена від . Управління підключенням сервера LibreTranslate та поведінкою трубопроводів.

Проживання
|---|---|---|---|
URL сервера LibreTranslate
Чи потрібен ключ API
API ключ
Мова за замовчуванням
Мова для виключення з перекладу
Документація кореневих директорій
Увімкнути регулярні трубопроводи
Прокладка до першого запуску
Протоколи між операціями
LibreTranslate текстова кінцева точка
Кінцева точка файлу LibreTranslate
LibreTranslate Мова кінцевої точки
Кінцева точка виявлення LibreTranslate
Прокладання між запитами перекладу
За запитом
Чи було завантажено конфігурацію

### Моделі API LibreTranslate

#### переклад

**Request** — text translation API call:

Проживання
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
до
до
до
| `ApiKey` | `string?` | `"api_key"` | `null` |
до

**Result** — translation response:

Проживання
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → Виявлення

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### javascript licenses api веб-сайт

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### ЛібреЛангуаж

Один запис мови з кінцевої точки:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Моделі звіту про трубопровід

#### Репортаж

Результат перевірки сервера:

Проживання
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### ПерекладиРепорт

Результат етапів перекладу словника/country:

Проживання
|---|---|
| `DefaultDictionaryExists` | `bool` |
| `DefaultDictionaryCount` | `int` |
| `ToTranslateCount` | `int` |
| `AddedCount` | `int` |
| `RemovedCount` | `int` |
| `SkippedCount` | `int` |
| `TranslatedCount` | `int` |
| `ErrorsCount` | `int` |
| `Errors` | `List<TranslationError>?` |

#### МаркуванняТрансляціїРепорт

Результат роботи:

Проживання
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Портрети

Остаточна агрегація стійких виходів:

Проживання
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### СтендРепорт <T>

Генетичні контейнери, які загортають будь-який тип звіту за допомогою метаданих етапу:

Проживання
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(комп'ютери)

### Моделі роботи перекладу

#### Проксимус

Робочий пункт для черги перекладу:

Проживання
|---|---|
| `Target` | `TranslationTarget` |
| `Key` | `string?` |
| `Phrase` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string` |
| `ChangeRequired` | `PhraseChange` |
| `AddedToList` | `DateTime` |
| `TranslationStart` | `DateTime?` |
| `TranslationEnds` | `DateTime?` |
| `IsTranslated` | `bool` |
| `TranslatedText` | `string?` |

#### Переклад

Структурований запис помилок, що здійснюється в усіх звітах:

Проживання
|---|---|
(мовний код, шлях файлу або ім'я етапу)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Єдине

Один з місцевих словників:

Проживання
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### МаркуванняЗавантажувальногоБлок

Витяжний блок з документа маркування:

Проживання
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Текстові моделі роздільної здатності

#### Налаштування Запит → TextLocalization Відправити

**Запит** — локалізація на основі словника (зважте):

Проживання
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Респонс**:

Проживання
|---|---|
(оригінал)
(розрахований)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### ТексттрансляціяЗапитування

**Request** — dynamic translation (read-only):

Проживання
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Респонс**:

Проживання
|---|---|
(оригінал)
(перекладений)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### JavaScript licenses API Веб-сайт

Визначено, де було вирішено локалізоване/перекладене значення:

Ціна
|---|---|
Знайдений у місцевому словнику для цільової мови
Знайдено у словнику мови за замовчуванням
Не знайдено; додано до словника за замовчуванням
Повернутися до LibreTranslate
Повернутися як-is без дозволу

### Поширені види

#### Українська

Читати далі запис:

Проживання
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Порівняння

Умови фільтра для оцінки:

Проживання
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Помилка

Простий конверт помилок API:

Проживання
|---|---|
| `Error` | `string?` |
