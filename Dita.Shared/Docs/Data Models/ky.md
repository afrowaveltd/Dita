# Маалымат моделдери

The `Dita.Shared.Localization.Models` namespace defines all data structures used across the localization and translation system — from API request/response pairs to pipeline reports and dashboard snapshots.

## Моделдин жалпы көрүнүшү

### Конфигурация

#### Автоматтык котормо топтомдору

Конфигурациялык модель. LibreTranslate сервердин туташуусун жана түтүктүн жүрүм-турумун көзөмөлдөйт.

Мүлк мүлк
|---|---|---|---|
LibreTranslate сервери URL
API ачкычы керекпи же жокпу
API ачкычы
Колдонмонун демейки тили
Котормодон четтетүүчү тилдер
Документтердин тамыр каталогдору
Түтүктөрдүн пландаштырылган иштешине шарт түзүү
Биринчи чуркоодон мурун кечиктирүү
Жүргүзүүлөрдүн протоколдору
Libre Котормо тексттин аяктоочу чекити
LibreTransport файлынын аяктоочу чекити
Libre Котормо тилдеринин аяктоочу чекити
LibreTranstection аяктоочу чекити
Котормо өтүнүчтөрүнүн ортосундагы кечиктирүү
Сураныч боюнча HTTP мөөнөтү
Конфиг жүктөлгөнбү же жокпу

### LibreTranslate API моделдерин которуу

#### transrequest → трансрезульт

**Request** — text translation API call:

Мүлк мүлк
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
| `Source` | `string` | — | `"auto"` |
| `Target` | `string` | — | `"en"` |
| `Format` | `string?` | — | `"text"` |
| `ApiKey` | `string?` | `"api_key"` | `null` |
| `Alternatives` | `int` | — | `0` |

**Result** — translation response:

Мүлк мүлк
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → Аныктоо

** Сураныч**: ** Жооп**:
**Response**: `{ Language, Confidence }`

#### transfilerequest → transfileresult

** Сураныч**: ** Жооп**:
**Response**: `{ TranslatedFileUrl }`

#### LibreLanguage тили

Бир тилдеги жазуу:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Түтүктөрдүн отчетунун моделдери

#### Текшерүү отчету

Серверди текшерүү баскычынын жыйынтыгы:

Мүлк мүлк
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Котормолор Баяндама

Сөздүктүн же өлкөнүн котормо баскычтарынын жыйынтыгы:

Мүлк мүлк
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

#### Маркаунт котормолору

Markdown котормо баскычынын жыйынтыгы:

Мүлк мүлк
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Сактоо отчету

Туруктуу өндүрүштөрдүн акыркы агрегациясы:

Мүлк мүлк
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### этаптык отчет <unk>t>

Ар бир отчеттун түрүн этаптуу метамаалыматтар менен ороп турган жалпы контейнер:

Мүлк мүлк
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(компьютердик)

### Котормо жумуш моделдери

#### Сөз айкашы

Котормо кезегинин жумушчу пункту:

Мүлк мүлк
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

#### Котормо катасы

Бардык отчеттордо жүргүзүлгөн структураланган каталар:

Мүлк мүлк
|---|---|
(тил коду, файл жолу же баскычтын аталышы)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Бирдиктүү котормо

Бир жергиликтүү сөздүк:

Мүлк мүлк
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### Markdown Котормо блогу

Markdown документинен алынган блок:

Мүлк мүлк
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Текстти чечүү моделдери

#### Текстти жайгаштыруу Сураныч → Текстти жайгаштыруу Жооп жооп

**Request** — dictionary-based localization (writable):

Мүлк мүлк
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Жооп **:

Мүлк мүлк
|---|---|
(оригиналдуу)
(жергиликтүү)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Текстти которуу өтүнүчү → Текстти которуу

**Request** — dynamic translation (read-only):

Мүлк мүлк
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Жооп **:

Мүлк мүлк
|---|---|
(оригиналдуу)
(котормо)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Тексттин чечилиши булагы

Локализацияланган же которулган маанинин кайдан чечилгенин аныктайт:

Баа мааниси
|---|---|
Максаттык тилдин жергиликтүү сөздүгүндө табылган
Демейки тил сөздүгүндө табылган
Табылган жок; демейки сөздүккө кошулган
LibreTranslate тарабынан кайтарылган
Резолюциясыз кайтарылган

### Биргелешкен түрлөр

#### Өлкө аныктамасы

Окуу үчүн гана жазуу:

Мүлк мүлк
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Салыштырмалуу абал

Баалоо үчүн чыпкалоочу шарт:

Мүлк мүлк
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Ката Жооп берүү

Жөнөкөй API ката конверти:

Мүлк мүлк
|---|---|
| `Error` | `string?` |
