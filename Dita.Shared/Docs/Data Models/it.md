# Modelli dati

Lo namespace definisce tutte le strutture di dati utilizzate in tutto il sistema di localizzazione e traduzione — dalla richiesta API/risposta coppie a report pipeline e snapshot dashboard.

## Panoramica del modello

### Configurazione

#### Impostazioni di traduzione automatica

Modello di configurazione legato da . Controlla la connessione e il comportamento del server LibreTranslate.

Proprietà
|---|---|---|---|
URL del server LibreTranslate
Se è richiesta una chiave API
Chiave API
Lingua di default dell'applicazione
Lingue da escludere dalla traduzione
Documentazione directory radice
Attivare le operazioni di tubazione programmate
Ritardo prima della prima corsa
Verbale tra corsa
LibreTranslate testo endpoint
LibreTranslate file endpoint
LibreTranslate lingue endpoint
Finalpoint di rilevamento LibreTranslate
Ritardo tra le richieste di traduzione
Timeout HTTP per richiesta
Se la configurazione è stata caricata

### Modelli API di LibreTranslate

#### TradurreRichiesta → TraduciRisultato

**Richiesta** — traduzione di testo chiamata API:

Proprietà
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Risultato** — risposta di traduzione:

Proprietà
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### RilevamentoRichiesta → Rilevazioni

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### TranslateFileRichiesta → TranslateFileRisultato

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### LibreLanguage

Ingresso linguistico singolo dal punto di vista:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Modelli di report Pipeline

#### controlloreportuale

Risultato della fase di convalida del server:

Proprietà
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### TraduzioniRelazione

Risultato delle fasi di traduzione del dizionario/paese:

Proprietà
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

#### MarkdownTraduzioniRelazione

Risultato della fase di traduzione di Markdown:

Proprietà
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StoccaggioReport

Aggregazione finale delle uscite perseverate:

Proprietà
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport\<T\>

Contenitore generico che avvolge qualsiasi tipo di relazione con metadati di fase:

Proprietà
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(computato)

### Modelli di lavoro di traduzione

#### Condividi su Google

Elemento di lavoro per la coda di traduzione:

Proprietà
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

#### TraduzioneErrore

Registrazione di errore strutturata effettuata in tutte le relazioni:

Proprietà
|---|---|
(codice della lingua, percorso del file o nome della fase)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Traduzioni singole

Dizionario locale singolo:

Proprietà
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownTranslatableBlock

Blocco estratto da un documento Markdown:

Proprietà
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Modelli di risoluzione del testo

#### TestoLocalizzazione Richiesta → TextLocalization Risposta

**Richiesta** — localizzazione basata sul dizionario (writable):

Proprietà
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Proprietà
|---|---|
(originale)
(localizzato)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TestoTraduzioneRichiesta → TextTraduzioneRisponsa

**Richiesta** — traduzione dinamica (solo lettura):

Proprietà
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Proprietà
|---|---|
(originale)
(tradotto)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Risoluzione del testo

Identificare dove è stato risolto un valore localizzato/traslato da:

Valore
|---|---|
Trovato nel dizionario locale per la lingua di destinazione
Trovato nel dizionario di lingua predefinito
Non trovato; aggiunto al dizionario predefinito
Ritornato da LibreTranslate
Ritornato come-è senza risoluzione

### Tipi condivisi

#### PaeseDefinizione

Ingresso in sola lettura da:

Proprietà
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### ConfrontoCondizione

Filtro condizione per la valutazione:

Proprietà
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### ErroreRisposta

Semplice busta di errore API:

Proprietà
|---|---|
| `Error` | `string?` |
