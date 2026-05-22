# Modelos de datos

El espacio de nombres define todas las estructuras de datos utilizadas en todo el sistema de localización y traducción, desde pares de solicitud/respuesta de API hasta informes de tuberías y instantáneas de panel.

## Panorama general

### Configuración

#### Ajustes de traducción automática

Modelo de configuración atado de . Controla la conexión del servidor LibreTranslate y el comportamiento del oleoducto.

Propiedad
|---|---|---|---|
URL del servidor LibreTranslate
Si se requiere una clave de API
Clave de API
Idioma predeterminado de la aplicación
Idiomas que deben excluir de la traducción
Dirección de la documentación
Activar las tuberías programadas
Delay antes de correr
Minutos entre carreras
texto libretranslated text endpoint
archivo libretranslate endpoint
Punto final de traducción libre
Punto final de detección LibreTranslate
Diferencia entre las solicitudes de traducción
HTTP timeout por solicitud
Ya sea que se haya cargado el config

### Modelos de API de LibreTranslate

#### TraducirRequest → Traducir Resultado

**Solicitud** — traducción de texto API llamada:

Propiedad
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

** Resultado** — respuesta a la traducción:

Propiedad
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectarSolicitud → Detección

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### TraducirFileRequest → Traducir

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### LibreIdioma

Entrada de idioma único desde el punto final:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Modelos de informe de tuberías

#### checkreport

Resultado de la etapa de validación del servidor:

Propiedad
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### TraduccionesReport

Resultado de las etapas de traducción del diccionario/país:

Propiedad
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

#### MarkdownTranslationsReport

Resultado de la etapa de traducción de Markdown:

Propiedad
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StoringReport

La agregación final de los productos persistentes:

Propiedad
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport\<T\>

Contenedor genérico que envuelve cualquier tipo de informe con metadatos en estadio:

Propiedad
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(computado)

### Modelos de trabajo de traducción

#### fraseinqueue

Tema de trabajo para la cola de traducción:

Propiedad
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

#### TraducciónError

Registro de error estructurado en todos los informes:

Propiedad
|---|---|
(código de idioma, ruta de archivo o nombre de escenario)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Traducción simple

Diccionario único local:

Propiedad
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownTranslatableBlock

Bloque extraído de un documento Markdown:

Propiedad
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Modelos de resolución de texto

#### TextLocalization Solicitud → TextLocalization Respuesta

**Solicitud**: localización basada en diccionarios (escritos):

Propiedad
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Propiedad
|---|---|
(original)
(localizado)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextoTraducciónSolicitud → TextoTraducciónResponse

**Solicitud** — traducción dinámica (sólo lectura):

Propiedad
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Response**:

Propiedad
|---|---|
(original)
(traducido)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextoResoluciónFuente

Identifica dónde se resolvió un valor localizado/traducido de:

Valor
|---|---|
Encontrado en el diccionario locale para el idioma objetivo
Encontrado en el diccionario de idioma predeterminado
No encontrado; añadido al diccionario predeterminado
Regresado por LibreTranslate
Regresado como es sin resolución

### Tipos compartidos

#### PaísDefinición

Entrada única desde:

Propiedad
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### ComparaciónCondición

Condición de filtro para la evaluación:

Propiedad
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### ErrorResponse

Sobre de error de API simple:

Propiedad
|---|---|
| `Error` | `string?` |
