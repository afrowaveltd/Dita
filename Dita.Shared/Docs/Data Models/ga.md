# Sonraí Múnlaí

Sainmhíníonn an t-ainmspás na struchtúir sonraí go léir a úsáidtear ar fud an córas logánaithe agus aistriúcháin - ó API iarraidh / péirí comhfhreagracha le tuarascálacha píblíne agus snapshots Painéal na nIonstraimí.

## Forbhreathnú múnla

### Cumraíocht

#### Socruithe Aistrithe Uathoibríoch

Samhail Cumraíocht cheangal ó . Rialuithe Nasc freastalaí LibreTranslate agus iompar píblíne.

Díroghnaigh gach rud
|---|---|---|---|
LibreTranslate URL freastalaí
Cibé an bhfuil eochair API ag teastáil
Eochair API
Iarratais teanga réamhshocraithe
Teangacha a eisiamh ó aistriúcháin
Eolaithe fréimhe doiciméid
Ritheann píblíne Cumasaithe
Moill roimh an gcéad reáchtáil
Miontuairiscí ar Ritheann
LibreTranslate téacs endpoint
LibreTranslate comhad endpoint
Teangacha LibreTranslate endpoint
Críochnú braite LibreTranslate
Moill idir iarratais aistriúcháin
HTTP timeout in aghaidh an iarratais
Cibé an raibh mearbhall luchtaithe

### LibreTranslate samhlacha API

#### TranslateRequest →

**Request** — aistriúchán téacs API glaoch:

Díroghnaigh gach rud
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Result** — freagra aistriúcháin:

Díroghnaigh gach rud
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DetectRequest →

**Freagrais **: **Freagrais **:
**Response**: `{ Language, Confidence }`

#### TranslateFileRequest → TranslateFileReult

**Freagrais **: **Freagrais **:
**Response**: `{ TranslatedFileUrl }`

#### Teanga na Gaeilge

Iontráil teanga amháin ón bpointe deiridh:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Samhlacha tuarascáil Pipeline

#### Seiceáil Tuairisc

Toradh na céime bailíochtaithe freastalaí:

Díroghnaigh gach rud
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Amharc ar gach eolas

Toradh céimeanna aistriúcháin foclóir/tíre:

Díroghnaigh gach rud
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

#### Nuacht agus Imeachtaí

Toradh na céime aistriúcháin Markdown:

Díroghnaigh gach rud
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### An tSraith Shinsearach

Comhiomlánú deiridh na n-aschur leanúnach:

Díroghnaigh gach rud
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport\<T\>

Coimeádán cineálach a wraps aon chineál tuarascáil le meiteashonraí stáitse:

Díroghnaigh gach rud
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
taiseachas aeir: fliuch

### Samhlacha oibre aistriúcháin

#### Frása InQueu

Mír oibre don scuaine aistriúcháin:

Díroghnaigh gach rud
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

#### Aistriúchán

Taifead earráide struchtúrtha a rinneadh i ngach tuarascáil:

Díroghnaigh gach rud
|---|---|
(cód teanga, cosán comhad, nó ainm stáitse)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Aistriú Aonair

Foclóir locale Aonair:

Díroghnaigh gach rud
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### Cliceáil grianghraf a mhéadú

Bloc bainte as doiciméad Markdown:

Díroghnaigh gach rud
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Múnlaí réitigh téacs

#### TéacsLocalization Iarratas → TextLocalization Plandaí faoi dhíon

**Request** — logánú foclóra-bhunaithe (writable):

Díroghnaigh gach rud
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Freagrais **:

Díroghnaigh gach rud
|---|---|
(bunaidh)
(áitithe)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Téacs Aistriú Freagra

**Request** — aistriúchán dinimiciúil (léamh amháin):

Díroghnaigh gach rud
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Freagrais **:

Díroghnaigh gach rud
|---|---|
(bunaidh)
taiseachas aeir: fliuch
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Téacs Réiteach

Aitheantas nuair a réitíodh luach áitiúil/aistrithe ó:

Luach
|---|---|
Bunaithe i foclóir locale don sprioctheanga
Bunaithe san Fhoclóir réamhshocraithe
Gan fáil; curtha leis an bhfoclóir réamhshocraithe
Ar ais ag LibreTranslate
Ar ais mar-is gan réiteach

### Cineálacha roinnte

#### Inis dúinn, le do thoil..

Léigh ar aghaidh ó:

Díroghnaigh gach rud
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Comhdhearcadh

Coinníoll Scagaire le haghaidh meastóireachta:

Díroghnaigh gach rud
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Riachtanais uisce: measartha

Clúdach earráide API Simplí:

Díroghnaigh gach rud
|---|---|
| `Error` | `string?` |
