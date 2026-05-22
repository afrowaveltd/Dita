# Tietomallit

Nameavaruudessa määritellään kaikki lokalisointi- ja käännösjärjestelmässä käytetyt datarakenteet API-pyynnöstä/vastauksesta putkijohtoraportteihin ja kojelautakuviin.

## Mallin yleiskatsaus

### Asetukset

#### Automaattinen käännösasetus

Asetukset malli sidottu alkaen . Ohjaa LibreKäännä palvelinyhteys ja putkiston käyttäytymistä.

Omaisuus
|---|---|---|---|
LibreKäännä palvelimen URL
Tarvitaanko API-avain
API-avain
Sovelluksen oletuskieli
Kielet, jotka jätetään kääntämättä
Dokumentaation juurihakemistot
Käytä suunniteltuja putkistoajoja
Viive ennen ensimmäistä ajoa
Pöytäkirja ajojen välillä
LibreKäännä tekstin päätepiste
LibreTranslate tiedoston päätepiste
LibreKäännä kieli päätepiste
LibreKäännä havaitsemispäätetapahtuma
Käännöspyyntöjen välinen viive
HTTP- aikakatkaisu per pyyntö
Onko konfigi ladattu

### LibreTranslate API malleja

#### KäännäRequest → KäännäResult

**Pyynti** Tekstikäännös API-puhelu:

Omaisuus
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
..
..
..
| `ApiKey` | `string?` | `"api_key"` | `null` |
..

**Result** — translation response:

Omaisuus
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### löytää pyyntö → havaita

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### KäännäFileRequest → KäännäFileResult

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### LibreKieli

Yksi kieli päätetapahtumasta:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Putkijohtojen ilmoitusmallit

#### Tarkistusraportti

Palvelimen validointivaiheen tulos:

Omaisuus
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Kääntämisraportti

Sanakirjan/maan käännösvaiheiden tulos:

Omaisuus
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

#### MarkdownKäännyksetRaportti

Markdownin käännösvaiheen tulos:

Omaisuus
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Varastointiraportti

Pysyvien tuotosten lopullinen yhteenlaskeminen:

Omaisuus
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Vaiheraportti <T>

Tavallinen säiliö, joka käärii minkä tahansa ilmoitustyypin vaiheen metatiedolla:

Omaisuus
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
(tulostettu)

### Käännöstyömallit

#### fraseinqueue

Käännösjonon työkappale:

Omaisuus
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

#### Käännös

Kaikissa raporteissa esitetty jäsennelty virhe:

Omaisuus
|---|---|
(kielikoodi, tiedostopolku tai taiteilijanimi)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Yksikielinen käännös

Yksi paikallinen sanakirja:

Omaisuus
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### markdowntranslatable lohko

Poistettu lohko Markdown-asiakirjasta:

Omaisuus
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Tekstien erottelumallit

#### Tekstin sijainti Pyyntö → Tekstin sijainti Vaste

**Request** Sanakirjapohjainen lokalisointi (kirjoitettava):

Omaisuus
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Varo **:

Omaisuus
|---|---|
(alkuperäinen)
(paikallinen)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### TextTranslationRequest → TextTranslationVastaa

**Pyyntö** Dynaaminen käännös (vain lukea):

Omaisuus
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

**Varo **:

Omaisuus
|---|---|
(alkuperäinen)
(käännetty)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Tekstiresoluutiolähde

Tunnisteet, joissa paikallinen/käännetty arvo on ratkaistu:

Arvo
|---|---|
Löydetty kohdekielen paikallisesta sanakirjasta
Löytyi oletuskielisanakirjasta
Ei löytynyt; lisätty oletussanakirjaan
Palauttanut LibreKääntäjä
Palautettu kuin- on ilman ratkaisua

### Jaetut tyypit

#### MaaMääritelmä

Lue vain:

Omaisuus
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Vertailu

Suodatinehto arviointia varten:

Omaisuus
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### Virheilmoitus

Yksinkertainen API-virhekuori:

Omaisuus
|---|---|
| `Error` | `string?` |
