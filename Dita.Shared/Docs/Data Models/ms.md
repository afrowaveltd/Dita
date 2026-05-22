# Model Data

Namespace mendefinisikan semua struktur data yang digunakan di seluruh lokalisasi dan sistem terjemahan — dari API request/respon pasangan ke laporan pipa dan snapshot dashboard.

## Sekilas pandang

### Konfigurasi

#### Pengaturan Translasi Automatik

Model konfigurasi dari . Kekontrolan freTranslate koneksi server dan perilaku pipa.

Ciri-ciri
|---|---|---|---|
URL server LibreTranslate
Apakah anak kunci API diperlukan atau tidak
Kunci API
Bahasa baku aplikasi gunjing
Bahasa - Bahasa yang tidak diterjemahkan
Direktori root dokumentasi Dokumentasi Dokumentasi Dokumentasi
Gossip
Lengahan sebelum lari pertama
Minit antara berjalan
Titik akhir teks LibreTranslate
Titik akhir berkas LibreTranslate
Ukraina LibreTranslate language endpoint
Titik akhir deteksi LibreTranslate
Penundaan iuran antara permintaan terjemahan
Had masa tamat HTTP setiap permintaan
Apakah konfigurasi dimuat

### Model API LibreTranslate

#### Terjemahkan Terjemahan → TerjemahResult

**Permintaan** — panggilan terjemahan teks API:

Ciri-ciri
|---|---|---|---|
| `Query` | `string` | `"q"` | `""` |
—
—
—
| `ApiKey` | `string?` | `"api_key"` | `null` |
—

**Result** — translation response:

Ciri-ciri
|---|---|
| `TranslatedText` | `string` |
| `DetectedLanguage` | `Detections` |
| `Alternatives` | `List<string>` |

#### DeteksiProjectRequest → Deteksi

**Request**: `{ Query, ApiKey }`  
**Response**: `{ Language, Confidence }`

#### Diterjemahkan oleh :

**Request**: `{ File (IFormFile), Source, Target, Api_key }`  
**Response**: `{ TranslatedFileUrl }`

#### bahasa libre

Bahasa tunggal dari titik akhir:

```json
{ "code": "en", "name": "English", "targets": ["cs", "de", "fr", ...] }
```

### Model laporan pipa

#### Memeriksa Teleport

Hasil tahap validasi server:

Ciri-ciri
|---|---|
| `AppsettingsLoaded` | `bool` |
| `TranslationServerReady` | `bool` |
| `DefaultLanguage` | `string` |
| `AvailableLanguages` | `string[]` |
| `ServerLatencyMs` | `int` |

#### Terjemahan

Hasil kamus/tahap terjemahan yang lengkap:

Ciri-ciri
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

Hasil tahap terjemahan Markdown:

Ciri-ciri
|---|---|
| `SourceFilesDetected` | `int` |
| `SourceFilesChanged` | `int` |
| `SavedFiles` | `int` |
| `SkippedFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### Amerika Serikat

Agregasi akhir dari output yang terus-menerus:

Ciri-ciri
|---|---|
| `RunStartedUtc` | `DateTime` |
| `RunCompletedUtc` | `DateTime?` |
| `SavedDictionaryFiles` | `int` |
| `SavedMarkdownFiles` | `int` |
| `SavedHashFiles` | `int` |
| `TempFallbackWrites` | `int` |
| `Errors` | `List<TranslationError>` |

#### StageReport\<T\>

Kontainer generik yang membungkus setiap jenis laporan dengan metadata tahap:

Ciri-ciri
|---|---|
| `ReportedStage` | `ProcessStage` |
| `StageData` | `T?` |
| `StageStartTime` | `DateTime?` |
| `StageEndTime` | `DateTime?` |
| `StageDuration` | `TimeSpan?` (computed) |

### Model-model kerja terjemahan Terjemahan Terjemahan Baru

#### Frasa Frasa Dalam Baris Gilir

Barang kerja untuk antrian terjemahan:

Ciri-ciri
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

#### Terjemahan Terjemahan Terjemahan:Error

Catatan kesalahan struktur yang dibawa dalam semua laporan:

Ciri-ciri
|---|---|
(kode bahasa, jalur berkas, atau nama panggung)
| `Code` | `ErrorCode` |
| `ErrorMessage` | `string` |

#### Perpindahan Tunggal

Kamus lokal CVS:

Ciri-ciri
|---|---|
| `Language` | `string` |
| `Translations` | `Dictionary<string, string>` |

#### Lapangan Terbang Bawah

Pengekstrakan blok dari dokumen Markdown:

Ciri-ciri
|---|---|
| `Key` | `Guid` |
| `OriginalText` | `string` |
| `TranslatedText` | `string?` |
| `StartLine` | `int` |
| `EndLine` | `int` |
| `BlockType` | `string` |
| `Metadata` | `Dictionary<string, object>` |
| `IsTranslated` | `bool` |

### Model resolusi teks

#### TextLocalization Permintaan → TextLocalization Sambutan

**Request** — lokalisasi berbasis kamus (ditulis):

Ciri-ciri
|---|---|
| `Text` | `string` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** tanggapan **:

Ciri-ciri
|---|---|
nama samaran (asal)
(terlokalisasi)
| `TargetLanguage` | `string` |
| `DefaultLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `AddedToDefaultDictionary` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Air → TeksTranslasi

**Request** — terjemahan dinamis (baca-saja):

Ciri-ciri
|---|---|
| `Text` | `string` |
| `SourceLanguage` | `string?` |
| `TargetLanguage` | `string?` |
| `Values` | `Dictionary<string, string>?` |

** tanggapan **:

Ciri-ciri
|---|---|
nama samaran (asal)
(terjemahan)
| `SourceLanguage` | `string` |
| `TargetLanguage` | `string` |
| `FoundInTargetDictionary` | `bool` |
| `TranslationServerUsed` | `bool` |
| `ResolvedFrom` | `TextResolutionSource` |

#### Sumber sumber

Identifikasi dimana nilai lokalisasi/terjemahan diselesaikan dari:

Nilai
|---|---|
Kamus lokal untuk bahasa target
Klien bagi kamus bahasa baku
Pangkalan data untuk digunakan pada pelayan kamus
Dikembalikan oleh LibreTranslate
Kembali sebagai-adalah tanpa resolusi

### Jenis Kongsi

#### Definisi Negara

Entri baca-saja dari :

Ciri-ciri
|---|---|---|
| `Name` | `string` | `"name"` |
| `DialCode` | `string` | `"dial_code"` |
| `Emoji` | `string` | `"emoji"` |
| `Code` | `string` | `"code"` |

#### Perbandingan

Syarat penapisan untuk evaluasi:

Ciri-ciri
|---|---|
| `Compare` | `Comparison` |
| `Values` | `int[]` |
| `IsOr` | `bool` |

#### respons kesalahan alias

Sampul galat API Sederhana:

Ciri-ciri
|---|---|
| `Error` | `string?` |
