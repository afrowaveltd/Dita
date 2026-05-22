# Podatkovni modeli

Imenski prostor opredeljuje vse podatkovne strukture, ki se uporabljajo po sistemu lokalizacije in prevajanja – od API zahtevkov/odgovorov parov do poročil o cevovodih in posnetkov armaturne plošče.

## Pregled modela

### Nastavitev

#### Samodejni prevodNastavitve

Nastavitev modela vezana iz . Nadzor LibrePrevaja povezavo strežnika in ravnanje cevovoda.

Lastnost
|---|---|---|---|
LibrePrevajaj URL strežnika
Ali je potreben ključ API
Ključ API
Privzeti jezik programa
Jeziki za izključitev iz prevoda
Osnovne mape dokumentacije
Omogoči načrtovane cevovode
Zakasnitev pred prvim zagonom
Zapisnik med poteki
LibrePrevedena končna točka besedila
Opazovana zadeva LibreTranslate
Opazovana točka LibrePrevedeni jeziki
Opazovana točka detekcije LibrePrevedel
Zamuda med zahtevki za prevod
Zakasnitev HTTP na zahtevo
Ali je bil konfig naložen

### LibreTranslate API modeli

#### translaterequest → translaterezultat

**Zahteva** – prevod besedila API klic:

Lastnost
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Result** – odgovor na prevod:

Lastnost
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### ZaznajZahtevo → Odkrivanje

**Zahteva**: **Odgovor**:
**Response**: `{ Language, Confidence }`

#### translatefilerequest → translatefilerezultat

**Zahteva**: **Odgovor**:
**Response**: `{ TranslatedFileUrl }`

#### LibreJezik

Vnos v enem jeziku od končne točke:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Modeli poročil o cevovodih

#### Poročilo o preverjanju

Rezultat faze potrditve strežnika:

Lastnost
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Poročilo o prevodih

Rezultat slovarja/faze prevajanja države:

Lastnost
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

#### MarkdownPoročilo o prevodih

Rezultat faze prevajanja Markdown:

Lastnost
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Poročilo o skladiščenju

Končna agregacija trajnih izhodov:

Lastnost
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Poročilo o fazi <T>

Splošna posoda, ki ovija katero koli vrsto poročila z odrskimi metapodatki:

Lastnost
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(computed)

### Delovni modeli prevodov

#### frazeinqueue

Delovna točka za prevajalsko vrsto:

Lastnost
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

#### PrevodError

Strukturiran zapis o napakah v vseh poročilih:

Lastnost
|---|---|
(jezikovna koda, pot datoteke ali ime odra)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Enotni prevod

Slovar posameznih krajev:

Lastnost
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### MarkdownPrevajajBlock

Izvlečen blok iz dokumenta Markdown:

Lastnost
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Modeli za reševanje besedila

#### Lokacija besedila Zahteva → Lokacija besedila Odziv

**Zahteva** – lokalizacija, ki temelji na slovarju (pisno):

Lastnost
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Odziv**:

Lastnost
|---|---|
(izvorni)
(lokalizirano)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Prevajanje besedilaZahteva → Prevajanje besedilaOdziv

**Zahteva** – dinamičen prevod (samo za branje):

Lastnost
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** Odziv**:

Lastnost
|---|---|
(izvorni)
(prevedeno)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Vir rešitve besedila

Označuje, kje je bila lokalizirana/prevedena vrednost odpravljena iz:

Vrednost
|---|---|
Najdeno v krajevnem slovarju za ciljni jezik
Najdeno v slovarju privzetega jezika
Ni moč najti; dodano v privzet slovar
Vrnjen z LibrePrevedel
Vrnjen kot je brez ločljivosti

### Skupne vrste

#### Opredelitev države

Samo za branje iz:

Lastnost
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Primerjalni pogoji

Stanje filtra za vrednotenje:

Lastnost
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### NapakaOdziv

Enostavna ovojnica za napake API:

Lastnost
|---|---|
| `Error` | `string?` |
