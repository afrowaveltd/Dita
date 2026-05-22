# Modeli i të dhënave

Emri hapësirës përcakton të gjitha strukturat e të dhënave të përdorura në të gjithë zonën dhe sistemin e përkthimit ♫ nga çiftet e kërkesës/responzës në raportet e tubacionit dhe videot me stenda.

## Model

### Konfigurimi i Mail

#### Përkthim automatik

Modeli i konfigurimit lidhet me . Kontrollon lidhjen me serverin Libre Translate dhe sjelljen e tubacionit.

Pronësitë
|---|---|---|---|
Libre Translate server URL
Tregon nëse kërkohet një kyç API
Pulsanti API
Gjuha e prezgjedhur e programit
Gjuhët për të përjashtuar përkthimin
Regjistrimet bazë
Aktivizo rrjedhshmërinë e tubacionit të planifikuar
Vonesa para se të niset i pari
Minuta midis ekzekutimeve
Libre Translate tekst
LibreTranslate file fund
Ruaj
Libre Translate
Vonesa midis kërkesave të përkthimit
HTTP
Është

### Modele Libre Translate API

#### Përktheje Rikthimin → Përktheje Rezult

**Request** — text translation API call:

Pronësitë
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
⇩
⇩
⇩
| `ApiKey` | `string?` | `"api_key"` | `null` |
⇩

**Result** — translation response:

Pronësitë
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → Deteksione

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### Përktheje EFileRequest → Përktheje eFileResult

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### Gjuha Libre

Shtimi i gjuhës së vetme nga e fundit:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Modelet e raportit të tubacionit

#### Kontrolli

Rezultati nga server:

Pronësitë
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Raporti i përkthimeve

Rezultati i fazave të përkthimit në fjalorë/vendi:

Pronësitë
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

#### Përmbledhje e interfaqeve

Rezultati i fazës së përkthimit Markdown:

Pronësitë
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Duke magazinuar

Agregimi final i rezultateve të vazhdueshme:

Pronësitë
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Faza e Reportazhit<T>

I përgjithshëm që mbyll çdo lloj raporti me metadata fazë:

Pronësitë
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(pëshpëritur)

### Modeli i punës së përkthimit

#### PhraseInQuee

Punë për:

Pronësitë
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

#### Përkthimi

Rekord i strukturuar gabimi i kryer në të gjitha raportet:

Pronësitë
|---|---|
(kode në gjuhën e shenjave, shtegu i file, ose emri i skenës)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Përkthimi i vetëm

Fjalori i vetëm lokal:

Pronësitë
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### Shënon TranslableBlock

Blloku i nxjerrë nga një dokument i shënuar:

Pronësitë
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Modeli i rezolutës së tekstit

#### Lokale Kërkesë për liblokalizim Përgjigje

**Request** — dictionary-based localization (writable):

Pronësitë
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Pronësitë
|---|---|
(orgjinale)
(lokalizuar)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Rikthimi i Tekstit me translation → Pështjellim teksti

**Request** — dynamic translation (read-only):

Pronësitë
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Pronësitë
|---|---|
(orgjinale)
(përkthyer)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Burimi i zgjidhjes së tekstit

Identifikimi nga ku është zgjidhur një vlerë lokale/përkthyer:

Vlera
|---|---|
U gjet në fjalorin lokal për gjuhën e synuar
U gjet në fjalorin e prezgjedhur të gjuhës
Nuk u gjet, shtohet tek fjalori i paracaktuar
I kthyer nga Libre Translate
U kthye pa zgjidhje

### Të përbashkët

#### Definitimi i vendeve

Shtimi në vetëm lexim nga:

Pronësitë
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Kushti i krahasimit

Gjendja e filtrimit për vlerësimin:

Pronësitë
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Gabim

E thjeshtë gabim

Pronësitë
|---|---|
| `Error` | `string?` |
