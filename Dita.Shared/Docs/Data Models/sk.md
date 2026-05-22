# Dátové modely

Menový priestor definuje všetky dátové štruktúry používané v rámci lokalizačného a prekladateľského systému od párov API žiadosti/odpovede až po správy z potrubia a snímky palubnej dosky.

## Prehľad modelu

### Nastavenie

#### Automatické preklady

Konfiguračný model viazaný z . Ovláda LibreTranslate pripojenie servera a správanie potrubia.

Vlastnosť
|---|---|---|---|
LibreTranslate URL servera
Či sa vyžaduje kľúč API
Kľúč API
Štandardný jazyk aplikácie
Jazyky vylúčené z prekladu
Dokumentácia koreňové adresáre
Povoliť plánované chody potrubia
Oneskorenie pred prvým spustením
Zápisnica medzi chodmi
LibrePreložiť textový koncový ukazovateľ
LibreTranslate súbor koncový bod
LibreTranslate languages endpoint
LibreTranslat detection endpoint
Oneskorenie medzi žiadosťami o preklad
HTTP timeout na žiadosť
Či konfigurácia bola načítaná

### LibreTranslate modely API

#### preložiť žiadosť → preložiťvýsledok

**Request**

Vlastnosť
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Výsledok**

Vlastnosť
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest → Detekcie

**Request**: **Response**:
**Response**: `{ Language, Confidence }`

#### preložiť žiadosť o súbor → preložiť výsledok súboru

**Request**: **Response**:
**Response**: `{ TranslatedFileUrl }`

#### LibreLanguage

Vstup do jedného jazyka z koncového ukazovateľa:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Vzory správ o plynovodoch

#### Kontrolný výkaz

Výsledok fázy validácie servera:

Vlastnosť
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Správa o prekladoch

Výsledok štádií slovníka/štátneho prekladu:

Vlastnosť
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

#### Name

Výsledok fázy prekladu Markdown:

Vlastnosť
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Správa o ukladaní

Konečné zoskupenie pretrvávajúcich výstupov:

Vlastnosť
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Správa o stupni <T>

Všeobecná nádoba, ktorá zabalí akýkoľvek typ hlásenia metadátami stupňa:

Vlastnosť
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(v prílohe)

### Modely prekladateľských prác

#### FrázaInQueue

Pracovná položka pre front prekladu:

Vlastnosť
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

#### PrekladError

Štruktúrovaný záznam o chybe vo všetkých správach:

Vlastnosť
|---|---|
(jazykový kód, cesta k súboru alebo názov etapy)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Jednoduchý preklad

Jednoduchý lokálny slovník:

Vlastnosť
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### markdowntranslatabableblock

Extrahovaný blok z dokumentu Markdown:

Vlastnosť
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Modely textového rozlíšenia

#### TextLokalizácia Požiadavka → TextLokalizácia Odpoveď

**Request**

Vlastnosť
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response **:

Vlastnosť
|---|---|
(originálny)
(lokalizovaný)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextPrekladPožiadavka → TextPrekladOdpoveď

**Request**

Vlastnosť
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response **:

Vlastnosť
|---|---|
(originálny)
(preložené)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### textový zdroj

Identifikuje, kde bola vyriešená lokalizovaná/preložená hodnota:

Hodnota
|---|---|
Nájdené v lokálnom slovníku pre cieľový jazyk
Nájdené v predvolenom slovníku jazyka
Nenájdený; pridaný do štandardného slovníka
Vrátil sa LibreTranslate
Vrátený späť bez rozlíšenia

### Spoločné typy

#### Definovanie krajiny

Záznam iba na čítanie od:

Vlastnosť
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Podmienky porovnania

Stav filtra pre hodnotenie:

Vlastnosť
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### ChybaResponse

Jednoduchý chybový obal API:

Vlastnosť
|---|---|
| `Error` | `string?` |
