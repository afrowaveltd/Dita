# Models de dades

L' espai de noms defineix totes les estructures de dades usades a través de la localització i del sistema de traducció API/response parells per canonades i instantànies del tauler.

## Resum del model

### Configuració

#### Arranjament de la reducció automàtica

El model de configuració està vinculat a . Controla la connexió amb el servidor Libretrate i el comportament de canonada.

Propietat
|---|---|---|---|
URL del servidor Libretrate
Si es requereix una clau API
Clau API
Idioma per omissió de l' aplicació
Idiomes a excloure de la traducció
Carpeta arrel de la documentació
Habilita les curses de canonades planificades
Retard abans de la primera execució
Minuts entre execució
Punt d' acabament de text Libere
Punt d' acabament del fitxer Libretrage
Punt final de l' idioma de Librescue
Detecció del punt final de la Libree
Retard entre les peticions de traducció
Temps d' espera HTTP per petició
Si s' ha carregat la configuració

### Models API Libretrate

#### Traduïu l'  Translate TradueixResult

**Request** — text translation API call:

Propietat
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
1]
1]
1]
| `ApiKey` | `string?` | `"api_key"` | `null` |
1]

**Result** — translation response:

Propietat
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### Detecció de DetectRequest

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### Traduïu fitxerRequest  TranslateFileResult

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### LibreLanguge

Entrada d' idioma única des del punt d' acabament:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Models d'informe de conducte

#### S' està comprovant l' informe

Resultat de l' etapa de validació del servidor:

Propietat
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Traducció

Resultat de les fases de traducció al diccionari/austisme:

Propietat
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

#### MarkdowntrationsInformation

Resultat de l' escenari de traducció enrere:

Propietat
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### S' està desant l' informe

Agregació final de sortides persisteixdes:

Propietat
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport\<T\>

Un contenidor genèric que ajusta qualsevol tipus d'informe amb metadades de l'escenari:

Propietat
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(computat)

### Models de treball de traducció

#### FraseInQue

Element de treball per a la cua de traducció:

Propietat
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

#### Error de traducció

Registre d' error estructurat usat en tots els informes:

Propietat
|---|---|
(codi en llengua, camí de fitxer o nom de l'escenari)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Traduïció única

Diccionari local simple:

Propietat
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### Bloc comprimitComment

Bloc extret des d' un document Markdown:

Propietat
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Models de resolució de text

#### TextLocalització Sol· licita localització de text Manveen Resposta

**Request** — dictionary-based localization (writable):

Propietat
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Reversió **:

Propietat
|---|---|
(original)
(localitzat)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TexttrationRequest Manveen TextReconductionRespons

**Request** — dynamic translation (read-only):

Propietat
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Reversió **:

Propietat
|---|---|
(original)
( traduir)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Font de textResolution

Identifica d' on es va resoldre un valor localitzat/ traduït de:

Valor
|---|---|
S' ha trobat al diccionari local per a l' idioma de destí
S' ha trobat al diccionari d' idioma predeterminat
No s' ha trobat; s' afegirà al diccionari per omissió
Retituït per Librescape
Retornat tal com està sense resolució

### Tipus compartits

#### PaísDefinition

Entrada de només lectura des de:

Propietat
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Comparació

Condicions de filtratge per a l' avaluació:

Propietat
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### ErrorRevers

Sobre d' error simple de l' API:

Propietat
|---|---|
| `Error` | `string?` |
